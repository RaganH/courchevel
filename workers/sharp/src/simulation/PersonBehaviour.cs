using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Improbable;
using Improbable.Math;
using Improbable.Worker;
using Improbable.Worker.Query;
using Ragan;
using SharpWorker.framework;

namespace SharpWorker.simulation
{
  class PersonBehaviour : IComponentBehaviour<Person>
  {
    private readonly SerializedConnection _conn;
    private readonly Dispatcher _dispatcher;
    private readonly EntityId _entityId;
    private readonly Logger _logger;

    private Coordinates _currentPosition;
    private int _currentWealth;
    private Coordinates _target;
    private ulong _callbackKey;

    public PersonBehaviour(SerializedConnection conn, Dispatcher dispatcher, IComponentData<Person> value, EntityId entityId)
    {
      _conn = conn;
      _dispatcher = dispatcher;
      _entityId = entityId;

      _logger = Logger.WithName(conn, typeof(PersonBehaviour).Name);

      var data = value.Get().Value;
      _currentWealth = data.wealth.current;
      _currentPosition = data.position;

      _target = new Coordinates(0,0,0);
    }

    public void AuthorityChanged(bool hasAuthority)
    {
      if (hasAuthority)
      {
        var entityQuery = new EntityQuery
        {
          Constraint = new AndConstraint(
              new IConstraint[]
              {
                new SphereConstraint(_currentPosition, 1000),
                new ComponentConstraint(Mountain.ComponentId),
              }
            ),
          ResultType = new SnapshotResultType()
        };

        var id = _conn.Do(c => c.SendEntityQueryRequest(entityQuery, 5000));
        _callbackKey = _dispatcher.OnEntityQueryResponse(o => MoveToTarget(id, o));

        Task.Run(() => StartUpdatingWealth());
      }
      else
      {
        //TODO(harry) - stop bascground tasks here.
      }
    }

    private void MoveToTarget(RequestId<EntityQueryRequest> id, EntityQueryResponseOp entityQueryResponseOp)
    {
      if (entityQueryResponseOp.RequestId == id)
      {
        _dispatcher.Remove(_callbackKey);

        if (entityQueryResponseOp.StatusCode != StatusCode.Success)
        {
          _logger.Warn($"Query failed with {entityQueryResponseOp.Message}");
          return;
        }

        var results = entityQueryResponseOp.Result;
        if (results.Any())
        {
          var target = results.Where(r => r.Value.Get<Mountain>().HasValue).ToList();
          if (target.Any())
          {
            _logger.Warn($"Moving {_entityId} towards mountain");
            _target = target.First().Value.Get<Mountain>().Value.Get().Value.position;
            Task.Run(() => MoveToTarget());
          }
        }
      }
    }

    public void Update(IComponentUpdate<Person> componentUpdate)
    {
      var wealthOption = componentUpdate.Get().wealth;
      if (wealthOption.HasValue)
      {
        _currentWealth = wealthOption.Value.current;
      }

      var positionOption = componentUpdate.Get().position;
      if (positionOption.HasValue)
      {
        _currentPosition = positionOption.Value;
      }
    }

    private void MoveToTarget()
    {
      int steps = 100;
      var originalPosition = _currentPosition;
      for (int i = 1; i < steps; i ++)
      {
        var newPosition = new Coordinates{
          X = Lerp(originalPosition.X, _target.X, i, steps),
          Y = Lerp(originalPosition.Y, _target.Y, i, steps),
          Z = Lerp(originalPosition.Z, _target.Z, i, steps),
        };

        var componentUpdate = new Person.Update
        {
          position = newPosition
        };

        _conn.Do(c => c.SendComponentUpdate(_entityId, componentUpdate));

        Thread.Sleep(1000);
      }
    }

    private double Lerp(double original, double target, int currentStep, int steps)
    {
      return original + ((target - original) * (double)currentStep / steps);
    }

    private void StartUpdatingWealth()
    {
      while (true)
      {
        var componentUpdate = new Person.Update
        {
          wealth = new Wealth
          {
            current = _currentWealth+1,
          }
        };

        _conn.Do(c => c.SendComponentUpdate(_entityId, componentUpdate));

        Thread.Sleep(1000);
      }
    }
  }
}
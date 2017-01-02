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
    private readonly Dependencies _deps;
    private readonly SerializedConnection _conn;
    private readonly EntityId _entityId;
    private readonly Logger _logger;

    private Coordinates _currentPosition;
    private int _currentWealth;
    private Coordinates _target;
    private ulong _callbackKey;

    public PersonBehaviour(Dependencies deps, IComponentData<Person> initialData, EntityId entityId)
    {
      _deps = deps;
      _conn = deps.Connection;
      _entityId = entityId;

      _logger = Logger.WithName(_conn, typeof(PersonBehaviour).Name);

      var data = initialData.Get().Value;
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
        _deps.QueryDispatcher.Send(entityQuery, MoveToNearbyMountain);

        Task.Run(() => StartUpdatingWealth());
      }
      else
      {
        //TODO(harry) - stop bascground tasks here.
      }
    }

    private void MoveToNearbyMountain(EntityQueryResponseOp entityQueryResponseOp)
    {
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

        Thread.Sleep(500);
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
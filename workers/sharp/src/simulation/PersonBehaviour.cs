using System;
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
    private const int OrePerMine = 10;
    private const double MovementSpeedMetresPerSecond = 10;

    private readonly Dependencies _deps;
    private readonly SerializedConnection _conn;
    private readonly EntityId _entityId;
    private readonly Logger _logger;
    private readonly long _houseId;

    private Coordinates _currentPosition;
    private int _currentOre;
    private Destination _currentDestination;
    //TODO(harry) - how do you dispose this?
    private CancellationTokenSource _cancellationTokenSource;

    public PersonBehaviour(Dependencies deps, IComponentData<Person> initialData, EntityId entityId)
    {
      _deps = deps;
      _conn = deps.Connection;
      _entityId = entityId;

      _logger = Logger.WithName(_conn, typeof(PersonBehaviour).Name);
      _cancellationTokenSource = new CancellationTokenSource();

      var data = initialData.Get().Value;
      _currentPosition = data.position;
      _currentDestination = data.destination;
      _houseId = (long) data.homeId;
    }

    public void Update(IComponentUpdate<Person> componentUpdate)
    {
      var oreOption = componentUpdate.Get().ore;
      if (oreOption.HasValue)
      {
        _currentOre = oreOption.Value;
      }

      var positionOption = componentUpdate.Get().position;
      if (positionOption.HasValue)
      {
        _currentPosition = positionOption.Value;
      }

      var destinationOption = componentUpdate.Get().destination;
      if (destinationOption.HasValue)
      {
        _currentDestination = destinationOption.Value;
      }
    }

    public void AuthorityChanged(bool hasAuthority)
    {
      if (hasAuthority)
      {
        if (_currentDestination == Destination.MOUNTAIN)
        {
          FindMountain(_cancellationTokenSource.Token);
        }
        else
        {
          FindHome(_cancellationTokenSource.Token);
        }
      }
      else
      {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
      }
    }

    private void FindMountain(CancellationToken token)
    {
      var entityQuery = new EntityQuery
      {
        Constraint = new AndConstraint(
          new IConstraint[]
          {
            new SphereConstraint(_currentPosition, 200),
            new ComponentConstraint(Mountain.ComponentId),
          }
        ),
        ResultType = new SnapshotResultType()
      };
      _deps.QueryDispatcher.Send(entityQuery, o => Task.Run(() => MoveToMountain(o, token), token));
    }

    private void MoveToMountain(EntityQueryResponseOp entityQueryResponseOp, CancellationToken token)
    {
      var results = entityQueryResponseOp.Result;
      if (results.Any())
      {
        var target = results.Where(r => r.Value.Get<Mountain>().HasValue).ToList();
        if (target.Any())
        {
          _logger.Warn($"Moving {_entityId} towards mountain");

          var mountain = target.First().Value.Get<Mountain>();
          MoveTo(token, mountain.Value.Get().Value.position);

          var mineRequest = new Mountain.Commands.Mine.Request(OrePerMine);
          _deps.QueryDispatcher.SendCommand(target.First().Key, mineRequest, op => ReceiveOre(token, op, OrePerMine));
        }
      }
    }

    private void ReceiveOre(CancellationToken token, CommandResponseOp<Mountain.Commands.Mine> op, int minedOre)
    {
      if (op.StatusCode == StatusCode.Success)
      {
        _logger.Warn($"Entity {_entityId} mined {minedOre} ore");
        var componentUpdate = new Person.Update
        {
          ore = _currentOre + minedOre,
          destination = Destination.HOUSE
        };
        _conn.Do(c => c.SendComponentUpdate(_entityId, componentUpdate));
      }

      FindHome(token);
    }

    private void FindHome(CancellationToken token)
    {
      var entityQuery = new EntityQuery
      {
        Constraint = new AndConstraint(
          new IConstraint[]
          {
            new EntityIdConstraint(new EntityId(_houseId)),
          }
        ),
        ResultType = new SnapshotResultType()
      };

      _deps.QueryDispatcher.Send(entityQuery, o => Task.Run(() => MoveToHome(o, token), token));
    }

    private void MoveToHome(EntityQueryResponseOp entityQueryResponseOp, CancellationToken token)
    {
      var results = entityQueryResponseOp.Result;
      if (results.Any())
      {
        var target = results.Where(r => r.Value.Get<House>().HasValue).ToList();
        if (target.Any())
        {
          _logger.Warn($"Moving {_entityId} towards house");

          MoveTo(token, target.First().Value.Get<House>().Value.Get().Value.position);

          var depositRequest = new House.Commands.Deposit.Request(_currentOre);
          _deps.QueryDispatcher.SendCommand(target.First().Key, depositRequest, op => DepositOre(token, op, _currentOre));
        }
      }
    }

    private void DepositOre(CancellationToken token, CommandResponseOp<House.Commands.Deposit> op, int depositedOre)
    {
      if (op.StatusCode == StatusCode.Success)
      {
        _logger.Warn($"Entity {_entityId} deposited {depositedOre} ore");
        var componentUpdate = new Person.Update
        {
          ore = _currentOre - depositedOre,
          destination = Destination.MOUNTAIN
        };
        _conn.Do(c => c.SendComponentUpdate(_entityId, componentUpdate));
      }

      FindMountain(token);
    }

    private void MoveTo(CancellationToken token, Coordinates target)
    {
      var originalPosition = _currentPosition;

      var direction = target - _currentPosition;
      var distance = Math.Sqrt(direction.SquareMagnitude());
      var unitDirection = direction / distance;

      var steps = Math.Floor(distance / MovementSpeedMetresPerSecond);

      for (int i = 1; i < steps; i++)
      {
        var newPosition = originalPosition + unitDirection * (i * MovementSpeedMetresPerSecond);

        var componentUpdate = new Person.Update
        {
          position = newPosition
        };
        _conn.Do(c => c.SendComponentUpdate(_entityId, componentUpdate));

        try
        {
          Task.Delay(500, token).Wait(token);
        }
        catch (Exception e)
        {
          // Task cancelled
          return;
        }
      }

      var finalUpdate = new Person.Update
      {
        position = target
      };
      _conn.Do(c => c.SendComponentUpdate(_entityId, finalUpdate));
    }
  }
}
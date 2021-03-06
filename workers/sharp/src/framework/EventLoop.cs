using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Improbable;
using Improbable.Worker;
using Improbable.Worker.Internal;

namespace SharpWorker.framework
{
  public class EventLoop
  {
    private const int FramesPerSecond = 60;

    private readonly Logger _logger;
    private readonly SerializedConnection _serializedConnection;
    private readonly Dispatcher _dispatcher;
    private readonly Dependencies _dependencies;

    private readonly Dictionary<EntityId, IList<IComponentBehaviour>> _componentBehaviours;

    public static EventLoop Connect(string workerId, string receptionistIp, string receptionistPort)
    {
      var parameters = new ConnectionParameters
      {
        WorkerType = "csharp",
        WorkerId = workerId,
        Network =
        {
          ConnectionType = NetworkConnectionType.Tcp,
          UseExternalIp = false
        }
      };
      var futureConnection = Connection.ConnectAsync(receptionistIp, (ushort) int.Parse(receptionistPort), parameters);

      var connection = futureConnection.Get(10000);
      if (!connection.HasValue)
      {
        throw new Exception($"Timed out waiting for connecton at {receptionistIp}:{receptionistPort}");
      }
      return new EventLoop(new SerializedConnection(connection.Value));
    }

    private EventLoop(SerializedConnection connection)
    {
      _componentBehaviours = new Dictionary<EntityId, IList<IComponentBehaviour>>();
      _serializedConnection = connection;
      _logger = Logger.WithName(_serializedConnection, typeof(EventLoop).Name);

      var dispatcher = new Dispatcher();
      SubscribeToAddEntity(dispatcher);

      _dependencies = new Dependencies(connection, new QueryDispatcher(dispatcher, connection));

      _dispatcher = dispatcher;
    }

    public void Register<TMeta, TBehaviour>(Func<Dependencies, IComponentData<TMeta>, EntityId, TBehaviour> creationFunc)
      where TMeta : IComponentMetaclass
      where TBehaviour : IComponentBehaviour<TMeta>
    {
      var componentId = ComponentDatabase.MetaclassToId<TMeta>();
      _dispatcher.OnAddComponent<TMeta>(o =>
      {
        IList<IComponentBehaviour> behaviours;
        if (_componentBehaviours.TryGetValue(o.EntityId, out behaviours))
        {
          _logger.Warn($"Component added for entity {o.EntityId}");

          behaviours.Add(creationFunc(_dependencies, o.Data, o.EntityId));
        }
        else
        {
          _logger.Warn($"Person component added for unknown entity {o.EntityId}");
        }
      });

      _dispatcher.OnAuthorityChange<TMeta>(o =>
      {
        IList<IComponentBehaviour> behaviour;
        if (_componentBehaviours.TryGetValue(o.EntityId, out behaviour))
        {
          var componentBehvaiour = behaviour.FirstOrDefault(b => b is TBehaviour);
          if (componentBehvaiour != null)
          {
            _logger.Warn(
              $"Authority changed on entity {o.EntityId} for component {componentId} to {o.HasAuthority}");
            ((TBehaviour)componentBehvaiour).AuthorityChanged(o.HasAuthority);
          }
          else
          {
            _logger.Warn($"Received AuthorityChanged on entity {o.EntityId} for unknown component {componentId}");
          }
        }
        else
        {
          _logger.Warn($"Received AuthorityChanged for component {componentId} on unknown entity {o.EntityId}");
        }
      });

      _dispatcher.OnComponentUpdate<TMeta>(o =>
      {
        IList<IComponentBehaviour> behaviour;
        if (_componentBehaviours.TryGetValue(o.EntityId, out behaviour))
        {
          var componentBehaviour = behaviour.FirstOrDefault(b => b is TBehaviour);
          if (componentBehaviour != null)
          {
            ((TBehaviour)componentBehaviour).Update(o.Update);
          }
          else
          {
            _logger.Warn($"Received component update on entity {o.EntityId} for unknown component {componentId}");
          }
        }
        else
        {
          _logger.Warn($"Received component update for component {componentId} on unknown entity {o.EntityId}");
        }
      });
    }

    private void SubscribeToAddEntity(Dispatcher dispatcher)
    {
      dispatcher.OnAddEntity(o =>
      {
        _logger.Warn($"Entity {o.EntityId} created on worker");
        _componentBehaviours[o.EntityId] = new List<IComponentBehaviour>();
      });
    }

    public void RegisterCommandHandler<TCommand, TBehaviour>() where TCommand : ICommandMetaclass, new()
    {
      _dispatcher.OnCommandRequest<TCommand>(o =>
        {
          IList<IComponentBehaviour> behaviour;
          if (_componentBehaviours.TryGetValue(o.EntityId, out behaviour))
          {
            var componentBehaviour = behaviour.FirstOrDefault(b => b is TBehaviour);
            if (componentBehaviour != null)
            {
              var handler = componentBehaviour as ICommandHandler<TCommand>;
              if (handler != null)
              {
                handler.DoCommand(o);
              }
              else
              {
                _logger.Warn($"Behaviour {typeof(TBehaviour).Name} on entity {o.EntityId} for unknown component");
              }
            }
            else
            {
              _logger.Warn($"Received command on entity {o.EntityId} for unknown component");
            }
          }
          else
          {
            _logger.Warn($"Received command for unknown entity {o.EntityId}");
          }
        });
    }

    public void Run()
    {
      var nextFrameTime = DateTime.Now;
      while (true)
      {

        var opList = _serializedConnection.Do(c => c.GetOpList(0 /* non-blocking */));

        _dispatcher.Process(opList);

        nextFrameTime = nextFrameTime.AddMilliseconds(1000f / FramesPerSecond);
        var waitFor = nextFrameTime.Subtract(DateTime.Now);
        Thread.Sleep(waitFor.Milliseconds > 0 ? waitFor : TimeSpan.Zero);
      }
    }
  }
}
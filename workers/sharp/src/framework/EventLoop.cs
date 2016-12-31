using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Improbable;
using Improbable.Worker;
using Ragan;
using SharpWorker.simulation;

namespace SharpWorker.framework
{
  public class EventLoop
  {
    private const int FramesPerSecond = 60;

    private readonly Logger _logger;
    private readonly SerializedConnection _serializedConnection;
    private readonly Dispatcher _dispatcher;
    private readonly Dictionary<EntityId, IList<IComponentBehaviour>> _componentBehaviours;

    private long _frame;

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

      SubscribeToPersonComponentAdded(dispatcher);

      SubscribeToPersonAuthorityChanged(dispatcher);

      _dispatcher = dispatcher;
    }

    private void SubscribeToPersonAuthorityChanged(Dispatcher dispatcher)
    {
      dispatcher.OnAuthorityChange<Person>(o =>
      {
        IList<IComponentBehaviour> behaviour;
        if (_componentBehaviours.TryGetValue(o.EntityId, out behaviour))
        {
          var personBehaviour = behaviour.FirstOrDefault(b => b is PersonBehaviour);
          if (personBehaviour != null)
          {
            _logger.Warn(
              $"Authority changed on entity {o.EntityId} for component {Person.ComponentId} to {o.HasAuthority}");
            personBehaviour.AuthorityChanged(o.HasAuthority);
          }
          else
          {
            _logger.Warn($"Received AuthorityChanged on entity {o.EntityId} for unknown component {Person.ComponentId}");
          }
        }
        else
        {
          _logger.Warn($"Received AuthorityChanged for component {Person.ComponentId} on unknown entity {o.EntityId}");
        }
      });
    }

    private void SubscribeToPersonComponentAdded(Dispatcher dispatcher)
    {
      dispatcher.OnAddComponent<Person>(o =>
      {
        IList<IComponentBehaviour> behaviour;
        if (_componentBehaviours.TryGetValue(o.EntityId, out behaviour))
        {
          _logger.Warn($"Person component added for entity {o.EntityId}");

          behaviour.Add(new PersonBehaviour(_serializedConnection, o.EntityId, o.Data.Get().Value));
        }
        else
        {
          _logger.Warn($"Person component added for unknown entity {o.EntityId}");
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

    public void Run()
    {
      var nextFrameTime = DateTime.Now;
      while (true)
      {
        _frame++;
        if (_frame % (FramesPerSecond * 5) == 0)
        {
          _logger.Warn($"Looping...");
        }

        var opList = _serializedConnection.Do(c => c.GetOpList(0 /* non-blocking */));

        // Invoke user-provided callbacks.
        _dispatcher.Process(opList);

        nextFrameTime = nextFrameTime.AddMilliseconds(1000f / FramesPerSecond);
        var waitFor = nextFrameTime.Subtract(DateTime.Now);
        Thread.Sleep(waitFor.Milliseconds > 0 ? waitFor : TimeSpan.Zero);
      }
    }
  }
}
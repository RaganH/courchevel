using System;
using System.Collections.Generic;
using System.Threading;
using Improbable;
using Improbable.Worker;
using Ragan;

namespace SharpWorker.framework
{
  public class EventLoop
  {
    private const int FramesPerSecond = 60;

    private readonly Connection _connection;
    private readonly SerializedConnection _serializedConnection;
    private readonly Dispatcher _dispatcher;
    private readonly Dictionary<EntityId, IEntityBehaviour> _entityBehaviours;

    private long _frame;

    public static EventLoop Create(string workerId, string receptionistIp, string receptionistPort)
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
      return new EventLoop(connection.Value);
    }

    private EventLoop(Connection connection)
    {
      _connection = connection;

      _entityBehaviours = new Dictionary<EntityId, IEntityBehaviour>();
      _serializedConnection = new SerializedConnection(connection);

      var dispatcher = new Dispatcher();
      dispatcher.OnAddEntity(o =>
      {
        _serializedConnection.Do(c => c.SendLogMessage(LogLevel.Warn, "EventLoop", $"Entity {o.EntityId} created on worker!!"));
        _entityBehaviours[o.EntityId] = new Person(_serializedConnection, o.EntityId);
      });
      dispatcher.OnAddComponent<Wealth>(o =>
      {
        connection.SendLogMessage(LogLevel.Warn, "EventLoop", $"Wealth component added for entity {o.EntityId}");
        IEntityBehaviour behaviour;
        if (_entityBehaviours.TryGetValue(o.EntityId, out behaviour))
        {
          behaviour.AddComponent(o);
        }
        else
        {
          //TODO(harry) - log here
        }
      });
      dispatcher.OnAuthorityChange<Wealth>(o =>
      {
        _serializedConnection.Do(c => c.SendLogMessage(LogLevel.Warn, "EventLoop",
          $"Authority changed on entity {o.EntityId} for component {Wealth.ComponentId} to {o.HasAuthority}"));

        IEntityBehaviour behaviour;
        if (_entityBehaviours.TryGetValue(o.EntityId, out behaviour))
        {
          behaviour.AuthorityChanged(o);
        }
        else
        {
          //TODO(harry) - log here
        }
      });
      _dispatcher = dispatcher;
    }

    public void Run()
    {
      var nextFrameTime = DateTime.Now;
      while (true)
      {
        _frame++;
        if (_frame % (FramesPerSecond * 5) == 0)
        {
          _connection.SendLogMessage(LogLevel.Warn, "EventLoop", $"Looping...");
        }
        var opList = _connection.GetOpList(0 /* non-blocking */);

        // Invoke user-provided callbacks.
        _dispatcher.Process(opList);

        nextFrameTime = nextFrameTime.AddMilliseconds(1000f / FramesPerSecond);
        var waitFor = nextFrameTime.Subtract(DateTime.Now);
        Thread.Sleep(waitFor.Milliseconds > 0 ? waitFor : TimeSpan.Zero);
      }
    }
  }
}
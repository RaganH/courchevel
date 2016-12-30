using System;
using System.Threading;
using Improbable.Worker;
using Ragan;

namespace SharpWorker.framework
{
  public class EventLoop
  {
    private readonly Connection _connection;
    private readonly Dispatcher _dispatcher;

    private const int FramesPerSecond = 60;

    private long _frame;
    private int _currentPersonWealth = 1;

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
      var futureConnection = Connection.ConnectAsync(receptionistIp, (ushort) int.Parse(receptionistPort),
        parameters);

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
      var dispatcher = new Dispatcher();
      dispatcher.OnAddEntity(o => LogOnEntityAdded(connection, o));
      dispatcher.OnAddComponent<Wealth>(o => LogOnComponentAdded(connection, o));
      dispatcher.OnAuthorityChange<Wealth>(o => StartUpdatingWealth(connection, o));

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

        // Do other work here...
        nextFrameTime = nextFrameTime.AddMilliseconds(1000f / FramesPerSecond);
        var waitFor = nextFrameTime.Subtract(DateTime.Now);
        Thread.Sleep(waitFor.Milliseconds > 0 ? waitFor : TimeSpan.Zero);
      }
    }

    private void StartUpdatingWealth(Connection conn, AuthorityChangeOp authorityChangeOp)
    {
      conn.SendLogMessage(LogLevel.Warn, "EventLoop",
        $"Authority changed for entity {authorityChangeOp.EntityId} to {authorityChangeOp.HasAuthority}");
      var entityId = authorityChangeOp.EntityId;
      if (authorityChangeOp.HasAuthority)
      {
        new Thread(() =>
        {
          for (int i = 0; i < 1000; i++)
          {
            var componentUpdate = new Wealth.Update();
            componentUpdate.SetCurrent(_currentPersonWealth++);
            conn.SendComponentUpdate(entityId, componentUpdate);
            Thread.Sleep(1000);
          }
        }).Start();
      }
    }

    private void LogOnComponentAdded(Connection conn, AddComponentOp<Wealth> addComponentOp)
    {
      conn.SendLogMessage(LogLevel.Warn, "EventLoop", $"Wealth component added for entity {addComponentOp.EntityId}");
    }

    private void LogOnEntityAdded(Connection conn, AddEntityOp obj)
    {
      conn.SendLogMessage(LogLevel.Warn, "EventLoop", $"Entity {obj.EntityId} created on worker!!");
    }

  }
}
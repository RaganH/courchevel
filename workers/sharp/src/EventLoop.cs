using System;
using System.Reflection;
using System.Threading;
using Improbable.Worker;
using Ragan;
using SharpWorker.snapshot;

class EventLoop
{
    private readonly Connection _connection;
    private readonly Dispatcher _dispatcher;

    private const int FramesPerSecond = 60;

    private long _frame;
    private int _currentPersonWealth = 1;

    static int Main(string[] args)
    {
        try
        {
            Assembly.Load("GeneratedCode"); // Required so subscriptions work

            if (args.Length == 2 && args[0].ToLower() == "snapshot")
            {
                return SnapshotFactory.CreateSnapshot(args[1]);
            }

            if (args.Length < 3)
            {
                Console.WriteLine($"ERROR: Expected 3 arguments but given {args}. Exiting.");
                return 7;
            }

            var workerId = args[0];
            var receptionistIp = args[1];
            var receptionistPort = args[2];

            Console.WriteLine($"Worker {workerId} starting");

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
                Console.WriteLine($"Timed out waiting for connecton at {receptionistIp}:{receptionistPort}");
                return 2;
            }
            Console.WriteLine("Connected! Starting event loop");
            var eventLoop = new EventLoop(connection.Value);

            eventLoop.Run(connection.Value);
        }
        catch (Exception e)
        {
            return 3;
        }

        return 0;
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

    private void StartUpdatingWealth(Connection conn, AuthorityChangeOp authorityChangeOp)
    {
        conn.SendLogMessage(LogLevel.Warn, "EventLoop", $"Authority changed for entity {authorityChangeOp.EntityId} to {authorityChangeOp.HasAuthority}");
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

    void Run(Connection conn)
    {
        var nextFrameTime = System.DateTime.Now;
        while (true)
        {
            _frame++;
            if (_frame%(FramesPerSecond*5) == 0)
            {
                conn.SendLogMessage(LogLevel.Warn, "EventLoop", $"Looping...");
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
}

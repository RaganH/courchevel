using System;
using System.Reflection;
using System.Threading;
using Improbable.Worker;
using Ragan;
using Improbable;
using System.Collections.Generic;
using System.Linq;
using Improbable.Math;

class EventLoop
{
    private readonly Connection _connection;
    private readonly Dispatcher _dispatcher;

    private const int FramesPerSecond = 60;

    private long _frame;
    private int _currentPersonWealth = 1;

    static int Main(string[] args)
    {
        Assembly.Load("GeneratedCode"); // Required so subscriptions work

        if (args.Length < 2)
        {
            Console.WriteLine($"Not enough arguments! Given {args} but expected at least 2. Exiting.");
            return 1;
        }

        if (args.Length == 2 && args[0].ToLower() == "snapshot")
        {
            createSnapshot(args[1]);
            return 0;
        }

        var workerId = args[0];
        Console.WriteLine($"Worker {workerId} starting");

        var parameters = new ConnectionParameters
        {
            WorkerType = "SharpWorker",
            WorkerId = workerId,
            Network =
            {
                ConnectionType = NetworkConnectionType.Tcp,
                UseExternalIp = false
            }
        };
        var hostname = "localhost";
        if (args.Length == 2)
        {
            hostname = args[1];
        }
        ushort port = 7777;

        var futureConnection = Connection.ConnectAsync(hostname, port, parameters);

        var connection = futureConnection.Get(10000);

        if (!connection.HasValue)
        {
            Console.WriteLine($"Timed out waiting for connecton at {hostname}:{port}");
            return 1;
        }
        Console.WriteLine("Connected! Starting event loop");
        var eventLoop = new EventLoop(connection.Value);    
        
        eventLoop.Run(connection.Value);

        return 0;
    }

    private static void createSnapshot(string snapshotPath)
    {
        var entity = new SnapshotEntity();

        entity.Add(new Position.Data(new Coordinates(0, 0, 0)));
        entity.Add(new Wealth.Data(100));
        entity.SetAuthority<Wealth>(true);

        IDictionary<EntityId, SnapshotEntity> entities = new Dictionary<EntityId, SnapshotEntity>
        {
            {new EntityId(1), entity},
        };
        // Save the snapshot back to the file.
        var errorOpt = Snapshot.Save(snapshotPath, entities);
        if (errorOpt.HasValue)
        {
            Console.WriteLine($"Error saving snapshot: {errorOpt.Value}");
        }
        Console.WriteLine($"Saved {snapshotPath} successfully.");
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
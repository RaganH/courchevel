using System.Threading;
using Improbable.Worker;
using Ragan;
using SharpWorker;

class EventLoop
{
    private readonly Connection _connection;
    private readonly Dispatcher _dispatcher;

    private Person _person;

    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            return 1;
        }
        var parameters = new ConnectionParameters
        {
            WorkerType = "SharpWorker",
            WorkerId = args[0],
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
        var connection = new Connection(hostname, 7777, parameters);
        var eventLoop = new EventLoop(connection);
        
        eventLoop.Run();

        return 0;
    }

    private EventLoop(Connection connection)
    {
        _connection = connection;

        var dispatcher = new Dispatcher();
//        dispatcher.OnAddEntity(o => LogOnEntityAdded(connection, o));
        dispatcher.OnAddComponent<Wealth>(o => LogOnComponentAdded(connection, o));
        _dispatcher = dispatcher;
    }

    private void LogOnComponentAdded(Connection conn, AddComponentOp<Wealth> addComponentOp)
    {
        conn.SendLogMessage(LogLevel.Warn, "EventLoop",
            $"Wealth component added for entity {addComponentOp.EntityId}");
//        if (_person == null)
//        {
//            _person = new Person(conn, addComponentOp.EntityId, addComponentOp.Data);
//        }
    }

    private void LogOnEntityAdded(Connection conn, AddEntityOp obj)
    {
        conn.SendLogMessage(LogLevel.Warn, "EventLoop", $"Entity {obj.EntityId} created on worker!!");
    }

    const int FramesPerSecond = 60;
    void Run()
    {
        var nextFrameTime = System.DateTime.Now;
        while (true)
        {
            var opList = _connection.GetOpList(0 /* non-blocking */);
            // Invoke user-provided callbacks.
            _dispatcher.Process(opList);
            // Do other work here...
            nextFrameTime = nextFrameTime.AddMilliseconds(1000f / FramesPerSecond);
            var waitFor = nextFrameTime.Subtract(System.DateTime.Now);
            Thread.Sleep(waitFor.Milliseconds > 0 ? waitFor : System.TimeSpan.Zero);
        }
    }
}

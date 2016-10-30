using Improbable.Worker;
using System;

class Test
{
    static int Main(string[] args)
    {
        /*
        if (args.Length < 2)
        {
            return 1;
        }
        */
        var parameters = new ConnectionParameters();
        parameters.WorkerType = "SharpWorker";
        parameters.WorkerId = "local_worker_yo";//  args[0];
        parameters.Network.ConnectionType = NetworkConnectionType.Tcp;
        parameters.Network.UseExternalIp = false;

        var hostname = "localhost";
        /*
        if (args.Length == 2)
        {
            hostname = args[1];
        }
        */
        var connection = new Connection(hostname, 7777, parameters);

        var dispatcher = new Dispatcher();

        dispatcher.OnAddEntity(o => DoAThing(connection, o));

        RunEventLoop(connection, dispatcher);

        return 0;
    }

    private static void DoAThing(Connection conn, AddEntityOp obj)
    {
        conn.SendLogMessage(LogLevel.Warn, "MyWorkerLogger", string.Format("Entity {0} created on worker!!", obj.EntityId));
    }

    const int FramesPerSecond = 60;
    static void RunEventLoop(Improbable.Worker.Connection connection, Improbable.Worker.Dispatcher dispatcher)
    {
        var nextFrameTime = System.DateTime.Now;
        while (true)
        {
            connection.SendLogMessage(LogLevel.Warn, "eventLoop", "looping");
            var opList = connection.GetOpList(0 /* non-blocking */);
            // Invoke user-provided callbacks.
            dispatcher.Process(opList);
            // Do other work here...
            nextFrameTime = nextFrameTime.AddMilliseconds(1000f / FramesPerSecond);
            var waitFor = nextFrameTime.Subtract(System.DateTime.Now);
            System.Threading.Thread.Sleep(waitFor.Milliseconds > 0 ? waitFor : System.TimeSpan.Zero);
        }
    }
}

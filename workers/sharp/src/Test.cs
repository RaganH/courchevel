using Improbable.Worker;
using System;

class Test
{
    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            return 1;
        }

        var parameters = new ConnectionParameters();
        parameters.WorkerType = "MyCsharpWorker";
        parameters.WorkerId = args[0];
        parameters.Network.ConnectionType = NetworkConnectionType.Tcp;
        parameters.Network.UseExternalIp = false;

        var hostname = "localhost";
        if (args.Length == 2)
        {
            hostname = args[1];
        }

        var connection = new Connection(hostname, 7777, parameters);
        return 0;
    }
}

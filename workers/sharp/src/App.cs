using System;
using System.Reflection;
using SharpWorker.errors;
using SharpWorker.framework;
using SharpWorker.snapshot;

namespace SharpWorker
{
  static class App
  {
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
          return ErrorCodes.ErrNotEnoughArgs;
        }

        var workerId = args[0];
        var receptionistIp = args[1];
        var receptionistPort = args[2];

        EventLoop eventLoop;
        try
        {
          eventLoop = EventLoop.Connect(workerId, receptionistIp, receptionistPort);
        }
        catch (Exception e)
        {
          return ErrorCodes.ErrInitialConnectionFailed;
        }
        eventLoop.Run();

        return 0;
      }
      catch (Exception e)
      {
        return ErrorCodes.ErrUnhandledException;
      }
    }
  }
}
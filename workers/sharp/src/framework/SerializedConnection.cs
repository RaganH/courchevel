using System;
using Improbable.Worker;

namespace SharpWorker.framework
{
  public class SerializedConnection
  {
    private readonly object _lock = new object();
    private readonly Connection _connection;

    public SerializedConnection(Connection connection)
    {
      _connection = connection;
    }

    public void Do (Action<Connection> func)
    {
      lock (_lock)
      {
        func(_connection);
      }
    }

    public T Do<T> (Func<Connection, T> func)
    {
      lock (_lock)
      {
        return func(_connection);
      }
    }

    public void SendLogMessage(LogLevel level, string loggerName, string message)
    {
      lock (_lock)
      {
        _connection.SendLogMessage(level, loggerName, message);
      }
    }
  }
}
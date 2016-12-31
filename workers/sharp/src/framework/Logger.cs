using Improbable.Worker;

namespace SharpWorker.framework
{
  public class Logger
  {
    private readonly SerializedConnection _connection;
    private readonly string _loggerName;

    public static Logger WithName(SerializedConnection connection, string loggerName)
    {
      return new Logger(connection, loggerName);
    }

    private Logger(SerializedConnection connection, string loggerName)
    {
      _connection = connection;
      _loggerName = loggerName;
    }

    public void Debug(string message)
    {
      _connection.SendLogMessage(LogLevel.Debug, _loggerName, message);
    }

    public void Info(string message)
    {
      _connection.SendLogMessage(LogLevel.Info, _loggerName, message);
    }

    public void Warn(string message)
    {
      _connection.SendLogMessage(LogLevel.Warn, _loggerName, message);
    }

    public void Error(string message)
    {
      _connection.SendLogMessage(LogLevel.Error, _loggerName, message);
    }

    public void Fatal(string message)
    {
      _connection.SendLogMessage(LogLevel.Fatal, _loggerName, message);
    }
  }
}
namespace SharpWorker.framework
{
  public class Dependencies
  {
    public SerializedConnection Connection { get; private set; }
    public QueryDispatcher QueryDispatcher { get; private set; }

    public Dependencies(SerializedConnection connection, QueryDispatcher queryDispatcher)
    {
      Connection = connection;
      QueryDispatcher = queryDispatcher;
    }
  }
}
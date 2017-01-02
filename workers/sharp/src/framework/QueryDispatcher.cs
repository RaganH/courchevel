using System;
using System.Collections.Concurrent;
using Improbable.Worker;
using Improbable.Worker.Query;

namespace SharpWorker.framework
{
  public class QueryDispatcher
  {
    private const int _defaultTimeoutMillis = 5000;

    private readonly SerializedConnection _conn;
    private readonly Logger _logger;

    private readonly ConcurrentDictionary<uint, Action<EntityQueryResponseOp>> _callbacks;

    public QueryDispatcher(Dispatcher dispatcher, SerializedConnection conn)
    {
      _conn = conn;
      _logger = Logger.WithName(conn, typeof(QueryDispatcher).Name);
      _callbacks = new ConcurrentDictionary<uint, Action<EntityQueryResponseOp>>();

      dispatcher.OnEntityQueryResponse(DispatchQueryResponse);
    }

    public void Send(EntityQuery query, Action<EntityQueryResponseOp> responseCallback)
    {
      var id = _conn.Do(c => c.SendEntityQueryRequest(query, _defaultTimeoutMillis));
      _callbacks.TryAdd(id.Id, responseCallback);
    }

    private void DispatchQueryResponse(EntityQueryResponseOp op)
    {
      if (op.StatusCode != StatusCode.Success)
      {
        _logger.Warn($"Query failed with {op.Message}");
        return;
      }

      Action<EntityQueryResponseOp> callback;
      if (!_callbacks.TryGetValue(op.RequestId.Id, out callback))
      {
        _logger.Warn($"No response callback for EntityQuery {op.RequestId.Id}");
        return;
      }

      callback(op);
    }
  }
}

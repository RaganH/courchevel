using System;
using System.Collections.Concurrent;
using Improbable;
using Improbable.Worker;
using Improbable.Worker.Query;

namespace SharpWorker.framework
{
  public class QueryDispatcher
  {
    private const int DefaultTimeoutMillis = 5000;

    private readonly Dispatcher _dispatcher;
    private readonly SerializedConnection _conn;
    private readonly Logger _logger;

    private readonly ConcurrentDictionary<uint, Action<EntityQueryResponseOp>> _entityQueryCallbacks;
    private readonly ConcurrentDictionary<uint, Action<object>> _commandCallbacks;
    private readonly ConcurrentDictionary<Type, bool> _susbscribed;

    public QueryDispatcher(Dispatcher dispatcher, SerializedConnection conn)
    {
      _dispatcher = dispatcher;
      _conn = conn;
      _logger = Logger.WithName(conn, typeof(QueryDispatcher).Name);

      _susbscribed = new ConcurrentDictionary<Type, bool>();
      _entityQueryCallbacks = new ConcurrentDictionary<uint, Action<EntityQueryResponseOp>>();
      _commandCallbacks = new ConcurrentDictionary<uint, Action<object>>();

      dispatcher.OnEntityQueryResponse(DispatchQueryResponse);
    }

    public void Send(EntityQuery query, Action<EntityQueryResponseOp> responseCallback)
    {
      var id = _conn.Do(c => c.SendEntityQueryRequest(query, DefaultTimeoutMillis));
      _entityQueryCallbacks.TryAdd(id.Id, responseCallback);
    }

    public void SendCommand<T>(EntityId entityId, ICommandRequest<T> commandRequest, Action<CommandResponseOp<T>> responseCallback)
      where T : ICommandMetaclass, new()
    {
      if (_susbscribed.TryAdd(typeof(T), true))
      {
        _dispatcher.OnCommandResponse<T>(DispatchCommandResponse);
      }
      var id = _conn.Do(c => c.SendCommandRequest(entityId, commandRequest, DefaultTimeoutMillis));
      _commandCallbacks.TryAdd(id.Id, o =>
      {
        if (o is CommandResponseOp<T>)
        {
          responseCallback((CommandResponseOp<T>) o);
        }
        else
        {
          _logger.Warn($"Command callback expects wrong type. Have type {typeof(T).Name} but callback was: {o.GetType()}");
        }
      });
    }

    private void DispatchCommandResponse<T>(CommandResponseOp<T> op) where T : ICommandMetaclass
    {
      if (op.StatusCode != StatusCode.Success)
      {
        _logger.Warn($"Query failed with {op.Message}");
        return;
      }

      Action<object> callback;
      if (!_commandCallbacks.TryGetValue(op.RequestId.Id, out callback))
      {
        _logger.Warn($"No response callback for EntityQuery {op.RequestId.Id}");
        return;
      }

      callback(op);
    }

    private void DispatchQueryResponse(EntityQueryResponseOp op)
    {
      if (op.StatusCode != StatusCode.Success)
      {
        _logger.Warn($"Query failed with {op.Message}");
        return;
      }

      Action<EntityQueryResponseOp> callback;
      if (!_entityQueryCallbacks.TryGetValue(op.RequestId.Id, out callback))
      {
        _logger.Warn($"No response callback for EntityQuery {op.RequestId.Id}");
        return;
      }

      callback(op);
    }
  }
}

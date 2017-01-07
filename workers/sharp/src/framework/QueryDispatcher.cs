using System;
using System.Collections.Concurrent;
using Improbable;
using Improbable.Worker;
using Improbable.Worker.Query;
using Ragan;

namespace SharpWorker.framework
{
  public class QueryDispatcher
  {
    private const int _defaultTimeoutMillis = 5000;

    private readonly SerializedConnection _conn;
    private readonly Logger _logger;

    private readonly ConcurrentDictionary<uint, Action<EntityQueryResponseOp>> _entityQueryCallbacks;
    private readonly ConcurrentDictionary<uint, Action<CommandResponseOp<Mountain.Commands.Mine>>> _commandCallbacks;

    public QueryDispatcher(Dispatcher dispatcher, SerializedConnection conn)
    {
      _conn = conn;
      _logger = Logger.WithName(conn, typeof(QueryDispatcher).Name);
      _entityQueryCallbacks = new ConcurrentDictionary<uint, Action<EntityQueryResponseOp>>();
      _commandCallbacks = new ConcurrentDictionary<uint, Action<CommandResponseOp<Mountain.Commands.Mine>>>();

      dispatcher.OnEntityQueryResponse(DispatchQueryResponse);
      // TODO(harry) remove hard coded type
      dispatcher.OnCommandResponse<Mountain.Commands.Mine>(DispatchCommandResponse);
    }

    public void Send(EntityQuery query, Action<EntityQueryResponseOp> responseCallback)
    {
      var id = _conn.Do(c => c.SendEntityQueryRequest(query, _defaultTimeoutMillis));
      _entityQueryCallbacks.TryAdd(id.Id, responseCallback);
    }

    public void SendCommand(EntityId entityId, ICommandRequest<Mountain.Commands.Mine> commandRequest, Action<CommandResponseOp<Mountain.Commands.Mine>> responseCallback)
    {
      var id = _conn.Do(c => c.SendCommandRequest(entityId, commandRequest, _defaultTimeoutMillis));
      _commandCallbacks.TryAdd(id.Id, responseCallback);
    }

    private void DispatchCommandResponse(CommandResponseOp<Mountain.Commands.Mine> op)
    {
      if (op.StatusCode != StatusCode.Success)
      {
        _logger.Warn($"Query failed with {op.Message}");
        return;
      }

      Action<CommandResponseOp<Mountain.Commands.Mine>> callback;
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

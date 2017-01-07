using Improbable;
using Improbable.Worker;
using Ragan;
using SharpWorker.framework;

namespace SharpWorker.simulation
{
  internal class MountainBehaviour : IComponentBehaviour<Mountain>, ICommandHandler<Mountain.Commands.Mine>
  {
    private readonly Dependencies _deps;
    private readonly EntityId _entityId;
    private int _currentOre;

    public MountainBehaviour(Dependencies deps, IComponentData<Mountain> data, EntityId entityId)
    {
      _deps = deps;
      _entityId = entityId;

      _currentOre = data.Get().Value.ore;
    }

    public void AuthorityChanged(bool hasAuthority) { }

    public void Update(IComponentUpdate<Mountain> componentUpdate)
    {
      if (componentUpdate.Get().ore.HasValue)
      {
        _currentOre = componentUpdate.Get().ore.Value;
      }
    }

    public void DoCommand(CommandRequestOp<Mountain.Commands.Mine> op)
    {
      var update = new Mountain.Update
      {
        ore = (int)(_currentOre - op.Request.Get().Value.amount)
      };
      _deps.Connection.Do(c => c.SendComponentUpdate(_entityId, update));
      _deps.Connection.Do(c => c.SendCommandResponse(op.RequestId, new Mountain.Commands.Mine.Response()));
    }
  }
}
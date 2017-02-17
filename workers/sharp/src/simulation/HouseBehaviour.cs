using System;
using Improbable;
using Improbable.Worker;
using Ragan;
using SharpWorker.framework;
using Improbable.Math;

namespace SharpWorker.simulation
{
  internal class HouseBehaviour : IComponentBehaviour<House>, ICommandHandler<House.Commands.Deposit>
  {
    private readonly Dependencies _deps;
    private readonly EntityId _entityId;
    private int _currentOre;

    public HouseBehaviour(Dependencies deps, IComponentData<House> data, EntityId entityId)
    {
      _deps = deps;
      _entityId = entityId;

      _currentOre = data.Get().Value.ore;
    }

      public void AuthorityChanged(bool hasAuthority)
      {
          if (hasAuthority)
          {
              Entity entity = new Entity();
              entity.Add<House>(new House.Data(new Coordinates(1, 2, 3), 200));
              _deps.Connection.Do(c => c.SendCreateEntityRequest(entity, "SpawnedHouse", null, null));
          }
      }

    public void Update(IComponentUpdate<House> componentUpdate)
    {
      if (componentUpdate.Get().ore.HasValue)
      {
        _currentOre = componentUpdate.Get().ore.Value;
      }
    }

    public void DoCommand(CommandRequestOp<House.Commands.Deposit> op)
    {
      var update = new House.Update
      {
        ore = _currentOre + op.Request.Get().Value.amount
      };
      _deps.Connection.Do(c => c.SendComponentUpdate(_entityId, update));
      _deps.Connection.Do(c => c.SendCommandResponse(op.RequestId, new House.Commands.Deposit.Response()));
    }
  }
}
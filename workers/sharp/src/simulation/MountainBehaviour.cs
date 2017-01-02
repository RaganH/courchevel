using Improbable;
using Improbable.Worker;
using Ragan;
using SharpWorker.framework;

namespace SharpWorker.simulation
{
  internal class MountainBehaviour : IComponentBehaviour<Mountain>
  {
    public MountainBehaviour(Dependencies deps, IComponentData<Mountain> dispatcher, EntityId entityId)
    {

    }

    public void AuthorityChanged(bool hasAuthority)
    {

    }

    public void Update(IComponentUpdate<Mountain> componentUpdate)
    {

    }
  }
}
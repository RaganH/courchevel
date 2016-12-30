using Improbable.Worker;
using Ragan;

namespace SharpWorker.framework
{
  interface IEntityBehaviour
  {
    void AddComponent(AddComponentOp<Wealth> data);

    void AuthorityChanged(AuthorityChangeOp authorityChangeOp);
  }
}
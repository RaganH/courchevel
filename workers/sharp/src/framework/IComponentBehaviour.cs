using Improbable.Worker;

namespace SharpWorker.framework
{
  // marker interface
  public interface IComponentBehaviour {}

  public interface IComponentBehaviour<T> : IComponentBehaviour where T : IComponentMetaclass
  {
    void AuthorityChanged(bool hasAuthority);
    void Update(IComponentUpdate<T> componentUpdate);
  }
}
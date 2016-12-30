using System.Threading;
using System.Threading.Tasks;
using Improbable;
using Improbable.Worker;
using Ragan;
using SharpWorker.framework;

namespace SharpWorker
{
  class Person : IEntityBehaviour
  {
    private readonly SerializedConnection _conn;
    private readonly EntityId _entityId;
    private int _currentPersonWealth = 1;
    private Thread _thread;

    public Person(SerializedConnection conn, EntityId entityId)
    {
      _conn = conn;
      _entityId = entityId;
    }

    public void AddComponent(AddComponentOp<Wealth> data)
    {
      _currentPersonWealth = data.Data.Get().Value.current;
    }

    public void AuthorityChanged(AuthorityChangeOp authorityChangeOp)
    {
      if (authorityChangeOp.HasAuthority)
      {
        Task.Run(() => StartUpdatingWealth());
      }
      else
      {
        //TODO(harry) - stop the task here.
      }
    }

    private void StartUpdatingWealth()
    {
      while (true)
      {
        var componentUpdate = new Wealth.Update();
        componentUpdate.SetCurrent(_currentPersonWealth++);
        _conn.Do(c => c.SendComponentUpdate(_entityId, componentUpdate));
        Thread.Sleep(1000);
      }
    }
  }
}
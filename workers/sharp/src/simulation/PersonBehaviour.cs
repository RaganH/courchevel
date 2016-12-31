using System.Threading;
using System.Threading.Tasks;
using Improbable;
using Ragan;
using SharpWorker.framework;

namespace SharpWorker.simulation
{
  class PersonBehaviour : IComponentBehaviour
  {
    private readonly SerializedConnection _conn;
    private readonly EntityId _entityId;
    private int _currentPersonWealth;

    public PersonBehaviour(SerializedConnection conn, EntityId entityId, PersonData value)
    {
      _conn = conn;
      _entityId = entityId;
      _currentPersonWealth = value.wealth.current;
    }

    public void AuthorityChanged(bool hasAuthority)
    {
      if (hasAuthority)
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
        var componentUpdate = new Person.Update
        {
          wealth = new Wealth
          {
            current = _currentPersonWealth++,
          }
        };

        _conn.Do(c => c.SendComponentUpdate(_entityId, componentUpdate));

        Thread.Sleep(1000);
      }
    }
  }
}
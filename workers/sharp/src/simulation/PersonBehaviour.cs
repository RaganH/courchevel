using System.Threading;
using System.Threading.Tasks;
using Improbable;
using Improbable.Worker;
using Ragan;
using SharpWorker.framework;

namespace SharpWorker.simulation
{
  class PersonBehaviour : IComponentBehaviour<Person>
  {
    private readonly SerializedConnection _conn;
    private readonly EntityId _entityId;
    private int _currentWealth;

    public PersonBehaviour(SerializedConnection conn, IComponentData<Person> value, EntityId entityId)
    {
      _conn = conn;
      _entityId = entityId;
      _currentWealth = value.Get().Value.wealth.current;
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
            current = _currentWealth+1,
          }
        };

        _conn.Do(c => c.SendComponentUpdate(_entityId, componentUpdate));

        Thread.Sleep(1000);
      }
    }

    public void Update(IComponentUpdate<Person> componentUpdate)
    {
      var wealthOption = componentUpdate.Get().wealth;
      if (wealthOption.HasValue)
      {
        _currentWealth = wealthOption.Value.current;
      }
    }
  }
}
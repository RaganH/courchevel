using System.Threading;
using Improbable;
using Improbable.Worker;
using Ragan;

namespace SharpWorker
{
  class Person
  {
    private readonly Connection _conn;
    private readonly EntityId _entityId;
    private int _currentWealth;

    public Person(Connection conn, EntityId entityId, IComponentData<Wealth> data)
    {
      _conn = conn;
      _entityId = entityId;
      _currentWealth = data.Get().Value.current;

      new Thread(decreaseWealth).Start();
    }

    private void decreaseWealth()
    {
      while (true)
      {
        Thread.Sleep(1000);
        _currentWealth--;
        IComponentUpdate<Wealth> update = new Wealth.Update
        {
          current = _currentWealth,
        };
        _conn.SendComponentUpdate(_entityId, update);
      }
    }
  }
}
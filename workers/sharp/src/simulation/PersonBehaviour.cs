using System.Threading;
using System.Threading.Tasks;
using Improbable;
using Improbable.Math;
using Improbable.Worker;
using Ragan;
using SharpWorker.framework;

namespace SharpWorker.simulation
{
  class PersonBehaviour : IComponentBehaviour<Person>
  {
    private readonly SerializedConnection _conn;
    private readonly EntityId _entityId;

    private Coordinates _currentPosition;
    private int _currentWealth;
    private Coordinates _target;

    public PersonBehaviour(SerializedConnection conn, IComponentData<Person> value, EntityId entityId)
    {
      _conn = conn;
      _entityId = entityId;

      var data = value.Get().Value;
      _currentWealth = data.wealth.current;
      _currentPosition = data.position;

      _target = new Coordinates(100, 100, 100);
    }

    public void AuthorityChanged(bool hasAuthority)
    {
      if (hasAuthority)
      {
        Task.Run(() => StartUpdatingWealth());
        Task.Run(() => MoveToTarget());
      }
      else
      {
        //TODO(harry) - stop the task here.
      }
    }

    public void Update(IComponentUpdate<Person> componentUpdate)
    {
      var wealthOption = componentUpdate.Get().wealth;
      if (wealthOption.HasValue)
      {
        _currentWealth = wealthOption.Value.current;
      }

      var positionOption = componentUpdate.Get().position;
      if (positionOption.HasValue)
      {
        _currentPosition = positionOption.Value;
      }
    }

    private void MoveToTarget()
    {
      int steps = 100;
      var originalPosition = _currentPosition;
      for (int i = 1; i < steps; i ++)
      {
        var newPosition = new Coordinates{
          X = Lerp(originalPosition.X, _target.X, i, steps),
          Y = Lerp(originalPosition.Y, _target.Y, i, steps),
          Z = Lerp(originalPosition.Z, _target.Z, i, steps),
        };

        var componentUpdate = new Person.Update
        {
          position = newPosition
        };

        _conn.Do(c => c.SendComponentUpdate(_entityId, componentUpdate));

        Thread.Sleep(1000);
      }
    }

    private double Lerp(double original, double target, int currentStep, int steps)
    {
      return original + ((target - original) * (double)currentStep / steps);
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
  }
}
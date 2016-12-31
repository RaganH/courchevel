using System;
using System.Collections.Generic;
using Improbable;
using Improbable.Collections;
using Improbable.Math;
using Improbable.Worker;
using Ragan;

namespace SharpWorker.snapshot
{
  public static class SnapshotFactory
  {
    public static int CreateSnapshot(string snapshotPath)
    {
      var entity = new SnapshotEntity();

      entity.Add(new Person.Data(new Coordinates(0, 0, 0), new Wealth(100)));

      var workerPredicate = new WorkerPredicate(new Improbable.Collections.List<WorkerClaim>
      {
        new WorkerClaim(new Improbable.Collections.List<WorkerClaimAtom>
        {
          new WorkerClaimAtom("sharp")
        }),
      });
      entity.Add(new EntityAcl.Data(workerPredicate, new ComponentAcl(new Map<uint, WorkerPredicate>
      {
        {Person.ComponentId, workerPredicate}
      })));

      IDictionary<EntityId, SnapshotEntity> entities = new Dictionary<EntityId, SnapshotEntity>
      {
        {new EntityId(1), entity},
      };

      var errorOpt = Snapshot.Save(snapshotPath, entities);
      if (errorOpt.HasValue)
      {
        Console.WriteLine($"Error saving snapshot: {errorOpt.Value}");
        return 1;
      }
      Console.WriteLine($"Saved {snapshotPath} successfully.");
      return 0;
    }
  }
}
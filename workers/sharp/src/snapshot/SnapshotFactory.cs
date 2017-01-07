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
    private const int worldSize = 1000;

    public static int CreateSnapshot(string snapshotPath)
    {
      IDictionary<EntityId, SnapshotEntity> entities = new Dictionary<EntityId, SnapshotEntity>();
      int currentEntityId = 1;

      var mountainGrid = PlaceInSquareGrid(3, 0);
      foreach (var coord in mountainGrid)
      {
        var mountainEntity = new SnapshotEntity();
        mountainEntity.Add(new Mountain.Data(coord, 500));
        AssignToSharpWorker(mountainEntity, Mountain.ComponentId);

        entities[new EntityId(currentEntityId)] = mountainEntity;
        currentEntityId++;
      }

      var personGrid = PlaceInSquareGrid(2, 0);
//      var personGrid = PlaceInSquareGrid(5, 0);
      foreach (var coord in personGrid)
      {
        var houseEntity = new SnapshotEntity();
        houseEntity.Add(new House.Data(coord, 0));
        AssignToSharpWorker(houseEntity, House.ComponentId);

        entities[new EntityId(currentEntityId)] = houseEntity;
        currentEntityId++;

        var personEntity = new SnapshotEntity();
        personEntity.Add(new Person.Data(coord, 0, (ulong)currentEntityId-1, Destination.MOUNTAIN));
        AssignToSharpWorker(personEntity, Person.ComponentId);

        entities[new EntityId(currentEntityId)] = personEntity;
        currentEntityId++;
      }

      var errorOpt = Snapshot.Save(snapshotPath, entities);
      if (errorOpt.HasValue)
      {
        Console.WriteLine($"Error saving snapshot: {errorOpt.Value}");
        return 1;
      }
      Console.WriteLine($"Saved {snapshotPath} successfully.");
      return 0;
    }

    private static void AssignToSharpWorker(SnapshotEntity entity, uint componentId)
    {
      var workerPredicate = new WorkerPredicate(new Improbable.Collections.List<WorkerClaim>
      {
        new WorkerClaim(new Improbable.Collections.List<WorkerClaimAtom>
        {
          new WorkerClaimAtom("sharp")
        }),
      });
      entity.Add(new EntityAcl.Data(workerPredicate, new ComponentAcl(new Map<uint, WorkerPredicate>
      {
        {componentId, workerPredicate}
      })));
    }

    private static Coordinates[] PlaceInSquareGrid(int width, double centre)
    {
      double buffer = (worldSize / width) / 2;
      double bottomLeft = centre - worldSize / 2 + buffer;
      double spacing = (worldSize - 2 * buffer) / (width-1);

      var grid = new Coordinates[width * width];

      double currentX = bottomLeft;
      for (int i = 0; i < width; i++)
      {
        double currentZ = bottomLeft;
        for (int j = 0; j < width; j++)
        {
          grid[i * width + j] = new Coordinates(currentX, 0, currentZ);
          currentZ = currentZ + spacing;
        }
        currentX = currentX + spacing;
      }

      return grid;
    }
  }
}
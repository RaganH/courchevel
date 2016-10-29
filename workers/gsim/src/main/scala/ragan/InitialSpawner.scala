package ragan

import improbable.logging.Logger
import improbable.papi.world.AppWorld
import improbable.papi.worldapp.WorldApp

class InitialSpawner(appWorld: AppWorld, logger: Logger) extends WorldApp {
  appWorld.entities.spawnEntity(HealthNature())
}

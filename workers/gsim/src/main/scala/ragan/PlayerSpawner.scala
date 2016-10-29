package ragan

import improbable.logging.Logger
import improbable.papi.world.AppWorld
import improbable.papi.worldapp.WorldApp

/**
  * Created by Harry on 29/10/2016.
  */
class PlayerSpawner(appWorld: AppWorld, logger: Logger) extends WorldApp {
  appWorld.entities.spawnEntity(HealthNature())
}

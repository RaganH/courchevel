package ragan

import improbable.logging.Logger
import improbable.papi.world.AppWorld
import improbable.papi.worldapp.WorldApp
import ragan.natures.PersonNature

import scala.concurrent.duration.Duration

class InitialSpawner(appWorld: AppWorld, logger: Logger) extends WorldApp {
  appWorld.timing.every(Duration(1,"s"))(() => {
    appWorld.entities.spawnEntity(PersonNature())
  })
}

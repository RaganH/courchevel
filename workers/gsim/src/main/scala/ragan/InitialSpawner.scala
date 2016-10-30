package ragan

import improbable.logging.Logger
import improbable.papi.world.AppWorld
import improbable.papi.worldapp.WorldApp
import ragan.natures.PersonNature

class InitialSpawner(appWorld: AppWorld, logger: Logger) extends WorldApp {
  appWorld.entities.spawnEntity(PersonNature())
}

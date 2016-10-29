package ragan

import improbable.logging.Logger
import improbable.papi.entity.{Entity, EntityBehaviour}

class DelegateStateBehaviour(entity: Entity, logger: Logger) extends EntityBehaviour {
  override def onReady(): Unit = {
    logger.warn("behaviour ready on gsim, delegating state")
    entity.delegateState[Health](YoloConstraint)
    logger.warn("delegated state")
  }
}

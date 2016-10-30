package ragan.behaviours

import improbable.logging.Logger
import improbable.papi.entity.{Entity, EntityBehaviour}
import ragan.SharpConstraint
import ragan.Wealth

class DelegateStateBehaviour(entity: Entity, logger: Logger) extends EntityBehaviour {
  override def onReady(): Unit = {
    logger.warn("behaviour ready on gsim, delegating state")
    entity.delegateState[Wealth](SharpConstraint)
    logger.warn("delegated state")
  }
}

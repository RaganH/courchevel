package ragan

import improbable.corelib.natures.{BaseNature, NatureApplication, NatureDescription}
import improbable.corelibrary.transforms.TransformNature
import improbable.papi.entity.EntityPrefab
import improbable.papi.entity.behaviour.EntityBehaviourDescriptor

object HealthNature extends NatureDescription {
  override val dependencies = Set[NatureDescription](BaseNature, TransformNature)

  override def activeBehaviours: Set[EntityBehaviourDescriptor] = Set(descriptorOf[DelegateStateBehaviour])

  def apply(): NatureApplication = {
    application(
      states = Seq(
        Health(10, 500)
      ),
      natures = Seq(
        BaseNature(entityPrefab = EntityPrefab("UNUSED"), isPhysical = false),
        TransformNature()
      )
    )
  }
}


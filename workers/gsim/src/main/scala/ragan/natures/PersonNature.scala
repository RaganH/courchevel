package ragan.natures

import improbable.corelib.natures.{BaseNature, NatureApplication, NatureDescription}
import improbable.corelibrary.transforms.TransformNature
import improbable.papi.entity.EntityPrefab
import improbable.papi.entity.behaviour.EntityBehaviourDescriptor
import ragan.Wealth
import ragan.behaviours.DelegateStateBehaviour

object PersonNature extends NatureDescription {
  override val dependencies = Set[NatureDescription](BaseNature, TransformNature)

  override def activeBehaviours: Set[EntityBehaviourDescriptor] = Set(descriptorOf[DelegateStateBehaviour])

  def apply(): NatureApplication = {
    application(
      states = Seq(
        Wealth(500)
      ),
      natures = Seq(
        BaseNature(entityPrefab = EntityPrefab("Person"), isPhysical = false),
        TransformNature()
      )
    )
  }
}


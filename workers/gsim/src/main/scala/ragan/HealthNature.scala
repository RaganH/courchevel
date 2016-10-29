package ragan

import improbable.corelib.natures.{BaseNature, NatureApplication, NatureDescription}
import improbable.corelibrary.transforms.TransformNature
import improbable.papi.entity.EntityPrefab
import improbable.papi.entity.behaviour.EntityBehaviourDescriptor

/**
  * Created by Harry on 29/10/2016.
  */
object HealthNature extends NatureDescription {
  override val dependencies = Set[NatureDescription](BaseNature, TransformNature)

  override def activeBehaviours: Set[EntityBehaviourDescriptor] = Set()

  def apply(): NatureApplication = {
    application(
      states = Seq(),
      natures = Seq(
        BaseNature(entityPrefab = EntityPrefab("UNUSED"), isPhysical = false),
        TransformNature()
      )
    )
  }
}

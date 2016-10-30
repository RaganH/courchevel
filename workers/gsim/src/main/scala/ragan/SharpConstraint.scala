package ragan

import improbable.fapi.engine.ProtoEngineConstraintSatisfier
import improbable.papi.engine.{EngineConstraints, ProtoEngineConstraint}

case object SharpConstraint extends
  ProtoEngineConstraint(EngineConstraints.makePredicate("sharp"))

case object SharpConstraintSatisfier extends ProtoEngineConstraintSatisfier(
  EngineConstraints.makeClaim(EngineConstraints.makeAtom("sharp"))
)

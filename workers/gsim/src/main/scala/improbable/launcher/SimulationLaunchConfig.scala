package improbable.launcher

import improbable.bridgesettings.SharpBridgeSettings
import improbable.dapi.LaunchConfig
import improbable.fapi.bridge.CompositeBridgeSettingsResolver
import improbable.fapi.engine.{ConstraintToEngineDescriptorResolver, EngineDescriptor}
import improbable.papi.engine.EngineConstraint
import improbable.papi.worldapp.WorldApp
import ragan.{InitialSpawner, SharpConstraint, SharpWorkerDescriptor}

object AutomaticWorkerStartup extends SimulationLaunchConfigWithApps(dynamicallySpoolUpWorkers = true)

/**
  * Use this class to specify the list of apps you want to run when the game starts.
  */
class SimulationLaunchConfigWithApps(dynamicallySpoolUpWorkers: Boolean) extends
  SimulationLaunchConfig(appsToStart = Seq(classOf[InitialSpawner]), dynamicallySpoolUpWorkers)

class SimulationLaunchConfig(appsToStart: Seq[Class[_ <: WorldApp]],
                             dynamicallySpoolUpWorkers: Boolean) extends LaunchConfig(
  appsToStart,
  dynamicallySpoolUpWorkers,
  DefaultBridgeSettingsResolver,
  SharpDescriptorResolver)

object DefaultBridgeSettingsResolver extends CompositeBridgeSettingsResolver(
  SharpBridgeSettings
)

object SharpDescriptorResolver extends SharpConstraintToEngineDescriptorResolver

// TODO(harry) - this was not mentioned in the docs
class SharpConstraintToEngineDescriptorResolver extends ConstraintToEngineDescriptorResolver {
  override def getEngineDescriptorForConstraint(engineConstraint: EngineConstraint): Option[EngineDescriptor] = {
    engineConstraint match {
      case SharpConstraint =>
        Some(SharpWorkerDescriptor())
      case _ =>
        None
    }
  }
}

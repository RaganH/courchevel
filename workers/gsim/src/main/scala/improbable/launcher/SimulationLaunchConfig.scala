package improbable.launcher

import improbable.bridgesettings.SharpBridgeSettings
import improbable.dapi.LaunchConfig
import improbable.fapi.bridge.CompositeBridgeSettingsResolver
import improbable.fapi.engine.EnginePickerStrategy.EnginePickerStrategy
import improbable.fapi.engine.{ConstraintToEngineDescriptorResolver, EngineDescriptor}
import improbable.papi.engine.EngineConstraint
import improbable.papi.worldapp.WorldApp
import improbable.unity.fabric.engine.DownloadableUnityConstraintToEngineDescriptorResolver
import ragan.{InitialSpawner, SharpConstraint, SharpWorkerDescriptor}

/**
  * These are the engine startup configs.
  *
  * ManualWorkerStartup will not start an engines when you start the game.
  * AutomaticWorkerStartup will automatically spool up engines as you need them.
  */
//object ManualWorkerStartup extends SimulationLaunchConfigWithApps(dynamicallySpoolUpWorkers = false)

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
//  DownloadableUnityConstraintToEngineDescriptorResolver,
  FooDescriptorResolver)

object DefaultBridgeSettingsResolver extends CompositeBridgeSettingsResolver(
  SharpBridgeSettings
)

object FooDescriptorResolver extends SharpConstraintToEngineDescriptorResolver

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

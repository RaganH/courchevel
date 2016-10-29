package improbable.bridgesettings

import improbable.fapi.bridge._
import improbable.fapi.network.MultiplexTcpLinkSettings
import improbable.unity.fabric.AuthoritativeEntityOnly
import improbable.unity.fabric.bridge.FSimAssetContextDiscriminator
import improbable.unity.fabric.engine.EnginePlatform
import improbable.unity.fabric.satisfiers.SatisfyPhysics

object SharpBridgeSettings extends BridgeSettingsResolver {

  private val sharpEngineBridgeSettings = BridgeSettings(
    FSimAssetContextDiscriminator(),
    MultiplexTcpLinkSettings(),
    EnginePlatform.UNITY_FSIM_ENGINE,
    SatisfyPhysics,
    AuthoritativeEntityOnly(),
    MetricsEngineLoadPolicy,
    PerEntityOrderedStateUpdateQos
  )

  override def engineTypeToBridgeSettings(engineType: String, metadata: String): Option[BridgeSettings] = {
    if (engineType == EnginePlatform.UNITY_FSIM_ENGINE) {
      Some(sharpEngineBridgeSettings)
    } else {
      None
    }
  }
}
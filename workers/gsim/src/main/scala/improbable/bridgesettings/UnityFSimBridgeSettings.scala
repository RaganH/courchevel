package improbable.bridgesettings

import improbable.fapi.bridge._
import improbable.fapi.network.MultiplexTcpLinkSettings
import improbable.unity.fabric.AuthoritativeEntityOnly
import improbable.unity.fabric.bridge.FSimAssetContextDiscriminator
import improbable.unity.fabric.engine.EnginePlatform
import ragan.SharpConstraintSatisfier

object SharpBridgeSettings extends BridgeSettingsResolver {

  private val sharpEngineBridgeSettings = BridgeSettings(
    FSimAssetContextDiscriminator(), // The recommended default.
    MultiplexTcpLinkSettings(),
    "SharpWorker",//EnginePlatform.UNITY_FSIM_ENGINE,
    SharpConstraintSatisfier,
    AuthoritativeEntityOnly(),
    MetricsEngineLoadPolicy,
    PerEntityOrderedStateUpdateQos
  )

  override def engineTypeToBridgeSettings(engineType: String, metadata: String): Option[BridgeSettings] = {
    if (engineType == "SharpWorker") {
      Some(sharpEngineBridgeSettings)
    } else {
      None
    }
  }
}


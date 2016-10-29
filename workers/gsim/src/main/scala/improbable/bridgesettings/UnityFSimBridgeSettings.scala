package improbable.bridgesettings

import improbable.fapi.bridge._
import improbable.fapi.network.MultiplexTcpLinkSettings
import improbable.unity.fabric.AuthoritativeEntityOnly
import improbable.unity.fabric.bridge.FSimAssetContextDiscriminator
import improbable.unity.fabric.engine.EnginePlatform
import ragan.YoloWorkerConstraintSatisfier

object SharpBridgeSettings extends BridgeSettingsResolver {

  private val sharpEngineBridgeSettings = BridgeSettings(
    FSimAssetContextDiscriminator(), // The recommended default.
    MultiplexTcpLinkSettings(),
    "CsharpWorkerName",//EnginePlatform.UNITY_FSIM_ENGINE,
    YoloWorkerConstraintSatisfier,
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


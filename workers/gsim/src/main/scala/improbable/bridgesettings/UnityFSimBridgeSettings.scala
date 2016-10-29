package improbable.bridgesettings

import improbable.fapi.bridge._
import improbable.fapi.network.MultiplexTcpLinkSettings
import improbable.unity.fabric.AuthoritativeEntityOnly
import improbable.unity.fabric.bridge.FSimAssetContextDiscriminator
import improbable.unity.fabric.engine.EnginePlatform

object SharpBridgeSettings extends BridgeSettingsResolver {

  private val sharpEngineBridgeSettings = BridgeSettings(
    FSimAssetContextDiscriminator(),
    MultiplexTcpLinkSettings(),
    "yoloEnginePlatform",//EnginePlatform.UNITY_FSIM_ENGINE,
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

import improbable.fapi.engine.ProtoEngineConstraintSatisfier
import improbable.papi.engine.EngineConstraints

case object YoloWorkerConstraintSatisfier extends ProtoEngineConstraintSatisfier(
  EngineConstraints.makeClaim(EngineConstraints.makeAtom("yolo"))
)
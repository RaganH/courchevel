package ragan

import java.io.File
import java.nio.file.Path

import improbable.fapi.engine.{DownloadableEngineDescriptor, EngineStartConfig, ProtoEngineConstraintSatisfier}
import improbable.papi.engine.{EngineConstraints, EnginePlatform, ProtoEngineConstraint}

case class MyWorkerDescriptor() extends DownloadableEngineDescriptor {
  override def startCommand(config: EngineStartConfig, enginePath: Path): Seq[String] = {
    Seq(makeExecutablePath(enginePath), "foo_worker", config.receptionistIp)
  }

  private def makeExecutablePath(startPath: Path): String = {
    val absoluteExecutablePath = startPath.resolve("main").toAbsolutePath
    ensureFileIsExecutable(absoluteExecutablePath.toFile)
    absoluteExecutablePath.toString
  }

  private def ensureFileIsExecutable(file: File): Unit = {
    file.setExecutable(true)
    file.setReadable(true)
  }

  override def enginePlatform: EnginePlatform = {
    return "CsharpWorkerName"
  }
}

case object YoloConstraint extends
  ProtoEngineConstraint(EngineConstraints.makePredicate("yolo"))

case object YoloWorkerConstraintSatisfier extends ProtoEngineConstraintSatisfier(
  EngineConstraints.makeClaim(EngineConstraints.makeAtom("yolo"))
)
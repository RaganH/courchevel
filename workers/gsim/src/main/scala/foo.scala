import java.io.File
import java.nio.file.Path

import improbable.fapi.engine.{DownloadableEngineDescriptor, EngineStartConfig}
import improbable.papi.engine.EnginePlatform

case class MyWorkerDescriptor() extends DownloadableEngineDescriptor {
  override def startCommand(config: EngineStartConfig, enginePath: Path): Seq[String] = {
    Seq(makeExecutablePath(enginePath), "foo worker", config.receptionistIp)
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
    return "trololol"
  }
}
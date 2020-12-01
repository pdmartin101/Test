using System.Threading;
using Microsoft.Extensions.Logging;
using PdmBase.Logger;
using PdmBase.Threads;

namespace ConsoleLogTest
{
    class Pdm
    {
        public void Test()
        {
            ILogger logger = PdmLogger.LoggerFactory.CreateLogger<Pdm>();
            logger.LogCritical("Logging critical information.");
            logger.LogError("Logging error information.");
            logger.LogWarning("Logging warning.");
            logger.LogInformation("Logging information.");
            logger.LogDebug("Logging debug information.");
            logger.LogTrace("Logging trace");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            ILogger logger = PdmLogger.LoggerFactory.CreateLogger("Test");
            using (logger.BeginScope("MainWindow"))
            {
                logger.LogCritical("Logging critical information.");
                logger.LogError("Logging error information.");
                logger.LogWarning("Logging warning.");
                logger.LogInformation("Logging information.");
                logger.LogDebug("Logging debug information.");
                logger.LogTrace("Logging trace");
                using (logger.BeginScope("PDM"))
                {
                    var pdm = new Pdm();
                    pdm.Test();
                }
            }
            Thread.Sleep(5000);
            PdmLogger.Shutdown();
            PdmExitThread.Exit();
        }
    }
}

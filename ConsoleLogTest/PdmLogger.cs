using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.PdmConsole;

namespace ConsoleLogTest
{
    public static class PdmLogger
    {
        public static readonly ILoggerFactory LoggerFactory;

        static PdmLogger()
        {
#if TRACE

            var factory = new LoggerFactory();
//            factory.AddProvider(new FileLoggerProvider("D:\\Logs\\h.log"));

            var logger = factory.CreateLogger("TEST");


            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
            var console = false;
            var debug = false;
            var file = false;
            foreach (var child in configuration.GetSection("Logging").GetChildren())
            {
                if (child.Path == "Logging:Console")
                    console = true;
                if (child.Path == "Logging:Debug")
                    debug = true;
                if (child.Path == "Logging:PdmFile")
                    file = true;
            }

            if (console && (LoggingConsole.ConsoleCreater != null))
                LoggingConsole.ConsoleCreater.Create();
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder
                    .AddConfiguration(configuration.GetSection("Logging"))
                    .AddPdmFile()
                    .AddPdmConsole();
            });
#else
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
            });
#endif
        }

        public static void Shutdown()
        {
            LoggerFactory.Dispose();
        }
    }
    public static class LoggingConsole
    {
        public static IPdmConsole ConsoleCreater { get; set; }
    }

    public interface IPdmConsole
    {
        void Create();
    }
}
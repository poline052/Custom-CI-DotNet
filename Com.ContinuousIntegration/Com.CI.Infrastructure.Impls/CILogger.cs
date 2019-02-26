using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.IO;

namespace Com.CI.Infrastructure.Impls
{
    public class CILogger : ICILogger
    {
        private static readonly ILog LogWriter = LogManager.GetLogger("Com CI Logger");

        public void WriteInfo(string info)
        {
            LogWriter.Info(info);
            Console.WriteLine(info);
        }

        public void WriteError(string error, Exception e)
        {
            LogWriter.Error(error, e);
            Console.WriteLine(error);
        }

        public void WriteWarning(string warning, Exception e)
        {
            LogWriter.Error(warning, e);
            Console.WriteLine(warning);
        }

        public static void Setup(string logFilesRootDirectory, string serviceEndPointName, string logLevel)
        {
            var logFileNameFormat = string.Format("{0}.log", serviceEndPointName);

            var logFilePath = Path.Combine(logFilesRootDirectory, serviceEndPointName, logFileNameFormat);

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            hierarchy.Root.RemoveAllAppenders();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %level %logger - %message%newline";
            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender();
            roller.LockingModel = new FileAppender.MinimalLock();
            roller.AppendToFile = true;
            roller.File = logFilePath;
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = 7;
            roller.MaximumFileSize = "20MB";
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = true;
            roller.ActivateOptions();
            roller.Threshold = new LevelMap()[logLevel];

            hierarchy.Root.AddAppender(roller);

            hierarchy.Root.Level = hierarchy.LevelMap[logLevel];
            hierarchy.Configured = true;
        }
    }
}

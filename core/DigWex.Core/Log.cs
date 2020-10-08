using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Text;
using DigWex.Managers;

namespace DigWex
{
  public sealed class Log
  {
    private static readonly Lazy<Log> _instance =
        new Lazy<Log>(() => new Log());

    private static Logger _log;
    private static SentryManager _sentry;

    private Log()
    {
      Initialize();
      _log = LogManager.GetCurrentClassLogger();
      _sentry = SentryManager.Instance;
      _sentry.Init();
    }

    public static Log Instance => _instance.Value;

    public void Init() { }

    private void Initialize()
    {
      var fileTarget = new FileTarget
      {
        Encoding = Encoding.UTF8,
        FileName = "${basedir}/logs/${shortdate}.log",
        Layout = "${longdate}|${level}|${message}" +
          " ${exception:format=tostring}",
        ArchiveFileName = "${basedir}/logs/{#}.log",
        KeepFileOpen = false,
        ArchiveDateFormat = "yyyy-MM-dd",
        ArchiveNumbering = ArchiveNumberingMode.Date,
        ArchiveEvery = FileArchivePeriod.Day,
        MaxArchiveFiles = 7
      };

      var consoleTarget = new ConsoleTarget
      {
        Layout = "${message} ${exception:format=tostring}"
      };

      var ruleFile = new LoggingRule("*", LogLevel.Debug, fileTarget);
      var ruleConsole = new LoggingRule("*", LogLevel.Debug, consoleTarget);

      var config = new LoggingConfiguration();
      config.AddTarget("file", fileTarget);
      config.AddTarget("console", consoleTarget);
      config.LoggingRules.Add(ruleFile);
      config.LoggingRules.Add(ruleConsole);

      LogManager.Configuration = config;
    }

    public static void Info(string message)
    {
      _log.Info(message);
    }

    public static void Warn(string message)
    {
      _log.Warn(message);
    }

    public static void Warn(Exception exception, string message)
    {
      _log.Warn(exception, message);
    }

    public static void Warn(Exception exception)
    {
      _log.Warn(exception);
    }

    public static void Fatal(Exception exception)
    {
      _log.Fatal(exception);
      //try
      //{
      //    _sentry.Fatal(exception);
      //}
      //catch { }
    }

    public static void Debug(string message)
    {
      _log.Debug(message);
    }

    public static void Error(string message)
    {
      _log.Error(message);
    }

    public static void Error(Exception exception, string message)
    {
      _log.Error(exception, message);
    }
  }
}
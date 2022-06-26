using NLog;
using NLog.Config;
using NLog.Targets;
using System.Text;

namespace EtumrepMMO.Server;
// Mostly borrowed from kwsch's SysBot
// https://github.com/kwsch/SysBot.NET/blob/master/SysBot.Base/Util/LogUtil.cs

public static class LogUtil
{
    public static readonly List<Action<string, string>> Forwarders = new();
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public static DateTime LastLogged { get; private set; } = DateTime.Now;

    static LogUtil()
    {
        var config = new LoggingConfiguration();
        Directory.CreateDirectory("logs");
        var logfile = new FileTarget("logfile")
        {
            FileName = Path.Combine("logs", "EtumrepMMO.Server.txt"),
            ConcurrentWrites = true,
            ArchiveEvery = FileArchivePeriod.Day,
            ArchiveNumbering = ArchiveNumberingMode.Date,
            ArchiveFileName = Path.Combine("logs", "EtumrepMMO.Server.{#}.txt"),
            ArchiveDateFormat = "yyyy-MM-dd",
            MaxArchiveFiles = 7,
            Encoding = Encoding.Unicode,
            WriteBom = true,
        };
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
        LogManager.Configuration = config;
    }

    public static void Log(string text, string identity) => LogInternal(LogLevel.Info, text, identity);

    private static void LogInternal(LogLevel level, string text, string identity)
    {
        foreach (var fw in Forwarders)
            fw(text, identity);

        Logger.Log(level, $"{identity} {text}");
        LastLogged = DateTime.Now;
    }
}

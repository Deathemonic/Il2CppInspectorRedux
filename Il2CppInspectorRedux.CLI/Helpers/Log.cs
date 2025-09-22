using Kokuban;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Il2CppInspectorRedux.CLI.Helpers;

public static class CustomLogLevel
{
    public const LogLevel Success = (LogLevel)15;
}

public static class Log
{
    private static ILoggerFactory? _loggerFactory;
    private static ILogger? _logger;
    private static bool _isInitialized;

    public static ILogger Global
    {
        get
        {
            EnsureInitialized();
            return _logger!;
        }
    }

    private static void EnsureInitialized()
    {
        if (_isInitialized) return;

        _loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Information);

            logging.AddZLoggerConsole(options =>
            {
                options.UsePlainTextFormatter(formatter =>
                {
                    formatter.SetPrefixFormatter($"{0} {1} ",
                        (in template, in info) =>
                        {
                            var timestamp = Chalk.Gray + info.Timestamp.Local.ToString("HH:mm:ss");
                            var logLevel = GetColoredLogLevel(info.LogLevel);
                            template.Format(timestamp, logLevel);
                        });
                });
                options.LogToStandardErrorThreshold = LogLevel.Error;
            });
        });

        _logger = _loggerFactory.CreateLogger("Il2CppInspectorRedux");
        _isInitialized = true;
    }

    private static string GetColoredLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => Chalk.Magenta + "[TRC]",
            LogLevel.Debug => Chalk.Cyan + "[DBG]",
            LogLevel.Information => Chalk.Blue + "[INF]",
            LogLevel.Warning => Chalk.Yellow + "[WRN]",
            LogLevel.Error => Chalk.Red + "[ERR]",
            LogLevel.Critical => Chalk.BgRed.White + "[CRT]",
            CustomLogLevel.Success => Chalk.Green + "[SUC]",
            _ => Chalk.White + "[???]"
        };
    }


    public static void Info(string message)
    {
        EnsureInitialized();
        _logger!.ZLogInformation($"{message}");
    }

    public static void Success(string message)
    {
        EnsureInitialized();
        _logger!.Log(CustomLogLevel.Success, "{Message}", message);
    }

    public static void Error(string message)
    {
        EnsureInitialized();
        _logger!.ZLogError($"{message}");
    }

    public static void Error(string message, Exception exception)
    {
        EnsureInitialized();
        _logger!.ZLogError(exception, $"{message}");
    }
}

public static partial class LogMessages
{
    [ZLoggerMessage(LogLevel.Information, "Export completed successfully!")]
    public static partial void LogExportComplete(this ILogger logger);
}
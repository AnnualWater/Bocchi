using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;
using YukariToolBox.LightLog;
using LogLevel = YukariToolBox.LightLog.LogLevel;

namespace Bocchi.SoraBotCore;

public class SoraLogService : ILogService, ISingletonDependency
{
    public ILogger<SoraLogService> Logger { get; set; }

    public SoraLogService()
    {
        Logger = NullLogger<SoraLogService>.Instance;
    }

    public void SetLogService()
    {
        Log.LogConfiguration
#if DEBUG
            .SetLogLevel(LogLevel.Debug)
#else
            .SetLogLevel(LogLevel.Info)
#endif
            .AddLogService(this);
    }

    public void Info(string source, string message)
    {
        Logger.LogInformation("[{Source}]{Message}", source, message);
    }

    public void Info<T>(string source, string message, T context)
    {
        Logger.LogInformation("[{Source}:{Context}]{Message}", source, context, message);
    }

    public void Warning(string source, string message)
    {
        Logger.LogWarning("[{Source}]{Message}", source, message);
    }

    public void Warning<T>(string source, string message, T context)
    {
        Logger.LogWarning("[{Source}:{Context}]{Message}", source, context, message);
    }

    public void Error(string source, string message)
    {
        Logger.LogError("[{Source}]{Message}", source, message);
    }

    public void Error(Exception exception, string source, string message)
    {
        Logger.LogError(exception, "[{Source}]{Message}", source, message);
    }

    public void Error<T>(string source, string message, T context)
    {
        Logger.LogError("[{Source}:{Context}]{Message}", source, context, message);
    }

    public void Error<T>(Exception exception, string source, string message, T context)
    {
        Logger.LogError(exception, "[{Source}:{Context}]{Message}", source, context, message);
    }

    public void Fatal(Exception exception, string source, string message)
    {
        Logger.LogError("[{Source}]{Message}\n{Exception}", source, message, exception.Message);
    }

    public void Fatal<T>(Exception exception, string source, string message, T context)
    {
        Logger.LogError(exception, "[{Source}:{Context}]{Message}", source, context, message);
    }

    public void Debug(string source, string message)
    {
        Logger.LogDebug("[{Source}]{Message}", source, message);
    }

    public void Debug<T>(string source, string message, T context)
    {
        Logger.LogDebug("[{Source}:{Context}]{Message}", source, context, message);
    }

    public void Verbose(string source, string message)
    {
        Logger.LogTrace("[{Source}]{Message}", source, message);
    }

    public void Verbose<T>(string source, string message, T context)
    {
        Logger.LogTrace("[{Source}:{Context}]{Message}", source, context, message);
    }

    public void UnhandledExceptionLog(UnhandledExceptionEventArgs args)
    {
        Logger.LogError("[UnhandledExceptionLog]{Message}", args.ExceptionObject);
    }

    public void SetCultureInfo(CultureInfo cultureInfo)
    {
        Logger.LogError("[CultureInfo]{Message}", cultureInfo.Name);
    }
}
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging.PdmConsole
{
    internal class FileLogger : ILogger
    {
        private static readonly string LoglevelPadding = ": ";
        private static readonly string MessagePadding;
        private static readonly string NewLineWithMessagePadding;

        // ConsoleColor does not have a value to specify the 'Default' color

        private readonly string _name;
        private readonly FileLoggerProcessor _queueProcessor;

        [ThreadStatic]
        private static StringBuilder _logBuilder;

        static FileLogger()
        {
            var logLevelString = GetLogLevelString(LogLevel.Information);
            MessagePadding = new string(' ', logLevelString.Length + LoglevelPadding.Length);
            NewLineWithMessagePadding = Environment.NewLine + MessagePadding;
        }

        internal FileLogger(string name, FileLoggerProcessor loggerProcessor)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            _name = name;
            _queueProcessor = loggerProcessor;
        }

        internal IExternalScopeProvider ScopeProvider { get; set; }

        internal FileLoggerOptions Options { get; set; }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                WriteMessage(logLevel, _name, eventId.Id, message, exception);
            }
        }

        public virtual void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
             var logBuilder = _logBuilder;
            _logBuilder = null;

            if (logBuilder == null)
            {
                logBuilder = new StringBuilder();
            }

            LogMessageEntry2 entry2 = CreateDefaultLogMessage(logBuilder, logLevel, logName, eventId, message, exception);
            _queueProcessor.EnqueueMessage(entry2);

            logBuilder.Clear();
            if (logBuilder.Capacity > 1024)
            {
                logBuilder.Capacity = 1024;
            }
            _logBuilder = logBuilder;
        }

        private LogMessageEntry2 CreateDefaultLogMessage(StringBuilder logBuilder, LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            // Example:
            // INFO: ConsoleApp.Program[10]
            //       Request received

            var logLevelString = GetLogLevelString(logLevel);
            // category and event id
            logBuilder.Append(LoglevelPadding);
            logBuilder.Append(logName);
            logBuilder.Append("[");
            logBuilder.Append(eventId);
            logBuilder.AppendLine("]");

            // scope information
            GetScopeInformation(logBuilder, multiLine: true);

            if (!string.IsNullOrEmpty(message))
            {
                // message
                logBuilder.Append(MessagePadding);

                var len = logBuilder.Length;
                logBuilder.AppendLine(message);
                logBuilder.Replace(Environment.NewLine, NewLineWithMessagePadding, len, message.Length);
            }

            // Example:
            // System.InvalidOperationException
            //    at Namespace.Class.Function() in File:line X
            if (exception != null)
            {
                // exception message
                logBuilder.AppendLine(exception.ToString());
            }

            var timestampFormat = Options.TimestampFormat;

            return new LogMessageEntry2(
                message: logBuilder.ToString(),
                timeStamp: timestampFormat != null ? DateTime.Now.ToString(timestampFormat) : null,
                levelString: logLevelString
            );
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }
        public IDisposable BeginScope<TState>(TState state) => null;
        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "trce";
                case LogLevel.Debug:
                    return "dbug";
                case LogLevel.Information:
                    return "info";
                case LogLevel.Warning:
                    return "warn";
                case LogLevel.Error:
                    return "fail";
                case LogLevel.Critical:
                    return "crit";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
        private void GetScopeInformation(StringBuilder stringBuilder, bool multiLine)
        {
            var scopeProvider = ScopeProvider;
            if (Options.IncludeScopes && scopeProvider != null)
            {
                var initialLength = stringBuilder.Length;

                scopeProvider.ForEachScope((scope, state) =>
                {
                    var (builder, paddAt) = state;
                    var padd = paddAt == builder.Length;
                    if (padd)
                    {
                        builder.Append(MessagePadding);
                        builder.Append("=> ");
                    }
                    else
                    {
                        builder.Append(" => ");
                    }
                    builder.Append(scope);
                }, (stringBuilder, multiLine ? initialLength : -1));

                if (stringBuilder.Length > initialLength && multiLine)
                {
                    stringBuilder.AppendLine();
                }
            }
        }

    }
}

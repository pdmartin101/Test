// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging.PdmConsole
{
    /// <summary>
    /// A provider of <see cref="FileLogger"/> instances.
    /// </summary>
    [ProviderAlias("PdmFile")]
    public class FileLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly IOptionsMonitor<FileLoggerOptions> _options;
        private readonly ConcurrentDictionary<string, FileLogger> _loggers;
        private readonly FileLoggerProcessor _messageQueue;

        private readonly IDisposable _optionsReloadToken;
        private IExternalScopeProvider _scopeProvider;

        /// <summary>
        /// Creates an instance of <see cref="FileLoggerProvider"/>.
        /// </summary>
        /// <param name="options">The options to create <see cref="FileLogger"/> instances with.</param>
        public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options)
        {
            _options = options;
            _loggers = new ConcurrentDictionary<string, FileLogger>();

            ReloadLoggerOptions(options.CurrentValue);
            _optionsReloadToken = _options.OnChange(ReloadLoggerOptions);

            _messageQueue = new FileLoggerProcessor();
            _messageQueue.OutStream = new StreamWriter(options.CurrentValue.LogFile, false);
        }

        private void ReloadLoggerOptions(FileLoggerOptions options)
        {
            foreach (var logger in _loggers)
            {
                logger.Value.Options = options;
            }
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, loggerName => new FileLogger(name, _messageQueue)
            {
                Options = _options.CurrentValue,
                ScopeProvider = _scopeProvider
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
            _messageQueue.Dispose();
        }

        /// <inheritdoc />
        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;

            foreach (var logger in _loggers)
            {
                logger.Value.ScopeProvider = _scopeProvider;
            }

        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Logging.PdmConsole
{
    /// <summary>
    /// A provider of <see cref="PdmConsoleLogger"/> instances.
    /// </summary>
    [ProviderAlias("PdmConsole")]
    public class PdmConsoleLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly IOptionsMonitor<PdmConsoleLoggerOptions> _options;
        private readonly ConcurrentDictionary<string, PdmConsoleLogger> _loggers;
        private readonly PdmConsoleLoggerProcessor _messageQueue;

        private IDisposable _optionsReloadToken;
        private IExternalScopeProvider _scopeProvider = null;

        /// <summary>
        /// Creates an instance of <see cref="ConsoleLoggerProvider"/>.
        /// </summary>
        /// <param name="options">The options to create <see cref="ConsoleLogger"/> instances with.</param>
        public PdmConsoleLoggerProvider(IOptionsMonitor<PdmConsoleLoggerOptions> options)
        {
            _options = options;
            _loggers = new ConcurrentDictionary<string, PdmConsoleLogger>();

            ReloadLoggerOptions(options.CurrentValue);
            _optionsReloadToken = _options.OnChange(ReloadLoggerOptions);

            _messageQueue = new PdmConsoleLoggerProcessor();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _messageQueue.Console = new WindowsLogConsole();
                _messageQueue.ErrorConsole = new WindowsLogConsole(stdErr: true);
            }
            else
            {
                _messageQueue.Console = new AnsiLogConsole(new AnsiSystemConsole());
                _messageQueue.ErrorConsole = new AnsiLogConsole(new AnsiSystemConsole(stdErr: true));
            }
        }

        private void ReloadLoggerOptions(PdmConsoleLoggerOptions options)
        {
            foreach (var logger in _loggers)
            {
                logger.Value.Options = options;
            }
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, loggerName => new PdmConsoleLogger(name, _messageQueue)
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

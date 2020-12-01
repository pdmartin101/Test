// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.Logging.PdmConsole
{
    /// <summary>
    /// Options for a <see cref="PdmConsoleLogger"/>.
    /// </summary>
    public class PdmConsoleLoggerOptions
    {
        private PdmConsoleLoggerFormat _format = PdmConsoleLoggerFormat.Default;

        /// <summary>
        /// Includes scopes when <code>true</code>.
        /// </summary>
        public bool IncludeScopes { get; set; }

        /// <summary>
        /// Disables colors when <code>true</code>.
        /// </summary>
        public bool DisableColors { get; set; }

        /// <summary>
        /// Gets or sets log message format. Defaults to <see cref="ConsoleLoggerFormat.Default" />.
        /// </summary>
        public PdmConsoleLoggerFormat Format
        {
            get => _format;
            set
            {
                if (value < PdmConsoleLoggerFormat.Default || value > PdmConsoleLoggerFormat.Systemd)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _format = value;
            }
        }

        /// <summary>
        /// Gets or sets value indicating the minimum level of messaged that would get written to <c>Console.Error</c>.
        /// </summary>
        public LogLevel LogToStandardErrorThreshold { get; set; } = LogLevel.None;

        /// <summary>
        /// Gets or sets format string used to format timestamp in logging messages. Defaults to <c>null</c>.
        /// </summary>
        public string TimestampFormat { get; set; }
    }
}

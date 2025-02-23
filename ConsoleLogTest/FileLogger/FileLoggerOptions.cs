// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging.PdmConsole
{
    /// <summary>
    /// Options for a <see cref="FileLogger"/>.
    /// </summary>
    public class FileLoggerOptions
    {

        /// <summary>
        /// Includes scopes when <code>true</code>.
        /// </summary>
        public bool IncludeScopes { get; set; }

        /// <summary>
        /// Gets or sets format string used to format timestamp in logging messages. Defaults to <c>null</c>.
        /// </summary>
        public string TimestampFormat { get; set; }
        public string LogFile { get; set; }
    }
}

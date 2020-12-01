// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging.PdmConsole
{
    internal readonly struct LogMessageEntry2
    {
        public LogMessageEntry2(string message, string timeStamp = null, string levelString = null)
        {
            TimeStamp = timeStamp;
            LevelString = levelString;
            Message = message;
        }

        public readonly string TimeStamp;
        public readonly string LevelString;
        public readonly string Message;
    }
}

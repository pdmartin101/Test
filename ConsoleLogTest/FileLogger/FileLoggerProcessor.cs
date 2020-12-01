// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging.PdmConsole
{
    internal class FileLoggerProcessor : IDisposable
    {
        private const int MaxQueuedMessages = 1024;

        private readonly BlockingCollection<LogMessageEntry2> _messageQueue = new BlockingCollection<LogMessageEntry2>(MaxQueuedMessages);
        private readonly Thread _outputThread;

        public StreamWriter OutStream;

        public FileLoggerProcessor()
        {
            // Start Console message queue processor
            _outputThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "Console logger queue processing thread"
            };
            _outputThread.Start();
        }

        public virtual void EnqueueMessage(LogMessageEntry2 message)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    _messageQueue.Add(message);
                    return;
                }
                catch (InvalidOperationException) { }
            }

            // Adding is completed so just log the message
            try
            {
                WriteMessage(message);            
            }
            catch (Exception) { }
        }

        // for testing
        protected virtual void WriteMessage(LogMessageEntry2 message)
        {
            var console = OutStream;

            if (message.TimeStamp != null) console.Write(message.TimeStamp);

            if (message.LevelString != null) console.Write(message.LevelString);

            console.Write(message.Message);
            console.Flush();
        }

        private void ProcessLogQueue()
        {
            try
            {
                foreach (var message in _messageQueue.GetConsumingEnumerable())
                    WriteMessage(message);
            }
            catch
            {
                try
                {
                    _messageQueue.CompleteAdding();
                }
                catch { }
            }
        }

        public void Dispose()
        {
            _messageQueue.CompleteAdding();

            try
            {
                _outputThread.Join(1500); // with timeout in-case Console is locked by user input
            }
            catch (ThreadStateException) { }
        }
    }
}

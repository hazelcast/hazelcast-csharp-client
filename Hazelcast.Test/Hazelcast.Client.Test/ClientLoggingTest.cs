﻿// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Hazelcast.Config;
using Hazelcast.Logging;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientLoggingTest
    {
        [Test]
		public void TestConsoleLoggingInvalidLevel()
		{
			Assert.Throws<ConfigurationException>(() =>
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "asdf");
            var logger = new ConsoleLogFactory().GetLogger("logger");

            Assert.IsFalse(logger.IsFinestEnabled());
        });
		}

        [Test]
        public void TestConsoleLoggingLevel()
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            var logger = new ConsoleLogFactory().GetLogger("logger");
            var original = Console.Out;
            try
            {
                var memoryStream = new MemoryStream();
                var streamWriter = new StreamWriter(memoryStream);
                Console.SetOut(streamWriter);
                var message1 = TestSupport.RandomString();
                var message2 = TestSupport.RandomString();
                logger.Info(message1);
                logger.Finest(message2);
                streamWriter.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);
                var log = new StreamReader(memoryStream).ReadToEnd();

                Assert.IsFalse(logger.IsFinestEnabled());
                Assert.That(logger.GetLevel(), Is.EqualTo(LogLevel.Info));
                Assert.That(log, Does.Contain(message1));
                Assert.That(log, Does.Not.Contain(message2));
            }
            finally
            {
                Console.SetOut(original);
                Environment.SetEnvironmentVariable("hazelcast.logging.level", null);
            }
        }

        [Test]
        public void TestCustomLogger()
        {
            var oldLogger = Logger.GetLoggerFactory();
            try
            {
                Logger.SetLoggerFactory(new CustomLogFactory());
                var logger = Logger.GetLogger("test");

                logger.Info("test message");
                var exception = new ArgumentException();
                logger.Severe("got exception", exception);

                Assert.IsInstanceOf<CustomLogFactory.CustomLogger>(logger);

                var customLogger = (CustomLogFactory.CustomLogger) logger;

                Assert.AreEqual(new Tuple<LogLevel, string, Exception>(LogLevel.Info, "test message", null),
                    (customLogger.Logs[0]));
                Assert.AreEqual(new Tuple<LogLevel, string, Exception>(LogLevel.Severe, "got exception", exception),
                    (customLogger.Logs[1]));
            }
            finally
            {
                Logger.SetLoggerFactory(oldLogger);                
            }
        }

        [Test]
        public void TestTraceLogger()
        {
            var stream = new MemoryStream();
            var listener = new TextWriterTraceListener(stream);
            Trace.Listeners.Add(listener);
            var logger = new TraceLogFactory().GetLogger("logger");
            var logMsg = TestSupport.RandomString();
            logger.Finest(logMsg);
            logger.Info(logMsg);
            logger.Warning(logMsg);
            logger.Severe(logMsg);

            listener.Flush();

            stream.Seek(0, SeekOrigin.Begin);
            var log = new StreamReader(stream).ReadToEnd();
            Assert.That(log, Does.Contain("Information: 0 : " + logMsg));
            Assert.That(log, Does.Contain("Warning: 0 : " + logMsg));
            Assert.That(log, Does.Contain("Error: 0 : " + logMsg));
        }
    }

    public class CustomLogFactory : ILoggerFactory
    {
        public ILogger GetLogger(string name)
        {
            return new CustomLogger {Name = name};
        }

        internal class CustomLogger : AbstractLogger
        {
            public List<Tuple<LogLevel, string, Exception>> Logs = new List<Tuple<LogLevel, string, Exception>>();
            public string Name { get; set; }

            public override LogLevel GetLevel()
            {
                return LogLevel.Info;
            }


            public override bool IsLoggable(LogLevel arg1)
            {
                return true;
            }

            public override void Log(LogLevel arg1, string arg2)
            {
                Logs.Add(new Tuple<LogLevel, string, Exception>(arg1, arg2, null));
            }

            public override void Log(LogLevel arg1, string arg2, Exception arg3)
            {
                Logs.Add(new Tuple<LogLevel, string, Exception>(arg1, arg2, arg3));
            }
        }
    }
}
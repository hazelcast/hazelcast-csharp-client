// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Logging
{
    [TestFixture]
    public class LoggingOptionsTests
    {
        [Test]
        public void CloneOptions()
        {
            static ILoggerFactory Creator() => new NullLoggerFactory();

            var options = new LoggingOptions();
            var creator = new Func<ILoggerFactory>(Creator);
            options.LoggerFactory.Creator = creator;
            var clone = options.Clone();
            Assert.That(clone.LoggerFactory.Creator, Is.SameAs(creator));
        }
    }
}

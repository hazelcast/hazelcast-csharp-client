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

using Hazelcast.Messaging;
using NUnit.Framework;

namespace Hazelcast.Tests.Messaging
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Options()
        {
            var options = new MessagingOptions
            {
                InvocationTimeoutMilliseconds = 123,
                MinRetryDelayMilliseconds = 456,
                MaxFastInvocationCount = 789
            };

            Assert.That(options.InvocationTimeoutMilliseconds, Is.EqualTo(123));
            Assert.That(options.MinRetryDelayMilliseconds, Is.EqualTo(456));
            Assert.That(options.MaxFastInvocationCount, Is.EqualTo(789));

            var clone = options.Clone();

            Assert.That(clone.InvocationTimeoutMilliseconds, Is.EqualTo(123));
            Assert.That(clone.MinRetryDelayMilliseconds, Is.EqualTo(456));
            Assert.That(clone.MaxFastInvocationCount, Is.EqualTo(789));
        }

        [Test]
        public void FlagsExtensions()
        {
            Assert.That(FrameFlags.Default.ToBetterString(), Is.EqualTo("Default"));

            Assert.That(FrameFlags.BeginStruct.ToBetterString(), Is.EqualTo("BeginStruct"));
            Assert.That((FrameFlags.BeginStruct | FrameFlags.Final).ToBetterString(), Is.EqualTo("BeginStruct, Final"));

            Assert.That(ClientMessageFlags.BackupAware.ToBetterString(), Is.EqualTo("BackupAware"));
            Assert.That((ClientMessageFlags.BackupAware | ClientMessageFlags.Event).ToBetterString(), Is.EqualTo("BackupAware, Event"));

            Assert.That((FrameFlags.Final | (FrameFlags) ClientMessageFlags.Event).ToBetterString(), Is.EqualTo("Final, Event"));
            Assert.That(((ClientMessageFlags) FrameFlags.Final | ClientMessageFlags.Event).ToBetterString(), Is.EqualTo("Final, Event"));
        }
    }
}

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
using Hazelcast.Exceptions;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Exceptions
{
    [TestFixture]
    public class ExceptionsTests
    {
        [Test]
        public void HazelcastExceptionConstructors()
        {
            _ = new HazelcastException();
            _ = new HazelcastException("exception");
            _ = new HazelcastException(new Exception("bang"));
            var e = new HazelcastException("exception", new Exception("bang"));

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));

            e = e.SerializeAndDeSerialize();

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
        }

        [Test]
        public void AuthenticationExceptionConstructors()
        {
            _ = new AuthenticationException();
            _ = new AuthenticationException("exception");
            _ = new AuthenticationException(new Exception("bang"));
            var e = new AuthenticationException("exception", new Exception("bang"));

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));

            e = e.SerializeAndDeSerialize();

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
        }

        [Test]
        public void ClientNotConnectedExceptionConstructors()
        {
            _ = new ClientOfflineException(ClientState.Shutdown);
            _ = new ClientOfflineException("exception", ClientState.Shutdown);
            _ = new ClientOfflineException(new Exception("bang"), ClientState.Shutdown);

            var e = new ClientOfflineException("exception", new Exception("bang"), ClientState.Shutdown);

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
            Assert.That(e.State, Is.EqualTo(ClientState.Shutdown));

            e = e.SerializeAndDeSerialize();

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
            Assert.That(e.State, Is.EqualTo(ClientState.Shutdown));
        }

        [Test]
        public void ConnectionExceptionConstructors()
        {
            _ = new ConnectionException();
            _ = new ConnectionException("exception");
            _ = new ConnectionException(new Exception("bang"));
            var e = new ConnectionException("exception", new Exception("bang"));

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));

            e = e.SerializeAndDeSerialize();

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
        }

        [Test]
        public void TargetDisconnectedExceptionConstructors()
        {
            _ = new TargetDisconnectedException();
            _ = new TargetDisconnectedException("exception");
            _ = new TargetDisconnectedException(new Exception("bang"));
            var e = new TargetDisconnectedException("exception", new Exception("bang"));

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));

            e = e.SerializeAndDeSerialize();

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
        }

        [Test]
        public void TaskTimeoutExceptionConstructors()
        {
            _ = new TaskTimeoutException();
            _ = new TaskTimeoutException("exception");
            var e = new TaskTimeoutException("exception", new Exception("bang"));

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));

            e = e.SerializeAndDeSerialize();

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
        }
    }
}

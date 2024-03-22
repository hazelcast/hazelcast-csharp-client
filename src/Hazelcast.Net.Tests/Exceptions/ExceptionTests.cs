// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Protocol;
using Hazelcast.Serialization;
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

            #if !NET8_0_OR_GREATER
            e = e.SerializeAndDeSerialize();
#endif

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

            #if !NET8_0_OR_GREATER
            e = e.SerializeAndDeSerialize();
#endif

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
        }

        [Test]
        public void ClientNotAllowedInClusterExceptionConstructors()
        {
            _ = new ClientNotAllowedInClusterException();
            _ = new ClientNotAllowedInClusterException("exception");
            _ = new ClientNotAllowedInClusterException(new Exception("bang"));
            var e = new ClientNotAllowedInClusterException("exception", new Exception("bang"));

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));

            #if !NET8_0_OR_GREATER
            e = e.SerializeAndDeSerialize();
#endif

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
        }

        [Test]
        public void RemoteExceptionConstructors()
        {
            _ = new RemoteException();
            _ = new RemoteException("exception");
            var e = new RemoteException("exception", new Exception("bang"));

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));

            #if !NET8_0_OR_GREATER
            e = e.SerializeAndDeSerialize();
#endif

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
        }

        [Test]
        public void TargetUnreachableExceptionConstructors()
        {
            _ = new TargetUnreachableException();
            _ = new TargetUnreachableException("exception");
            _ = new TargetUnreachableException(new Exception("bang"));
            var e = new TargetUnreachableException("exception", new Exception("bang"));

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));

            #if !NET8_0_OR_GREATER
            e = e.SerializeAndDeSerialize();
#endif

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
        }

        [Test]
        public void UnknownCompactSchemaExceptionConstructors()
        {
            var e = new UnknownCompactSchemaException(1);

            Assert.True(e.Message.StartsWith("Unknown compact"));
            Assert.AreEqual(1, e.SchemaId);
            #if !NET8_0_OR_GREATER
            e = e.SerializeAndDeSerialize();
#endif

            Assert.True(e.Message.StartsWith("Unknown compact"));
            Assert.AreEqual(1, e.SchemaId);
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

            #if !NET8_0_OR_GREATER
            e = e.SerializeAndDeSerialize();
#endif

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

            #if !NET8_0_OR_GREATER
            e = e.SerializeAndDeSerialize();
#endif

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

            #if !NET8_0_OR_GREATER
            e = e.SerializeAndDeSerialize();
#endif

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

            #if !NET8_0_OR_GREATER
            e = e.SerializeAndDeSerialize();
#endif

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
        }
    }
}

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
            _ = new ClientNotConnectedException();
            _ = new ClientNotConnectedException("exception");
            _ = new ClientNotConnectedException(new Exception("bang"));
            var e = new ClientNotConnectedException("exception", new Exception("bang"));

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));

            e = e.SerializeAndDeSerialize();

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
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
    }
}

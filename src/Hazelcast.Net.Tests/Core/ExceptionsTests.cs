using System;
using Hazelcast.Core;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class ExceptionsTests
    {
        [Test]
        public void ServiceFactoryExceptionConstructors()
        {
            _ = new ServiceFactoryException();
            _ = new ServiceFactoryException("exception");
            _ = new ServiceFactoryException(new Exception("bang"));
            var e = new ServiceFactoryException("exception", new Exception("bang"));

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

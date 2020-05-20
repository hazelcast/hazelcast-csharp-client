using System;
using System.Collections.Generic;
using Hazelcast.Clustering;
using Hazelcast.Exceptions;
using Hazelcast.Protocol.Data;
using NUnit.Framework;

namespace Hazelcast.Tests
{
    [TestFixture]
    public class ClientProtocolExceptionTests
    {
        // These tests are not meant to fail but to validate how exceptions look like.

        [Test]
        [Explicit("Throws intentionally.")]
        public void Test()
        {
            try
            {
                ThrowClientProtocolException(ClientProtocolErrors.IllegalState);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                throw;
            }
        }

        [Test]
        [Explicit("Throws intentionally.")]
        public void TestRetryable()
        {
            try
            {
                ThrowClientProtocolException(ClientProtocolErrors.MemberLeft);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                throw;
            }
        }

        [Test]
        [Explicit("Throws intentionally.")]
        public void TestInner()
        {
            try
            {
                ThrowClientProtocolExceptionWithInner(ClientProtocolErrors.MemberLeft);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                throw;
            }
        }

        private static void ThrowClientProtocolException(ClientProtocolErrors error)
        {
            var stackTraceElements = new List<StackTraceElement>();
            for (var i = 0; i < 5; i++)
                stackTraceElements.Add(new StackTraceElement("className_" + i, "methodName_" + i, "fileName_" + i, i));

            var errorHolder = new ErrorHolder((int) error, "className", "message", stackTraceElements);

            var exception = ClientProtocolExceptions.CreateException(new[] { errorHolder });

            throw exception;
        }

        private static void ThrowClientProtocolExceptionWithInner(ClientProtocolErrors error)
        {
            var stackTraceElements = new List<StackTraceElement>();
            for (var i = 0; i < 5; i++)
                stackTraceElements.Add(new StackTraceElement("className_" + i, "methodName_" + i, "fileName_" + i, i));

            var errorHolder = new ErrorHolder((int)error, "className", "message", stackTraceElements);

            var exception = ClientProtocolExceptions.CreateException(new[] { errorHolder, errorHolder });

            throw exception;
        }
    }
}

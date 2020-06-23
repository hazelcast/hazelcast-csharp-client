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
using System.Collections.Generic;
using Hazelcast.Clustering;
using Hazelcast.Exceptions;
using Hazelcast.Protocol;
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
                ThrowClientProtocolException(ClientProtocolError.IllegalState);
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
                ThrowClientProtocolException(ClientProtocolError.MemberLeft);
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
                ThrowClientProtocolExceptionWithInner(ClientProtocolError.MemberLeft);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                throw;
            }
        }

        private static void ThrowClientProtocolException(ClientProtocolError error)
        {
            var stackTraceElements = new List<StackTraceElement>();
            for (var i = 0; i < 5; i++)
                stackTraceElements.Add(new StackTraceElement("className_" + i, "methodName_" + i, "fileName_" + i, i));

            var errorHolder = new ErrorHolder((int) error, "className", "message", stackTraceElements);

            var exception = ClientProtocolExceptions.CreateException(new[] { errorHolder });

            throw exception;
        }

        private static void ThrowClientProtocolExceptionWithInner(ClientProtocolError error)
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

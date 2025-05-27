// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Protocol.Models;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Exceptions
{
    [TestFixture]
    public class RemoteExceptionTests
    {
        [Test]
        public void Constructors()
        {
            _ = new RemoteException();
            _ = new RemoteException("exception");
            _ = new RemoteException("exception", new Exception("bang"));
            _ = new RemoteException(Guid.Empty, RemoteError.Undefined, true);
            _ = new RemoteException(Guid.Empty, RemoteError.Undefined, "exception");
            _ = new RemoteException(Guid.Empty, RemoteError.Undefined, "exception", true);
            _ = new RemoteException(Guid.Empty, RemoteError.Undefined, new Exception("bang"));
            _ = new RemoteException(Guid.Empty, RemoteError.Undefined, "exception", new Exception("bang"));
            _ = new RemoteException(Guid.Empty, RemoteError.Undefined, new Exception("bang"), true);
            var e = new RemoteException(Guid.Empty, RemoteError.AccessControl, "exception", new Exception("bang"), "", true);

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
            Assert.That(e.Retryable, Is.True);
            Assert.That(e.Error, Is.EqualTo(RemoteError.AccessControl));

            #if !NET8_0_OR_GREATER
            e = e.SerializeAndDeSerialize();
#endif

            Assert.That(e.Message, Is.EqualTo("exception"));
            Assert.That(e.InnerException, Is.Not.Null);
            Assert.That(e.InnerException.Message, Is.EqualTo("bang"));
            Assert.That(e.Retryable, Is.True);
            Assert.That(e.Error, Is.EqualTo(RemoteError.AccessControl));

            Assert.Throws<ArgumentNullException>(() => e.GetObjectData(default!, default));
        }

        [Test]
        public void DefaultToString()
        {
            const string start = @"Hazelcast.Protocol.RemoteException (IllegalState): message";
            const string end = @" ---> IllegalState
   at className_0.methodName_0(...) in fileName_0:0
   at className_1.methodName_1(...) in fileName_1:1
   at className_2.methodName_2(...) in fileName_2:2
   at className_3.methodName_3(...) in fileName_3:3
   at className_4.methodName_4(...) in fileName_4:4
   --- End of remote stack trace ---";

            try
            {
                ThrowRemoteException(RemoteError.IllegalState);
            }
            catch (RemoteException exception)
            {
                var s = exception.ToString();

                Assert.That(s.ToLf(), Does.StartWith(start.ToLf()));
                Assert.That(s.ToLf(), Does.EndWith(end.ToLf()));

                Console.WriteLine(s);
                //throw;
            }
        }

        [Test]
        public void RetryableToString()
        {
            const string start = @"Hazelcast.Protocol.RemoteException (MemberLeft,Retryable): message";
            const string end = @" ---> MemberLeft
   at className_0.methodName_0(...) in fileName_0:0
   at className_1.methodName_1(...) in fileName_1:1
   at className_2.methodName_2(...) in fileName_2:2
   at className_3.methodName_3(...) in fileName_3:3
   at className_4.methodName_4(...) in fileName_4:4
   --- End of remote stack trace ---";

            try
            {
                ThrowRemoteException(RemoteError.MemberLeft);
            }
            catch (Exception exception)
            {
                var s = exception.ToString();

                Assert.That(s.ToLf(), Does.StartWith(start.ToLf()));
                Assert.That(s.ToLf(), Does.EndWith(end.ToLf()));

                Console.WriteLine(s);
                //throw;
            }
        }

        [Test]
        public void InnerToString()
        {
            const string start = @"Hazelcast.Protocol.RemoteException (MemberLeft,Retryable): message
 ---> Hazelcast.Protocol.RemoteException (MemberLeft,Retryable): message
 ---> MemberLeft
   at className_0.methodName_0(...) in fileName_0:0
   at className_1.methodName_1(...) in fileName_1:1
   at className_2.methodName_2(...) in fileName_2:2
   at className_3.methodName_3(...) in fileName_3:3
   at className_4.methodName_4(...) in fileName_4:4
   --- End of remote stack trace ---
   --- End of inner exception ---
";
            const string end = @" ---> MemberLeft
   at className_0.methodName_0(...) in fileName_0:0
   at className_1.methodName_1(...) in fileName_1:1
   at className_2.methodName_2(...) in fileName_2:2
   at className_3.methodName_3(...) in fileName_3:3
   at className_4.methodName_4(...) in fileName_4:4
   --- End of remote stack trace ---";

            try
            {
                ThrowRemoteExceptionWithInner(RemoteError.MemberLeft);
            }
            catch (Exception exception)
            {
                var s = exception.ToString();

                Assert.That(s.ToLf(), Does.StartWith(start.ToLf()));
                Assert.That(s.ToLf(), Does.EndWith(end.ToLf()));

                Console.WriteLine(s);
                //throw;
            }
        }

        [Test]
        public void Catching()
        {
            var count1 = 0;
            var count2 = 0;

            try
            {
                ThrowRemoteException(RemoteError.MemberLeft);
            }
            catch (RemoteException e) when (e.Error == RemoteError.MemberLeft)
            {
                count1++;
            }
            catch
            {
                count2++;
            }

            Assert.That(count1, Is.EqualTo(1));
            Assert.That(count2, Is.Zero);
        }

        private static void ThrowRemoteException(RemoteError error)
        {
            var stackTraceElements = new List<StackTraceElement>();
            for (var i = 0; i < 5; i++)
                stackTraceElements.Add(new StackTraceElement("className_" + i, "methodName_" + i, "fileName_" + i, i));

            var errorHolder = new ErrorHolder((int) error, "className", "message", stackTraceElements);

            var exception = RemoteExceptions.CreateException(Guid.Empty, new[] { errorHolder });

            throw exception;
        }

        private static void ThrowRemoteExceptionWithInner(RemoteError error)
        {
            var stackTraceElements = new List<StackTraceElement>();
            for (var i = 0; i < 5; i++)
                stackTraceElements.Add(new StackTraceElement("className_" + i, "methodName_" + i, "fileName_" + i, i));

            var errorHolder = new ErrorHolder((int)error, "className", "message", stackTraceElements);

            var exception = RemoteExceptions.CreateException(Guid.Empty, new[] { errorHolder, errorHolder });

            throw exception;
        }
    }
}

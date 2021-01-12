// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

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

using Hazelcast.Exceptions;
using NUnit.Framework;

namespace Hazelcast.Tests.Exceptions
{
    [TestFixture]
    public class StackTraceElementTests
    {
        [Test]
        public void Constructor()
        {
            var e = new StackTraceElement("className", "methodName", "fileName", 42);

            Assert.That(e.ClassName, Is.EqualTo("className"));
            Assert.That(e.MethodName, Is.EqualTo("methodName"));
            Assert.That(e.FileName, Is.EqualTo("fileName"));
            Assert.That(e.LineNumber, Is.EqualTo(42));

        }

        [Test]
        public void ToStringOverride()
        {
            var e = new StackTraceElement("className", "methodName", "fileName", 42);

            Assert.That(e.ToString(), Is.EqualTo("at className.methodName(...) in fileName:42"));
        }

        [Test]
        public void Equality()
        {
            var e = new StackTraceElement("className", "methodName", "fileName", 42);

            #pragma warning disable CS1718 // Comparison made to same variable
            // ReSharper disable EqualExpressionComparison
            // ReSharper disable ConditionIsAlwaysTrueOrFalse

            Assert.That(e.Equals(null), Is.False);
            Assert.That(e.Equals(e), Is.True);
            Assert.That(e.Equals(new StackTraceElement("className", "methodName", "fileName", 42)), Is.True);

            Assert.That(e == e, Is.True);
            Assert.That(e == null, Is.False);
            Assert.That(e == new StackTraceElement("className", "methodName", "fileName", 42), Is.True);
            Assert.That(e != new StackTraceElement("classNamX", "methodName", "fileName", 42), Is.True);

            Assert.That(e.GetHashCode(), Is.Not.Zero);

            // ReSharper restore EqualExpressionComparison
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            #pragma warning restore CS1718 // Comparison made to same variable
        }
    }
}

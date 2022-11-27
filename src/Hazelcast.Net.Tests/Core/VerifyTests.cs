// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Exceptions;
using NUnit.Framework;

namespace Hazelcast.Tests.Core;

[TestFixture]
public class VerifyTests
{
    [Test]
    public void ValidatesConditionMet()
    {
        Verify.Condition<Exception>(true, "");
    }

    [Test]
    public void ValidatesConditionNotMet()
    {
        var e = Assert.Throws<Exception>(() => Verify.Condition<Exception>(false, "meh"));
        Assert.That(e, Is.Not.Null);
        Assert.That(e.Message, Is.EqualTo("meh"));
    }

    [Test]
    public void ValidatesConditionNotMetCtor()
    {
        var e = Assert.Throws<MyExceptionWithCtor>(() => Verify.Condition<MyExceptionWithCtor>(false, "meh"));
        Assert.That(e, Is.Not.Null);
        Assert.That(e, Is.InstanceOf<MyExceptionWithCtor>());
        Assert.That(e.Message, Is.EqualTo("meh"));
    }

    [Test]
    public void ValidatesConditionNotMetNoCtor()
    {
        var e = Assert.Throws<HazelcastException>(() => Verify.Condition<MyExceptionWithoutCtor>(false, "meh"));
        Assert.That(e, Is.Not.Null);
        Assert.That(e, Is.InstanceOf<HazelcastException>());
        Assert.That(e.Message, Does.StartWith("MyExceptionWithoutCtor: meh. In addition"));
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    public class MyExceptionWithCtor : Exception
    {
        public MyExceptionWithCtor(string message) : base(message) { }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    public class MyExceptionWithoutCtor : Exception
    { }
}
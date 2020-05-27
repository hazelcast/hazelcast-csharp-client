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
using Hazelcast.Testing.Conditions;
using NuGet.Versioning;
using NUnit.Framework;

namespace Hazelcast.Tests.Testing
{
    public abstract class TestConditionsTestsBase : ServerVersionTestBase
    {
        // this test executes only if the server version is 0.2
        // and then the call to Require2 is expected to succeed
        //
        [Test]
        [ServerCondition("[0.2]")]
        public void CanDoSomethingOn2()
        {
            Require2();
        }

        // this test executes only if the server version is 0.1
        // and then the call to Require2 is expected to fail
        //
        [Test]
        [ServerCondition("[0.1]")]
        public void CannotDoSomethingOn1()
        {
            Assert.Throws<NotImplementedException>(Require2);
        }

        // this test always executes...
        //
        [Test]
        public void Mixed()
        {
            // the code executes only if the server version is 0.2
            // and then the call to Require2 is expected to succeed
            //
            IfServerVersionIn("[0.2]", Require2);

            IfServerVersionIn("[0.1]", () =>
            {
                // this code executes only if the server version is 0.1
                // and then the call to Require2 is expected to fail
                //
                Assert.Throws<NotImplementedException>(Require2);
            });

            IfServerVersionIn("[0.2]",
                // this code executes only if the server version is 0.2
                // and then the call to Require2 is expected to succeed
                //
                Require2,

                // this code executes only if the server version is not 0.2
                // and then the call to Require2 is expected to fail
                //
                () =>
                {
                    Assert.Throws<NotImplementedException>(Require2);
                });
        }

        // ok on 0.2, throws on 0.1
        private void Require2()
        {
            if (ServerVersion < NuGetVersion.Parse("0.2"))
                throw new NotImplementedException();
        }
    }
}
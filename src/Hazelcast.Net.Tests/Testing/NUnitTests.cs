// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using NUnit.Framework;

namespace Hazelcast.Tests.Testing
{
    [TestFixture]
    public class NUnitTests
    {
        [Test]
        [Explicit("throws")]
        public async Task Timeout()
        {
            // NUnit *does* show the timeout exception here

            await Task.Delay(100).ContinueWith(x =>
            {
                try
                {
                    Throw();
                }
                catch (Exception e)
                {
                    throw new TimeoutException("timeout", e);
                }
            });
        }

        [Test]
        public void Eventually()
        {
            var value = 0;

            Task.Run(async () =>
            {
                await Task.Delay(400);
                value = 1;
            });

            // value should be equal to 1 within 1s, testing every .1s
            // beware! must pass a function, not 'value' else it captures the value!
            Assert.That(() => value, Is.EqualTo(1).After(2_000, 100));

            // value will not be equal to 2 within .1s
            Assert.Throws<AssertionException>(() =>
            {
                Assert.That(() => value, Is.EqualTo(2).After(100));
            });
        }

        private void Throw() => throw new Exception("bang");
    }
}

/*
 * Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Hazelcast.Core;
using NUnit.Framework;
using System.Collections.Generic;

namespace Hazelcast.Client.Test
{
    public class VectorClockTest : SingleMemberBaseTest
    {
        internal static VectorClock _inst;
        internal const string name = "ClientPNCounterTest";

        [SetUp]
        public void Init()
        {
            var initList = new List<KeyValuePair<string, long>>()
            {
                new KeyValuePair<string, long>("node-1", 10),
                new KeyValuePair<string, long>("node-2", 20),
                new KeyValuePair<string, long>("node-3", 30),
                new KeyValuePair<string, long>("node-4", 40),
                new KeyValuePair<string, long>("node-5", 50)
            };

            _inst = new VectorClock(initList);
        }

        [TearDown]
        public static void Destroy()
        {
            _inst = null;
        }

        [Test]
        public void NewerTSDetectedOnNewSet()
        {
            // Arrange
            var newList = new List<KeyValuePair<string, long>>()
            {
                new KeyValuePair<string, long>("node-1", 100),
                new KeyValuePair<string, long>("node-2", 20),
                new KeyValuePair<string, long>("node-3", 30),
                new KeyValuePair<string, long>("node-4", 40),
                new KeyValuePair<string, long>("node-5", 50)
            };

            var newVector = new VectorClock(newList);

            // Act
            var result = _inst.IsAfter(newVector);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void SmallerListOnNewSet()
        {
            // Arrange
            var newList = new List<KeyValuePair<string, long>>()
            {
                new KeyValuePair<string, long>("node-1", 10),
                new KeyValuePair<string, long>("node-2", 20)
            };

            var newVector = new VectorClock(newList);

            // Act
            var result = _inst.IsAfter(newVector);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void SmallerListWithNewerItemOnNewSet()
        {
            // Arrange
            var newList = new List<KeyValuePair<string, long>>()
            {
                new KeyValuePair<string, long>("node-1", 100),
                new KeyValuePair<string, long>("node-2", 20)
            };

            var newVector = new VectorClock(newList);

            // Act
            var result = _inst.IsAfter(newVector);

            // Assert
            Assert.IsFalse(result);
        }
    }
}

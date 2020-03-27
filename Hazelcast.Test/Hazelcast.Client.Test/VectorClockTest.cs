/*
 * Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System;
using Hazelcast.Core;
using NUnit.Framework;
using System.Collections.Generic;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class VectorClockTest
    {
        private static readonly Guid[] _guids = {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};
        private VectorClock _inst;

        [SetUp]
        public void Init()
        {
            var initList = new List<KeyValuePair<Guid, long>>()
            {
                new KeyValuePair<Guid, long>(_guids[0], 10),
                new KeyValuePair<Guid, long>(_guids[1], 20),
                new KeyValuePair<Guid, long>(_guids[2], 30),
                new KeyValuePair<Guid, long>(_guids[3], 40),
                new KeyValuePair<Guid, long>(_guids[4], 50)
            };

            _inst = new VectorClock(initList);
        }

        [TearDown]
        public  void Destroy()
        {
            _inst = null;
        }

        [Test]
        public void NewerTSDetectedOnNewSet()
        {
            // Arrange
            var newList = new List<KeyValuePair<Guid, long>>()
            {
                new KeyValuePair<Guid, long>(_guids[0], 100),
                new KeyValuePair<Guid, long>(_guids[1], 20),
                new KeyValuePair<Guid, long>(_guids[2], 30),
                new KeyValuePair<Guid, long>(_guids[3], 40),
                new KeyValuePair<Guid, long>(_guids[4], 50)
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
            var newList = new List<KeyValuePair<Guid, long>>()
            {
                new KeyValuePair<Guid, long>(_guids[0], 10),
                new KeyValuePair<Guid, long>(_guids[1], 20)
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
            var newList = new List<KeyValuePair<Guid, long>>()
            {
                new KeyValuePair<Guid, long>(_guids[0], 100),
                new KeyValuePair<Guid, long>(_guids[1], 20)
            };

            var newVector = new VectorClock(newList);

            // Act
            var result = _inst.IsAfter(newVector);

            // Assert
            Assert.IsFalse(result);
        }
    }
}

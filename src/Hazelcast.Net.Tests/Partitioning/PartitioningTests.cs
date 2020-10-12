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
using Hazelcast.Exceptions;
using Hazelcast.Partitioning;
using Hazelcast.Partitioning.Strategies;
using NUnit.Framework;

namespace Hazelcast.Tests.Partitioning
{
    [TestFixture]
    public class PartitioningTests
    {
        [Test]
        public void NullStrategy()
        {
            var strategy = new NullPartitioningStrategy();

            var o = new object();
            Assert.That(strategy.GetPartitionKey(o), Is.Null);
        }

        [Test]
        public void StringStrategy()
        {
            var strategy = new StringPartitioningStrategy();

            var o = new object();
            Assert.That(strategy.GetPartitionKey(o), Is.Null);

            Assert.That(strategy.GetPartitionKey("foo"), Is.EqualTo("foo"));
            Assert.That(strategy.GetPartitionKey("@bar"), Is.EqualTo("bar"));
        }

        [Test]
        public void AwareStrategy()
        {
            var strategy = new PartitionAwarePartitioningStragegy();

            var o = new object();
            Assert.That(strategy.GetPartitionKey(o), Is.Null);

            Assert.That(strategy.GetPartitionKey(new AwareClass("foo")), Is.EqualTo("foo"));
            Assert.That(strategy.GetPartitionKey(new AwareClass(1234)), Is.EqualTo(1234));
        }

        [Test]
        public void PartitionTableTest()
        {
            var originClientId = Guid.NewGuid();
            var map = new Dictionary<int, Guid>
            {
                { 1, Guid.NewGuid() },
                { 2, Guid.NewGuid() },
                { 3, Guid.NewGuid() },
            };

            var table = new PartitionTable(originClientId, 0, map);

            Assert.That(table.OriginClientId, Is.EqualTo(originClientId));
            Assert.That(table.Version, Is.EqualTo(0));
            Assert.That(table.Count, Is.EqualTo(3));

            Assert.That(table.MapPartitionId(2), Is.EqualTo(map[2]));
            Assert.That(table.MapPartitionId(3), Is.EqualTo(map[3]));
            Assert.That(table.MapPartitionId(4), Is.EqualTo(Guid.Empty));

            Assert.That(table.IsSupersededBy(originClientId, 0, map), Is.False);
            Assert.That(table.IsSupersededBy(Guid.NewGuid(), 0, map), Is.True);
            Assert.That(table.IsSupersededBy(originClientId, 1, map), Is.True);
            Assert.That(table.IsSupersededBy(originClientId, 1, new Dictionary<int, Guid>()), Is.False);
        }

        [Test]
        public void PartitionerTest()
        {
            var partitioner = new Partitioner();

            Assert.That(partitioner.Count, Is.EqualTo(0));

            Assert.That(partitioner.GetPartitionOwner(1), Is.EqualTo(Guid.Empty));
            Assert.That(partitioner.GetPartitionOwner(-1), Is.EqualTo(Guid.Empty));
            Assert.That(partitioner.GetPartitionOwner(new PartitionHashClass(1).PartitionHash), Is.EqualTo(Guid.Empty));
            Assert.That(partitioner.GetPartitionId(new PartitionHashClass(1).PartitionHash), Is.EqualTo(0));
            Assert.That(partitioner.GetPartitionId(new PartitionHashClass(int.MinValue).PartitionHash), Is.EqualTo(0));
            Assert.That(partitioner.GetRandomPartitionId(), Is.EqualTo(0));

            partitioner.NotifyPartitionsCount(4);

            Assert.That(partitioner.GetPartitionOwner(1), Is.EqualTo(Guid.Empty));
            Assert.That(partitioner.GetPartitionOwner(-1), Is.EqualTo(Guid.Empty));
            Assert.That(partitioner.GetPartitionOwner(new PartitionHashClass(1).PartitionHash), Is.EqualTo(Guid.Empty));
            Assert.That(partitioner.GetPartitionId(new PartitionHashClass(1).PartitionHash), Is.EqualTo(1));
            Assert.That(partitioner.GetPartitionId(new PartitionHashClass(5).PartitionHash), Is.EqualTo(1));
            Assert.That(partitioner.GetPartitionId(new PartitionHashClass(int.MinValue).PartitionHash), Is.EqualTo(0));
            var random = partitioner.GetRandomPartitionId();
            Assert.That(random, Is.GreaterThanOrEqualTo(0));
            Assert.That(random, Is.LessThan(4));

            var originClientId = Guid.NewGuid();
            var map = new Dictionary<int, Guid>
            {
                { 1, Guid.NewGuid() },
                { 2, Guid.NewGuid() },
                { 3, Guid.NewGuid() },
            };

            partitioner.NotifyPartitionView(originClientId, 0, map);

            // TODO: is this the right exception?
            Assert.Throws<ConnectionException>(() => partitioner.NotifyPartitionsCount(7));

            Assert.That(partitioner.GetPartitionOwner(1), Is.EqualTo(map[1]));
            Assert.That(partitioner.GetPartitionOwner(-1), Is.EqualTo(Guid.Empty));
            Assert.That(partitioner.GetPartitionOwner(new PartitionHashClass(1).PartitionHash), Is.EqualTo(map[1]));
            Assert.That(partitioner.GetPartitionId(new PartitionHashClass(1).PartitionHash), Is.EqualTo(1));
            Assert.That(partitioner.GetPartitionId(new PartitionHashClass(4).PartitionHash), Is.EqualTo(1));
            Assert.That(partitioner.GetPartitionId(new PartitionHashClass(int.MinValue).PartitionHash), Is.EqualTo(0));
            random = partitioner.GetRandomPartitionId();
            Assert.That(random, Is.GreaterThanOrEqualTo(0));
            Assert.That(random, Is.LessThan(3));

            Assert.That(partitioner.GetPartitionHashOwner(1), Is.EqualTo(map[1]));

            var newMap = new Dictionary<int, Guid>
            {
                { 1, Guid.NewGuid() },
                { 2, Guid.NewGuid() },
                { 3, Guid.NewGuid() },
            };

            partitioner.NotifyPartitionView(originClientId, 0, newMap);
            Assert.That(partitioner.GetPartitionOwner(1), Is.EqualTo(map[1]));

            partitioner.NotifyPartitionView(originClientId, 1, newMap);
            Assert.That(partitioner.GetPartitionOwner(1), Is.EqualTo(newMap[1]));

            newMap = new Dictionary<int, Guid>
            {
                { 1, Guid.NewGuid() },
                { 2, Guid.NewGuid() },
                { 3, Guid.NewGuid() },
            };

            var newOwnerId = Guid.NewGuid();

            partitioner.NotifyPartitionView(newOwnerId, 1, newMap);
            Assert.That(partitioner.GetPartitionOwner(1), Is.EqualTo(newMap[1]));

            partitioner.NotifyPartitionView(newOwnerId, 1, new Dictionary<int, Guid>());
            Assert.That(partitioner.GetPartitionOwner(1), Is.EqualTo(newMap[1]));
        }

        [Test]
        public void ArgumentExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => new Partitioner().NotifyPartitionView(Guid.NewGuid(), 0, null));
        }

        private class AwareClass : IPartitionAware
        {
            private readonly object _partitionKey;

            public AwareClass(object partitionKey)
            {
                _partitionKey = partitionKey;
            }

            public object GetPartitionKey() => _partitionKey;
        }

        private class PartitionHashClass
        {
            public PartitionHashClass(int partitionHash)
            {
                PartitionHash = partitionHash;
            }

            public int PartitionHash { get; }
        }
    }
}

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
using System.Linq;
using Hazelcast.Core;

namespace Hazelcast.Partitioning
{
    public class Partitioner
    {
        private readonly object _partitionsLock = new object();
        private PartitionTable _partitions;

        /// <summary>
        /// Gets the number of partitions.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the member owning a partition.
        /// </summary>
        /// <param name="partitionId">The identifier of the partition.</param>
        /// <returns>The unique identifier of the member owning the partition, or an empty Guid if the partition has no owner.</returns>
        public Guid GetPartitionOwner(int partitionId)
        {
            // once _partitions has a value, it won't be null again
            // and we can use whatever value it has, and references are atomic
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_partitions == null) return default;
            return _partitions.MapPartitionId(partitionId);
        }

        /// <summary>
        /// Gets the unique identifier of the member owning the partition of a key <see cref="IHavePartitionHash"/> instance.
        /// </summary>
        /// <param name="key">The key <see cref="IHavePartitionHash"/> instance.</param>
        /// <returns>The unique identifier of the member owning the partition, or an empty Guid if the partition has no owner.</returns>
        public Guid GetPartitionOwner(IHavePartitionHash key)
        {
            return GetPartitionOwner(GetPartitionId(key));
        }

        /// <summary>
        /// Gets the partition identifier of a key <see cref="IHavePartitionHash"/> instance.
        /// </summary>
        /// <param name="key">The key <see cref="IHavePartitionHash"/> instance.</param>
        /// <returns>The partition identifier for the specified key <see cref="IHavePartitionHash"/> instance.</returns>
        public int GetPartitionId(IHavePartitionHash key)
        {
            // we can use whatever value count is, and it is atomic

            var hash = key.PartitionHash;
            return hash == int.MinValue // cannot Abs(int.MinValue)
                ? 0
                : Math.Abs(hash) % Count;
        }

        /// <summary>
        /// Notifies the partitioner of a new partition view.
        /// </summary>
        /// <param name="originClientId">The unique identifier of the client originating the view.</param>
        /// <param name="version">The version of the view.</param>
        /// <param name="partitionsMap">The partitions map.</param>
        public void NotifyPartitionView(Guid originClientId, int version, Dictionary<int, Guid> partitionsMap)
        {
            HzConsole.WriteLine(this, $"Received partition table v{version}");
            HzConsole.WriteLine(this, 1, $"Partitions v{version}:\n" + string.Join("\n", partitionsMap.Select(kvp => $"\t{kvp.Key}:{kvp.Value}")));

            lock (_partitionsLock) // one at a time please
            {
                if (_partitions == null || _partitions.IsSupersededBy(originClientId, version, partitionsMap))
                {
                    _partitions = new PartitionTable(originClientId, version, partitionsMap);
                    Count = _partitions.Count;
                }
            }
        }

        /// <summary>
        /// Notifies the partitioner of the initial partitions count.
        /// </summary>
        /// <param name="count">The partitions count.</param>
        public void NotifyInitialCount(int count)
        {
            lock (_partitionsLock)
            {
                if (_partitions != null) return;
                Count = count;
            }
        }
    }
}

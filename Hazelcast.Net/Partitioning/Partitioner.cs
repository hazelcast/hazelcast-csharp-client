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
using Hazelcast.Logging;
using Hazelcast.Serialization;

namespace Hazelcast.Partitioning
{
    public class Partitioner
    {
        private readonly ISerializationService _serializationService;
        private readonly bool _isSmartRouting;
        private readonly object _partitionsLock = new object();
        private PartitionTable _partitions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Partitioner"/> class.
        /// </summary>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="isSmartRouting">Whether the cluster operates with smart routing.</param>
        public Partitioner(ISerializationService serializationService, bool isSmartRouting)
        {
            // FIXME null check
            _serializationService = serializationService;// ?? throw new ArgumentNullException(nameof(serializationService));
            _isSmartRouting = isSmartRouting;
        }

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
        /// Gets the unique identifier of the member owning the partition corresponding to a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The unique identifier of the member owning the partition, or an empty Guid if the partition has no owner.</returns>
        public Guid GetPartitionOwner(IData key)
        {
            return GetPartitionOwner(GetPartitionId(key));
        }

        /// <summary>
        /// Gets the partition identifier for an <see cref="IData"/> instance.
        /// </summary>
        /// <param name="data">The <see cref="IData"/> instance.</param>
        /// <returns>The partition identifier for the specified <see cref="IData"/> instance.</returns>
        public int GetPartitionId(IData data)
        {
            // we can use whatever value _count has, and it is atomic

            var hash = data.PartitionHash;
            return hash == int.MinValue
                ? 0
                : Math.Abs(hash) % Count;
        }

        /// <summary>
        /// Gets the partition identifier for an object.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns>The partition identifier for the specified object.</returns>
        public int GetPartitionId(object o) // FIXME check if we can move this out and not require ISerializationService here
        {
            return GetPartitionId(_serializationService.ToData(o));
        }

        /// <summary>
        /// Notifies the partitioner of a new partition view.
        /// </summary>
        /// <param name="originClientId">The unique identifier of the client originating the view.</param>
        /// <param name="version">The version of the view.</param>
        /// <param name="partitionsMap">The partitions map.</param>
        public void NotifyPartitionView(Guid originClientId, int version, Dictionary<int, Guid> partitionsMap)
        {
            XConsole.WriteLine(this, $"PARTITIONS\n\tv{version}\n" + string.Join("\n", partitionsMap.Select(kvp => $"\t{kvp.Key}:{kvp.Value}")));

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
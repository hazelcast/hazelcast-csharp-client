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

namespace Hazelcast.Partitioning
{
    /// <summary>
    /// Represents a cluster partition table.
    /// </summary>
    internal class PartitionTable
    {
        private readonly Dictionary<int, Guid> _partitionsMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionTable"/> class.
        /// </summary>
        /// <param name="originClientId">The unique identifier of the client that originated the table.</param>
        /// <param name="version">The version of the partition table.</param>
        /// <param name="partitionsMap">A map of partition identifiers to cluster member identifiers.</param>
        public PartitionTable(Guid originClientId, int version, Dictionary<int, Guid> partitionsMap)
        {
            OriginClientId = originClientId;
            Version = version;
            _partitionsMap = partitionsMap;
        }

        /// <summary>
        /// Gets the identifier of the client that originated the table.
        /// </summary>
        public Guid OriginClientId { get; }

        /// <summary>
        /// Gets the version of the partition table.
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// Gets the number of partitions in the table.
        /// </summary>
        public int Count => _partitionsMap.Count;

        /// <summary>
        /// Maps a partition to the corresponding cluster member.
        /// </summary>
        /// <param name="partitionId">The identifier of the partition.</param>
        /// <returns>The identifier of the cluster member corresponding to the partition, or an
        /// empty Guid if no cluster member corresponds to the partition.</returns>
        public Guid MapPartitionId(int partitionId)
            => _partitionsMap.TryGetValue(partitionId, out var memberId) ? memberId : default;

        /// <summary>
        /// Determines whether this partition table is superseded by a new table.
        /// </summary>
        /// <param name="originClientId">The identifier of the client that originated the new partition table.</param>
        /// <param name="version">The version of the new partition table.</param>
        /// <param name="partitionsMap">A new map of partition identifiers to cluster member identifiers.</param>
        /// <returns></returns>
        public bool IsSupersededBy(Guid originClientId, int version, Dictionary<int, Guid> partitionsMap)
            => partitionsMap.Count > 0 && (originClientId != OriginClientId || version > Version);
    }
}
﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    internal class Partitioner
    {
        private readonly object _partitionsLock = new object();
        private PartitionTable _partitions;

        public Partitioner()
        {
            HConsole.Configure(options => options.Set(this, x=> x.SetPrefix("PART")));
        }

        /// <summary>
        /// Gets the number of partitions.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the member owning a partition.
        /// </summary>
        /// <param name="partitionId">The identifier of the partition.</param>
        /// <returns>The unique identifier of the member owning the partition with the specified <paramref name="partitionId"/>,
        /// or an empty <see cref="Guid"/> if that partition has no owner.</returns>
        public Guid GetPartitionOwner(int partitionId)
        {
            if (partitionId < 0) return default;

            // once _partitions has a value, it won't be null again
            // and we can use whatever value it has, and references are atomic
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_partitions == null) return default;
            // ReSharper disable once InconsistentlySynchronizedField
            return _partitions.MapPartitionId(partitionId);
        }

        /// <summary>
        /// Gets the unique identifier of the member owning the partition corresponding to a partition hash.
        /// </summary>
        /// <param name="partitionHash">The partition hash.</param>
        /// <returns>The unique identifier of the owner owning the partition corresponding to the specified
        /// <paramref name="partitionHash"/>, or an empty <see cref="Guid"/> if that partition has no owner.</returns>
        public Guid GetPartitionHashOwner(int partitionHash)
        {
            return GetPartitionOwner(GetPartitionId(partitionHash));
        }

        /// <summary>
        /// Gets the identifier of the partition corresponding to a partition hash.
        /// </summary>
        /// <param name="partitionHash">The partition hash.</param>
        /// <returns>The identifier of the partition corresponding to the specified <paramref name="partitionHash"/>.</returns>
        public int GetPartitionId(int partitionHash)
        {
            if (Count == 0) return 0;

            // we can use whatever value count is, and it is atomic
            return partitionHash == int.MinValue // cannot Abs(int.MinValue)
                ? 0
                : Math.Abs(partitionHash) % Count;
        }

        /// <summary>
        /// Gets a random partition identifier.
        /// </summary>
        /// <returns>A random partition identifier.</returns>
        public int GetRandomPartitionId()
        {
            return RandomProvider.Random.Next(Count);
        }

        /// <summary>
        /// Notifies the partitioner of a new partition view.
        /// </summary>
        /// <param name="originClientId">The unique identifier of the client originating the view.</param>
        /// <param name="version">The version of the view.</param>
        /// <param name="partitionsMap">The partitions map.</param>
        /// <returns><c>true</c> if the new view superseded and replaced the existing one; otherwise <c>false</c>.</returns>
        public bool NotifyPartitionView(Guid originClientId, int version, Dictionary<int, Guid> partitionsMap)
        {
            if (partitionsMap == null) throw new ArgumentNullException(nameof(partitionsMap));

            HConsole.WriteLine(this, $"Received partition table v{version}");
            HConsole.WriteLine(this, 10, $"Partitions v{version}:\n" +
                                        string.Join("\n", partitionsMap.Select(kvp => $"\t{kvp.Key,-16}{kvp.Value}")));

            lock (_partitionsLock) // one at a time please
            {
                if (_partitions != null && !_partitions.IsSupersededBy(originClientId, version, partitionsMap))
                    return false;

                _partitions = new PartitionTable(originClientId, version, partitionsMap);
                Count = _partitions.Count;
                return true;
            }
        }

        /// <summary>
        /// Sets or verifies the partition count.
        /// </summary>
        /// <param name="count">The partition count.</param>
        /// <remarks>
        /// <para>The specified <paramref name="count"/> must be either the first one,
        /// i.e. the partitioner does not know of partitions yet, or it must be equal
        /// to the known partitions count. Otherwise, a <see cref="InvalidOperationException"/>
        /// is thrown.</para>
        /// </remarks>
        public void SetOrVerifyPartitionCount(int count)
        {
            lock (_partitionsLock)
            {
                if (_partitions == null)
                {
                    Count = count;
                }
                else if (Count != count)
                {
                    throw new InvalidOperationException($"Received count value {count} but expected {Count}.");
                }
            }
        }
    }
}

// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Hazelcast.Core;
using Hazelcast.Serialization;
namespace Hazelcast.Benchmarks
{
    /*
BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.7171)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.100
  [Host] : .NET 10.0.0 (10.0.25.52411), X64 RyuJIT AVX2

Job=InProcess  Toolchain=InProcessEmitToolchain

| Method            | EntryCount | DataSize | Mean      | Error    | StdDev    | Gen0   | Allocated |
|------------------ |----------- |--------- |----------:|---------:|----------:|-------:|----------:|
| NotSizedListUsage | 100        | 128      |  93.57 ns | 2.281 ns |  6.724 ns | 0.0401 |     336 B |
| SizedListUsage    | 100        | 128      | 134.60 ns | 3.728 ns | 10.992 ns | 0.0572 |     480 B |
| NotSizedListUsage | 10000      | 128      |  90.58 ns | 2.145 ns |  5.945 ns | 0.0401 |     336 B |
| SizedListUsage    | 10000      | 128      | 187.54 ns | 3.697 ns |  6.177 ns | 0.0572 |     480 B |
| NotSizedListUsage | 1000000    | 128      | 146.65 ns | 5.371 ns | 15.836 ns | 0.0401 |     336 B |
| SizedListUsage    | 1000000    | 128      | 190.83 ns | 6.574 ns | 19.281 ns | 0.0572 |     480 B |
     */


    /// <summary>
    /// This benchmark compares the usage of sized lists versus non-sized lists
    /// which might be used in grouping operations such as PutAll.
    /// </summary>
    public class SizedListUsageVsNotSized
    {
        public int PartitionCount { get; } = 271;

        [Params(100, 10000, 1_000_000)]
        public int EntryCount;

        [Params(128)]
        public int DataSize;

        private Dictionary<IData, IData> RawEntries { get; set; } = new Dictionary<IData, IData>();


        [GlobalSetup]
        public void GenerateHeapData()
        {
            for (int i = 0; i < EntryCount; i++)
            {
                var partitionId = i % PartitionCount;
                var data = new HeapData(GetData(DataSize, partitionId));
                var key = new HeapData(GetData(16, partitionId)); 
                RawEntries[key] = data;
            }
        }
        private byte[] GetData(int i, int partition)
        {
            var data = new byte[i];
            data.WriteInt(0, partition, Endianness.BigEndian);
            return data;
        }


        [Benchmark]
        public void NotSizedListUsage()
        {
            var groupedEntries = new Dictionary<int, List<KeyValuePair<IData, IData>>>();

            foreach (var rawEntry in RawEntries)
            {
                var partitionId = rawEntry.Key.PartitionHash;

                if (groupedEntries.TryGetValue(partitionId, out var list))
                {
                    list.Add(rawEntry);
                }
                else
                {
                    var newList = new List<KeyValuePair<IData, IData>>();
                    newList.Add(rawEntry);
                    groupedEntries[partitionId] = newList;
                }
            }
        }

        [Benchmark]
        public void SizedListUsage()
        {
            var groupSizes = new Dictionary<int, int>();

            foreach (var entry in RawEntries)
            {
                groupSizes[entry.Key.PartitionHash] = groupSizes.GetValueOrDefault(entry.Key.PartitionHash, 0) + 1;
            }

            var groupedEntries = new Dictionary<int, List<KeyValuePair<IData, IData>>>();

            foreach (var rawEntry in RawEntries)
            {
                var partitionId = rawEntry.Key.PartitionHash;

                if (groupedEntries.TryGetValue(partitionId, out var list))
                {
                    list.Add(rawEntry);
                }
                else
                {
                    // only change from the other method is setting a size to the list
                    var newList = new List<KeyValuePair<IData, IData>>(groupSizes[partitionId]);
                    newList.Add(rawEntry);
                    groupedEntries[partitionId] = newList;
                }
            }
        }

    }
}

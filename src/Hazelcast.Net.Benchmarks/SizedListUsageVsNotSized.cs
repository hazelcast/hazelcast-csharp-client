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

Job=InProcess  Toolchain=InProcessNoEmitToolchain

| Method            | EntryCount | DataSize | Mean      | Error    | StdDev    | Median    | Gen0   | Allocated |
|------------------ |----------- |--------- |----------:|---------:|----------:|----------:|-------:|----------:|
| NotSizedListUsage | 100        | 128      |  96.68 ns | 2.293 ns |  6.615 ns |  96.55 ns | 0.0401 |     336 B |
| SizedListUsage    | 100        | 128      | 133.54 ns | 3.828 ns | 11.287 ns | 132.74 ns | 0.0572 |     480 B |
| NotSizedListUsage | 10000      | 128      |  93.20 ns | 3.860 ns | 11.382 ns |  88.88 ns | 0.0401 |     336 B |
| SizedListUsage    | 10000      | 128      | 139.72 ns | 3.347 ns |  9.711 ns | 138.98 ns | 0.0572 |     480 B |
| NotSizedListUsage | 1000000    | 128      |  89.39 ns | 2.517 ns |  7.341 ns |  87.52 ns | 0.0401 |     336 B |
| SizedListUsage    | 1000000    | 128      | 134.41 ns | 3.477 ns | 10.198 ns | 132.89 ns | 0.0572 |     480 B |
     */
    
    
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
                var data = new HeapData(GetData(DataSize, i + 1)); // 1 KB
                var key = new HeapData(GetData(16, i + 1)); // 16 bytes
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
            var groupSizes = new Dictionary<int,int>();

            foreach (var entry in RawEntries)
            {
                groupSizes[entry.Key.PartitionHash] = groupSizes.GetValueOrDefault(entry.Key.PartitionHash,0) + 1;
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

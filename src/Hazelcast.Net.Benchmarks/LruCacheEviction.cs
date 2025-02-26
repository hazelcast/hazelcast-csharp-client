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
using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Hazelcast.Benchmarks;

/*
|           Method |   Size |          Mean |       Error |      StdDev |     Gen0 |     Gen1 |     Gen2 |  Allocated |
|----------------- |------- |--------------:|------------:|------------:|---------:|---------:|---------:|-----------:|
| EvictWithMinHeap |    100 |      2.403 us |   0.0055 us |   0.0046 us |   0.1335 |        - |        - |    1.09 KB |
|    EvictWithSort |    100 |      4.318 us |   0.0207 us |   0.0194 us |   0.2365 |        - |        - |    1.95 KB |
| EvictWithMinHeap |   1000 |     36.793 us |   0.1643 us |   0.1537 us |   0.5493 |        - |        - |    4.88 KB |
|    EvictWithSort |   1000 |     85.071 us |   0.5419 us |   0.5069 us |   2.0752 |        - |        - |   17.06 KB |
| EvictWithMinHeap |  10000 |    517.023 us |   2.6572 us |   2.4855 us |   3.9063 |        - |        - |   36.67 KB |
|    EvictWithSort |  10000 |  1,085.996 us |  11.2326 us |  10.5069 us |  19.5313 |   1.9531 |        - |  168.24 KB |
| EvictWithMinHeap | 100000 |  6,507.747 us |  55.3840 us |  51.8063 us |  54.6875 |  39.0625 |  39.0625 |  477.66 KB |
|    EvictWithSort | 100000 | 14,858.008 us | 168.6865 us | 149.5362 us | 484.3750 | 484.3750 | 484.3750 | 1680.26 KB |
*/


public class LruCacheEviction
{
    [Params(100, 1000, 10_000, 100_000)] public int Size { get; set; }

    public int NumRemove { get; set; }

    private List<int> _numbers;

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new Random();
        NumRemove = Size / 10;
        _numbers = Enumerable.Repeat(0, Size + NumRemove).Select(p => rnd.Next(1_000_000)).ToList();
    }


    [Benchmark]
    public void EvictWithMinHeap()
    {
        var q = new PriorityQueue<int, int>();

        foreach (var val in _numbers)
        {
            q.Enqueue(val, val);
        }

        for (int i = 0; i < NumRemove; i++)
        {
            q.Dequeue();
        }

        var cutOff = q.Dequeue();

        var _ = _numbers.Where(p => p >= cutOff).OrderBy(p => p).ToList();
    }

    [Benchmark]
    public void EvictWithSort()
    {
        var _=  _numbers.OrderBy(p => p).Skip(NumRemove).ToList();
    }
}

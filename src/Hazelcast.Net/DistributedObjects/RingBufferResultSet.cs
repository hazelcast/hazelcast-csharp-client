// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;

namespace Hazelcast.DistributedObjects;

internal class RingBufferResultSet<T> : ReadOnlyLazyList<T>, IRingBufferResultSet<T>
{
    private readonly long[] _sequences;
    private int _readCount;

    public RingBufferResultSet(SerializationService serializationService,
        long[] sequences,
        int readCount,
        long nextSequence) : base(serializationService)
    {
        _sequences = sequences;
        _readCount = readCount;
        NextSequence = nextSequence;
    }

    public long GetSequence(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException($"Given index {index} is out of range.");
        
        return _sequences.Length > index ? _sequences[index] : -1;
    }

    public long NextSequence { get; }
}

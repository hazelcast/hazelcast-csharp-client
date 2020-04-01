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

namespace Hazelcast.Client.Protocol.Codec
{
    internal enum AtomicLongMessageType
    {
        AtomicLongApply = 0x0a01,
        AtomicLongAlter = 0x0a02,
        AtomicLongAlterAndGet = 0x0a03,
        AtomicLongGetAndAlter = 0x0a04,
        AtomicLongAddAndGet = 0x0a05,
        AtomicLongCompareAndSet = 0x0a06,
        AtomicLongDecrementAndGet = 0x0a07,
        AtomicLongGet = 0x0a08,
        AtomicLongGetAndAdd = 0x0a09,
        AtomicLongGetAndSet = 0x0a0a,
        AtomicLongIncrementAndGet = 0x0a0b,
        AtomicLongGetAndIncrement = 0x0a0c,
        AtomicLongSet = 0x0a0d
    }
}
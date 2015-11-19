// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
    internal enum RingbufferMessageType
    {
        RingbufferSize = 0x1901,
        RingbufferTailSequence = 0x1902,
        RingbufferHeadSequence = 0x1903,
        RingbufferCapacity = 0x1904,
        RingbufferRemainingCapacity = 0x1905,
        RingbufferAdd = 0x1906,
        RingbufferReadOne = 0x1908,
        RingbufferAddAll = 0x1909,
        RingbufferReadMany = 0x190a
    }
}
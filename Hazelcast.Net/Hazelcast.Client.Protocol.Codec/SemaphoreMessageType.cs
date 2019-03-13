// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
    internal enum SemaphoreMessageType
    {
        SemaphoreInit = 0x0d01,
        SemaphoreAcquire = 0x0d02,
        SemaphoreAvailablePermits = 0x0d03,
        SemaphoreDrainPermits = 0x0d04,
        SemaphoreReducePermits = 0x0d05,
        SemaphoreRelease = 0x0d06,
        SemaphoreTryAcquire = 0x0d07,
        SemaphoreIncreasePermits = 0x0d08
    }
}
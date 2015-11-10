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
    internal enum LockMessageType
    {
        LockIsLocked = 0x0701,
        LockIsLockedByCurrentThread = 0x0702,
        LockGetLockCount = 0x0703,
        LockGetRemainingLeaseTime = 0x0704,
        LockLock = 0x0705,
        LockUnlock = 0x0706,
        LockForceUnlock = 0x0707,
        LockTryLock = 0x0708
    }
}
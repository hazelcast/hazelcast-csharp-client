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
    internal enum MultiMapMessageType
    {
        MultiMapPut = 0x0201,
        MultiMapGet = 0x0202,
        MultiMapRemove = 0x0203,
        MultiMapKeySet = 0x0204,
        MultiMapValues = 0x0205,
        MultiMapEntrySet = 0x0206,
        MultiMapContainsKey = 0x0207,
        MultiMapContainsValue = 0x0208,
        MultiMapContainsEntry = 0x0209,
        MultiMapSize = 0x020a,
        MultiMapClear = 0x020b,
        MultiMapValueCount = 0x020c,
        MultiMapAddEntryListenerToKey = 0x020d,
        MultiMapAddEntryListener = 0x020e,
        MultiMapRemoveEntryListener = 0x020f,
        MultiMapLock = 0x0210,
        MultiMapTryLock = 0x0211,
        MultiMapIsLocked = 0x0212,
        MultiMapUnlock = 0x0213,
        MultiMapForceUnlock = 0x0214,
        MultiMapRemoveEntry = 0x0215
    }
}
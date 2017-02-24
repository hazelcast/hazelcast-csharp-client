// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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
    internal enum MapMessageType
    {
        MapPut = 0x0101,
        MapGet = 0x0102,
        MapRemove = 0x0103,
        MapReplace = 0x0104,
        MapReplaceIfSame = 0x0105,
        MapContainsKey = 0x0109,
        MapContainsValue = 0x010a,
        MapRemoveIfSame = 0x010b,
        MapDelete = 0x010c,
        MapFlush = 0x010d,
        MapTryRemove = 0x010e,
        MapTryPut = 0x010f,
        MapPutTransient = 0x0110,
        MapPutIfAbsent = 0x0111,
        MapSet = 0x0112,
        MapLock = 0x0113,
        MapTryLock = 0x0114,
        MapIsLocked = 0x0115,
        MapUnlock = 0x0116,
        MapAddInterceptor = 0x0117,
        MapRemoveInterceptor = 0x0118,
        MapAddEntryListenerToKeyWithPredicate = 0x0119,
        MapAddEntryListenerWithPredicate = 0x011a,
        MapAddEntryListenerToKey = 0x011b,
        MapAddEntryListener = 0x011c,
        MapAddNearCacheEntryListener = 0x011d,
        MapRemoveEntryListener = 0x011e,
        MapAddPartitionLostListener = 0x011f,
        MapRemovePartitionLostListener = 0x0120,
        MapGetEntryView = 0x0121,
        MapEvict = 0x0122,
        MapEvictAll = 0x0123,
        MapLoadAll = 0x0124,
        MapLoadGivenKeys = 0x0125,
        MapKeySet = 0x0126,
        MapGetAll = 0x0127,
        MapValues = 0x0128,
        MapEntrySet = 0x0129,
        MapKeySetWithPredicate = 0x012a,
        MapValuesWithPredicate = 0x012b,
        MapEntriesWithPredicate = 0x012c,
        MapAddIndex = 0x012d,
        MapSize = 0x012e,
        MapIsEmpty = 0x012f,
        MapPutAll = 0x0130,
        MapClear = 0x0131,
        MapExecuteOnKey = 0x0132,
        MapSubmitToKey = 0x0133,
        MapExecuteOnAllKeys = 0x0134,
        MapExecuteWithPredicate = 0x0135,
        MapExecuteOnKeys = 0x0136,
        MapForceUnlock = 0x0137,
        MapKeySetWithPagingPredicate = 0x0138,
        MapValuesWithPagingPredicate = 0x0139,
        MapEntriesWithPagingPredicate = 0x013a,
        MapClearNearCache = 0x013b,
        MapRemoveAll = 0x0144

    }
}
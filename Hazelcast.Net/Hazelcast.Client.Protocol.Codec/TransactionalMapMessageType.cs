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
    internal enum TransactionalMapMessageType
    {
        TransactionalMapContainsKey = 0x1001,
        TransactionalMapGet = 0x1002,
        TransactionalMapGetForUpdate = 0x1003,
        TransactionalMapSize = 0x1004,
        TransactionalMapIsEmpty = 0x1005,
        TransactionalMapPut = 0x1006,
        TransactionalMapSet = 0x1007,
        TransactionalMapPutIfAbsent = 0x1008,
        TransactionalMapReplace = 0x1009,
        TransactionalMapReplaceIfSame = 0x100a,
        TransactionalMapRemove = 0x100b,
        TransactionalMapDelete = 0x100c,
        TransactionalMapRemoveIfSame = 0x100d,
        TransactionalMapKeySet = 0x100e,
        TransactionalMapKeySetWithPredicate = 0x100f,
        TransactionalMapValues = 0x1010,
        TransactionalMapValuesWithPredicate = 0x1011,
        TransactionalMapContainsValue = 0x1012
    }
}
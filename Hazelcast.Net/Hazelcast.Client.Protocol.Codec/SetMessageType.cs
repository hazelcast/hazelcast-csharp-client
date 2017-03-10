// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
    internal enum SetMessageType
    {
        SetSize = 0x0601,
        SetContains = 0x0602,
        SetContainsAll = 0x0603,
        SetAdd = 0x0604,
        SetRemove = 0x0605,
        SetAddAll = 0x0606,
        SetCompareAndRemoveAll = 0x0607,
        SetCompareAndRetainAll = 0x0608,
        SetClear = 0x0609,
        SetGetAll = 0x060a,
        SetAddListener = 0x060b,
        SetRemoveListener = 0x060c,
        SetIsEmpty = 0x060d
    }
}
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
    internal enum ClientMessageType
    {
        ClientAuthentication = 0x0002,
        ClientAuthenticationCustom = 0x0003,
        ClientAddMembershipListener = 0x0004,
        ClientCreateProxy = 0x0005,
        ClientDestroyProxy = 0x0006,
        ClientGetPartitions = 0x0008,
        ClientRemoveAllListeners = 0x0009,
        ClientAddPartitionLostListener = 0x000a,
        ClientRemovePartitionLostListener = 0x000b,
        ClientGetDistributedObjects = 0x000c,
        ClientAddDistributedObjectListener = 0x000d,
        ClientRemoveDistributedObjectListener = 0x000e,
        ClientPing = 0x000f,
        ClientStatistics = 0x0010,
        ClientDeployClasses = 0x0011,
        ClientAddPartitionListener = 0x0012,
        ClientCreateProxies = 0x0013
    }
}
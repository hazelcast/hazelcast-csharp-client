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

using Hazelcast.Client.Protocol;
using Hazelcast.Core;
using Hazelcast.IO;

namespace Hazelcast.Client.Spi
{
    /// <summary>
    /// The invocation service.
    /// </summary>
    interface IClientInvocationService
    {
        IFuture<ClientMessage> InvokeOnKeyOwner(ClientMessage request, object key);
        IFuture<ClientMessage> InvokeOnMember(ClientMessage request, IMember member);
        IFuture<ClientMessage> InvokeOnPartition(ClientMessage request, int partitionId);
        IFuture<ClientMessage> InvokeOnRandomTarget(ClientMessage request);
        IFuture<ClientMessage> InvokeOnTarget(ClientMessage request, Address target);
        void Shutdown();
    }
}
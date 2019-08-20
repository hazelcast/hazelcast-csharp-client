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
        IFuture<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key);
        IFuture<IClientMessage> InvokeOnMember(IClientMessage request, IMember member);
        IFuture<IClientMessage> InvokeOnPartition(IClientMessage request, int partitionId);
        IFuture<IClientMessage> InvokeOnRandomTarget(IClientMessage request);
        IFuture<IClientMessage> InvokeOnTarget(IClientMessage request, Address target);
        void Shutdown();
    }
}
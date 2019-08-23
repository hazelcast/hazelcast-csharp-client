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

using System.Threading.Tasks;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    /// <summary>
    /// The invocation service.
    /// </summary>
    interface IClientInvocationService
    {
        void InvokeOnKeyOwner(IClientMessage request, IFuture<IClientMessage> future, object key);
        void InvokeOnMember(IClientMessage request, IFuture<IClientMessage> future, IMember member);
        void InvokeOnPartition(IClientMessage request, IFuture<IClientMessage> future, int partitionId);
        void InvokeOnRandomTarget(IClientMessage request, IFuture<IClientMessage> future);
        void InvokeOnTarget(IClientMessage request, IFuture<IClientMessage> future, Address target);
        void InvokeOnConnection(IClientMessage request, IFuture<IClientMessage> future, ClientConnection connection);
        void InvokeListenerOnConnection(IClientMessage request, IFuture<IClientMessage> future,
            DistributedEventHandler eventHandler, ClientConnection connection);

        void Shutdown();
    }

    static class SyncClientInvocationService
    {
        public static IClientMessage InvokeOnKeyOwner(this IClientInvocationService service, IClientMessage request, object key)
        {
            var future = new SyncFuture<IClientMessage>();
            service.InvokeOnKeyOwner(request, future, key);
            return future.WaitAndGet();
        }

        public static IClientMessage InvokeOnMember(this IClientInvocationService service, IClientMessage request, IMember member)
        {
            var future = new SyncFuture<IClientMessage>();
            service.InvokeOnMember(request, future, member);
            return future.WaitAndGet();
        }

        public static IClientMessage InvokeOnPartition(this IClientInvocationService service, IClientMessage request, int partitionId)
        {
            var future = new SyncFuture<IClientMessage>();
            service.InvokeOnPartition(request, future, partitionId);
            return future.WaitAndGet();
        }

        public static IClientMessage InvokeOnRandomTarget(this IClientInvocationService service, IClientMessage request)
        {
            var future = new SyncFuture<IClientMessage>();
            service.InvokeOnRandomTarget(request, future);
            return future.WaitAndGet();
        }

        public static IClientMessage InvokeOnTarget(this IClientInvocationService service, IClientMessage request, Address target)
        {
            var future = new SyncFuture<IClientMessage>();
            service.InvokeOnTarget(request, future, target);
            return future.WaitAndGet();
        }

        public static IClientMessage InvokeOnConnection(this IClientInvocationService service, IClientMessage request, ClientConnection connection, int timeout = Future.NoLimit)
        {
            var future = new SyncFuture<IClientMessage>();
            service.InvokeOnConnection(request, future, connection);
            return future.WaitAndGet(timeout);
        }

        public static IClientMessage InvokeListenerOnConnection(this IClientInvocationService service, IClientMessage request, DistributedEventHandler eventHandler, ClientConnection connection)
        {
            var future = new SyncFuture<IClientMessage>();
            service.InvokeListenerOnConnection(request, future, eventHandler, connection);
            return future.WaitAndGet();
        }
    }

    static class AsyncClientInvocationService
    {
        public static Task<IClientMessage> InvokeOnPartitionAsync(this IClientInvocationService service, IClientMessage request, int partitionId)
        {
            var future = AsyncFuture<IClientMessage>.Create(out var tcs, null);
            service.InvokeOnPartition(request, future, partitionId);
            return tcs.Task;
        }

        public static Task<IClientMessage> InvokeOnTargetAsync(this IClientInvocationService service, IClientMessage request, Address target)
        {
            var future = AsyncFuture<IClientMessage>.Create(out var tcs, null);
            service.InvokeOnTarget(request, future, target);
            return tcs.Task;
        }

        public static Task<IClientMessage> InvokeOnKeyOwnerAsync(this IClientInvocationService service, IClientMessage request, object key)
        {
            var future = AsyncFuture<IClientMessage>.Create(out var tcs, null);
            service.InvokeOnKeyOwner(request, future, key);
            return tcs.Task;
        }
    }
}
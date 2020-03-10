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

#if !NETFRAMEWORK
using System;
using System.Threading;
using Thrift.Protocol;

namespace Hazelcast.Remote
{
    public partial class ThreadSafeRemoteController : RemoteController.Client, IRemoteController
    {
        private readonly object _mutex = new object();

        public ThreadSafeRemoteController(TProtocol prot) : base(prot)
        {
        }

        public ThreadSafeRemoteController(TProtocol iprot, TProtocol oprot) : base(iprot, oprot)
        {
        }

        bool IRemoteController.ping()
        {
            return ThreadSafeCall(() => pingAsync().Result);
        }

        bool IRemoteController.clean()
        {
            return ThreadSafeCall(() => cleanAsync().Result);
        }

        bool IRemoteController.exit()
        {
            return ThreadSafeCall(() => exitAsync().Result);
        }

        Cluster IRemoteController.createCluster(string hzVersion, string xmlconfig)
        {
            return ThreadSafeCall(() => createClusterAsync(hzVersion, xmlconfig).Result);
        }

        Member IRemoteController.startMember(string clusterId)
        {
            return ThreadSafeCall(() => startMemberAsync(clusterId).Result);
        }

        bool IRemoteController.shutdownMember(string clusterId, string memberId)
        {
            return ThreadSafeCall(() => shutdownMemberAsync(clusterId, memberId).Result);
        }

        bool IRemoteController.terminateMember(string clusterId, string memberId)
        {
            return ThreadSafeCall(() => terminateMemberAsync(clusterId, memberId).Result);
        }

        bool IRemoteController.suspendMember(string clusterId, string memberId)
        {
            return ThreadSafeCall(() => suspendMemberAsync(clusterId, memberId).Result);
        }

        bool IRemoteController.resumeMember(string clusterId, string memberId)
        {
            return ThreadSafeCall(() => resumeMemberAsync(clusterId, memberId).Result);
        }

        bool IRemoteController.shutdownCluster(string clusterId)
        {
            return ThreadSafeCall(() => shutdownClusterAsync(clusterId).Result);
        }

        bool IRemoteController.terminateCluster(string clusterId)
        {
            return ThreadSafeCall(() => terminateClusterAsync(clusterId).Result);
        }

        Cluster IRemoteController.splitMemberFromCluster(string memberId)
        {
            return ThreadSafeCall(() => splitMemberFromClusterAsync(memberId).Result);
        }

        Cluster IRemoteController.mergeMemberToCluster(string clusterId, string memberId)
        {
            return ThreadSafeCall(() => mergeMemberToClusterAsync(clusterId, memberId).Result);
        }

        Response IRemoteController.executeOnController(string clusterId, string script, Lang lang)
        {
            return ThreadSafeCall(() => executeOnControllerAsync(clusterId, script, lang).Result);
        }

        private T ThreadSafeCall<T>(Func<T> func)
        {
            Monitor.Enter(_mutex);
            try
            {
                return func();
            }
            finally
            {
                Monitor.Exit(_mutex);
            }
        }
    }
}
#endif

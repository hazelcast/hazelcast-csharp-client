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

using System;
using System.Threading;
using Thrift.Protocol;

namespace Hazelcast.Remote
{
    /// <summary>
    /// Thread safe remote controller
    /// </summary>
    public class ThreadSafeRemoteController : RemoteController.Client
    {
        private readonly object _mutex = new object();

        public ThreadSafeRemoteController(TProtocol prot) : base(prot)
        {
        }

        public ThreadSafeRemoteController(TProtocol iprot, TProtocol oprot) : base(iprot, oprot)
        {
        }
        
        public override bool ping()
        {
            return ThreadSafeCall(() => base.ping());
        }

        public override bool clean()
        {
            return ThreadSafeCall(() => base.clean());
        }

        public override bool exit()
        {
            return ThreadSafeCall(() => base.exit());
        }

        public override Cluster createCluster(string hzVersion, string xmlconfig)
        {
            return ThreadSafeCall(() => base.createCluster(hzVersion, xmlconfig));
        }

        public override Member startMember(string clusterId)
        {
            return ThreadSafeCall(() => base.startMember(clusterId));
        }

        public override bool shutdownMember(string clusterId, string memberId)
        {
            return ThreadSafeCall(() => base.shutdownMember(clusterId, memberId));
        }

        public override bool terminateMember(string clusterId, string memberId)
        {
            return ThreadSafeCall(() => base.terminateMember(clusterId, memberId));
        }

        public override bool suspendMember(string clusterId, string memberId)
        {
            return ThreadSafeCall(() => base.suspendMember(clusterId, memberId));
        }

        public override bool resumeMember(string clusterId, string memberId)
        {
            return ThreadSafeCall(() => base.resumeMember(clusterId, memberId));
        }

        public override bool shutdownCluster(string clusterId)
        {
            return ThreadSafeCall(() => base.shutdownCluster(clusterId));
        }

        public override bool terminateCluster(string clusterId)
        {
            return ThreadSafeCall(() => base.terminateCluster(clusterId));
        }

        public override Cluster splitMemberFromCluster(string memberId)
        {
            return ThreadSafeCall(() => base.splitMemberFromCluster(memberId));
        }

        public override Cluster mergeMemberToCluster(string clusterId, string memberId)
        {
            return ThreadSafeCall(() => base.mergeMemberToCluster(clusterId, memberId));
        }

        public override Response executeOnController(string clusterId, string script, Lang lang)
        {
            return ThreadSafeCall(() => base.executeOnController(clusterId, script, lang));
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
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

using Thrift.Protocol;

namespace Hazelcast.Testing.Remote
{
    public partial class ThreadSafeRemoteController : RemoteController.Client, IRemoteController
    {
        private readonly object _lockobj = new object();

        public ThreadSafeRemoteController(TProtocol prot) 
            : base(prot)
        { }

        public ThreadSafeRemoteController(TProtocol iprot, TProtocol oprot) 
            : base(iprot, oprot)
        { }

        // TODO: stop using async .Result!

        bool IRemoteController.ping()
        {
            lock (_lockobj) return pingAsync().Result;
        }

        bool IRemoteController.clean()
        {
            lock (_lockobj) return cleanAsync().Result;
        }

        bool IRemoteController.exit()
        {
            lock (_lockobj) return exitAsync().Result;
        }

        Cluster IRemoteController.createCluster(string hzVersion, string xmlconfig)
        {
            lock (_lockobj) return createClusterAsync(hzVersion, xmlconfig).Result;
        }

        Member IRemoteController.startMember(string clusterId)
        {
            lock (_lockobj) return startMemberAsync(clusterId).Result;
        }

        bool IRemoteController.shutdownMember(string clusterId, string memberId)
        {
            lock (_lockobj) return shutdownMemberAsync(clusterId, memberId).Result;
        }

        bool IRemoteController.terminateMember(string clusterId, string memberId)
        {
            lock (_lockobj) return terminateMemberAsync(clusterId, memberId).Result;
        }

        bool IRemoteController.suspendMember(string clusterId, string memberId)
        {
            lock (_lockobj) return suspendMemberAsync(clusterId, memberId).Result;
        }

        bool IRemoteController.resumeMember(string clusterId, string memberId)
        {
            lock (_lockobj) return resumeMemberAsync(clusterId, memberId).Result;
        }

        bool IRemoteController.shutdownCluster(string clusterId)
        {
            lock (_lockobj) return shutdownClusterAsync(clusterId).Result;
        }

        bool IRemoteController.terminateCluster(string clusterId)
        {
            lock (_lockobj) return terminateClusterAsync(clusterId).Result;
        }

        Cluster IRemoteController.splitMemberFromCluster(string memberId)
        {
            lock (_lockobj) return splitMemberFromClusterAsync(memberId).Result;
        }

        Cluster IRemoteController.mergeMemberToCluster(string clusterId, string memberId)
        {
            lock (_lockobj) return mergeMemberToClusterAsync(clusterId, memberId).Result;
        }

        Response IRemoteController.executeOnController(string clusterId, string script, Lang lang)
        {
            lock (_lockobj) return executeOnControllerAsync(clusterId, script, lang).Result;
        }
    }
}

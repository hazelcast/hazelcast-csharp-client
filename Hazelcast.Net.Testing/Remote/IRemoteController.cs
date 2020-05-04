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

namespace Hazelcast.Testing.Remote
{
    public interface IRemoteController
    {
        bool ping();

        bool clean();

        bool exit();

        Cluster createCluster(string hzVersion, string xmlconfig);

        Member startMember(string clusterId);

        bool shutdownMember(string clusterId, string memberId);

        bool terminateMember(string clusterId, string memberId);

        bool suspendMember(string clusterId, string memberId);

        bool resumeMember(string clusterId, string memberId);

        bool shutdownCluster(string clusterId);

        bool terminateCluster(string clusterId);

        Cluster splitMemberFromCluster(string memberId);

        Cluster mergeMemberToCluster(string clusterId, string memberId);

        Response executeOnController(string clusterId, string script, Lang lang);
    }
}
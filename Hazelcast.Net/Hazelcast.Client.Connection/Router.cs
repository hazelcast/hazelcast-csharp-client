// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.IO;

namespace Hazelcast.Client.Connection
{
    internal class Router
    {
        private readonly ILoadBalancer _loadBalancer;

        internal Router(ILoadBalancer loadBalancer)
        {
            _loadBalancer = loadBalancer;
        }

        public virtual Address Next()
        {
            var member = _loadBalancer.Next();
            return member == null ? null : member.GetAddress();
        }
    }
}
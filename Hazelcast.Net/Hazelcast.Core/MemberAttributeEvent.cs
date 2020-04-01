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

using Hazelcast.Client.Spi;

namespace Hazelcast.Core
{
    public class MemberAttributeEvent : MembershipEvent
    {
        private readonly string _key;
        private readonly Member _member;
        private readonly MemberAttributeOperationType _operationType;
        private readonly object _value;

        public MemberAttributeEvent() : base(null, null, MemberAttributeChanged, null)
        {
        }

        public MemberAttributeEvent(ICluster cluster, IMember member, MemberAttributeOperationType operationType,
            string key, object value)
            : base(cluster, member, MemberAttributeChanged, null)
        {
            _member = (Member) member;
            _operationType = operationType;
            _key = key;
            _value = value;
        }

        public string GetKey()
        {
            return _key;
        }

        public override IMember GetMember()
        {
            return _member;
        }

        public MemberAttributeOperationType GetOperationType()
        {
            return _operationType;
        }

        public object GetValue()
        {
            return _value;
        }
    }
}
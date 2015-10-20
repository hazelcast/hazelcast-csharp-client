/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

﻿using Hazelcast.Client.Spi;

namespace Hazelcast.Core
{
    public class MemberAttributeEvent : MembershipEvent
    {
        private readonly string key;
        private readonly Member member;
        private readonly MemberAttributeOperationType operationType;
        private readonly object value;

        public MemberAttributeEvent() : base(null, null, MemberAttributeChanged, null)
        {
        }

        public MemberAttributeEvent(ICluster cluster, IMember member, MemberAttributeOperationType operationType,
            string key, object value)
            : base(cluster, member, MemberAttributeChanged, null)
        {
            this.member = (Member) member;
            this.operationType = operationType;
            this.key = key;
            this.value = value;
        }

        public string GetKey()
        {
            return key;
        }

        public override IMember GetMember()
        {
            return member;
        }

        public MemberAttributeOperationType GetOperationType()
        {
            return operationType;
        }

        public object GetValue()
        {
            return value;
        }
    }
}
// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Client.Spi
{
    internal class MemberAttributeChange
    {
        public MemberAttributeChange()
        {
        }


        public MemberAttributeChange(string uuid, MemberAttributeOperationType operationType
            , string key, string value)
        {
            Uuid = uuid;
            OperationType = operationType;
            Key = key;
            Value = value;
        }

        public string Value { get; private set; }

        public string Uuid { get; private set; }

        public MemberAttributeOperationType OperationType { get; private set; }

        public string Key { get; private set; }
    }
}
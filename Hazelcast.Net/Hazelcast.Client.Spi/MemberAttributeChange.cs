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

using Hazelcast.IO;

namespace Hazelcast.Client.Spi
{
    internal class MemberAttributeChange 
    {
        private string key;
        private MemberAttributeOperationType operationType;
        private string uuid;
        private string value;

        public MemberAttributeChange()
        {
        }

        public string Value
        {
            get { return value; }
        }

        public string Uuid
        {
            get { return uuid; }
        }

        public MemberAttributeOperationType OperationType
        {
            get { return operationType; }
        }

        public string Key
        {
            get { return key; }
        }


        public MemberAttributeChange(string uuid, MemberAttributeOperationType operationType
            , string key, string value)
        {
            this.uuid = uuid;
            this.operationType = operationType;
            this.key = key;
            this.value = value;
        }
    }
}
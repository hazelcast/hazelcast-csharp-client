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

using Hazelcast.IO.Serialization;

namespace Hazelcast.Client
{
    internal class DistributedObjectInfo
    {
        private string name;
        private string serviceName;

        internal DistributedObjectInfo()
        {
        }

        public DistributedObjectInfo(string serviceName, string name)
        {
            this.name = name;
            this.serviceName = serviceName;
        }

        //REQUIRED

        public virtual string GetServiceName()
        {
            return serviceName;
        }

        public virtual string GetName()
        {
            return name;
        }

        protected bool Equals(DistributedObjectInfo other)
        {
            return string.Equals(name, other.name) && string.Equals(serviceName, other.serviceName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DistributedObjectInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((name != null ? name.GetHashCode() : 0)*397) ^ (serviceName != null ? serviceName.GetHashCode() : 0);
            }
        }
    }
}
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

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }
            var that = (DistributedObjectInfo) o;
            if (name != null ? !name.Equals(that.name) : that.name != null)
            {
                return false;
            }
            if (serviceName != null ? !serviceName.Equals(that.serviceName) : that.serviceName != null)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int result = serviceName != null ? serviceName.GetHashCode() : 0;
            result = 31*result + (name != null ? name.GetHashCode() : 0);
            return result;
        }
    }
}
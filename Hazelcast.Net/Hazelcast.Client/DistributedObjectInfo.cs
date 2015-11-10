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

namespace Hazelcast.Client
{
    internal class DistributedObjectInfo
    {
        private readonly string _name;
        private readonly string _serviceName;

        internal DistributedObjectInfo()
        {
        }

        public DistributedObjectInfo(string serviceName, string name)
        {
            _name = name;
            _serviceName = serviceName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DistributedObjectInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name != null ? _name.GetHashCode() : 0)*397) ^
                       (_serviceName != null ? _serviceName.GetHashCode() : 0);
            }
        }

        public virtual string GetName()
        {
            return _name;
        }

        //REQUIRED

        public virtual string GetServiceName()
        {
            return _serviceName;
        }

        protected bool Equals(DistributedObjectInfo other)
        {
            return string.Equals(_name, other._name) && string.Equals(_serviceName, other._serviceName);
        }
    }
}
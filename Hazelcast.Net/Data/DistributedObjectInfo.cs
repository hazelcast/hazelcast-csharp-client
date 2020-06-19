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

using System.Text;

namespace Hazelcast.Data
{
    internal class DistributedObjectInfo
    {
        public DistributedObjectInfo(string serviceName, string name)
        {
            Name = name;
            ServiceName = serviceName;
        }

        public string Name { get; }

        public string ServiceName { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is DistributedObjectInfo other && Equals(this, other);
        }

        private static bool Equals(DistributedObjectInfo obj1, DistributedObjectInfo obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 is null) return false;

            return obj1.Name == obj2.Name &&
                   obj1.ServiceName == obj2.ServiceName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^
                       (ServiceName != null ? ServiceName.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("DistributedObjectInfo");
            sb.Append("{service='").Append(ServiceName).Append('\'');
            sb.Append(", objectName=").Append(Name);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
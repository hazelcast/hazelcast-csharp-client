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

using System;
using System.Text;

namespace Hazelcast.Data
{
    internal class DistributedObjectInfo : IEquatable<DistributedObjectInfo>
    {
        public DistributedObjectInfo(string serviceName, string name)
        {
            Name = name;
            ServiceName = serviceName;
        }

        public string Name { get; }

        public string ServiceName { get; }

        public override bool Equals(object obj)
            => Equals(obj as DistributedObjectInfo);

        public bool Equals(DistributedObjectInfo other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                Name == other.Name &&
                ServiceName == other.ServiceName;
        }

        public static bool operator ==(DistributedObjectInfo left, DistributedObjectInfo right)
            => left is null ? right is null : left.Equals(right);

        public static bool operator !=(DistributedObjectInfo left, DistributedObjectInfo right)
            => !(left == right);

        public override int GetHashCode()
        {
            return HashCode.Combine(ServiceName, Name);
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

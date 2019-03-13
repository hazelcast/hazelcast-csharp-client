// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Client
{
    internal class DistributedObjectInfo
    {
        private readonly string _objectName;
        private readonly string _serviceName;

        public DistributedObjectInfo(string serviceName, string objectName)
        {
            _objectName = objectName;
            _serviceName = serviceName;
        }
        
        public string ObjectName
        {
            get { return _objectName; }
        }

        public  string ServiceName
        {
            get { return _serviceName; }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            var other= (DistributedObjectInfo) obj;
            return string.Equals(_objectName, other._objectName) && string.Equals(_serviceName, other._serviceName);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_objectName != null ? _objectName.GetHashCode() : 0) * 397) ^
                       (_serviceName != null ? _serviceName.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("DistributedObjectInfo");
            sb.Append("{service='").Append(_serviceName).Append('\'');
            sb.Append(", objectName=").Append(_objectName);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
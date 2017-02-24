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

using System.Text;

namespace Hazelcast.Client
{
    internal sealed class ClientPrincipal
    {
        private readonly string _ownerUuid;
        private readonly string _uuid;

        public ClientPrincipal()
        {
        }

        public ClientPrincipal(string uuid, string ownerUuid)
        {
            _uuid = uuid;
            _ownerUuid = ownerUuid;
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
            var that = (ClientPrincipal) o;
            if (_ownerUuid != null ? !_ownerUuid.Equals(that._ownerUuid) : that._ownerUuid != null)
            {
                return false;
            }
            if (_uuid != null ? !_uuid.Equals(that._uuid) : that._uuid != null)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            var result = _uuid != null ? _uuid.GetHashCode() : 0;
            result = 31*result + (_ownerUuid != null ? _ownerUuid.GetHashCode() : 0);
            return result;
        }

        public string GetOwnerUuid()
        {
            return _ownerUuid;
        }

        public string GetUuid()
        {
            return _uuid;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("ClientPrincipal{");
            sb.Append("uuid='").Append(_uuid).Append('\'');
            sb.Append(", ownerUuid='").Append(_ownerUuid).Append('\'');
            sb.Append('}');
            return sb.ToString();
        }
    }
}
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

using System.Text;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client
{
    internal sealed class ClientPrincipal 
    {
        private string ownerUuid;
        private string uuid;

        public ClientPrincipal()
        {
        }

        public ClientPrincipal(string uuid, string ownerUuid)
        {
            this.uuid = uuid;
            this.ownerUuid = ownerUuid;
        }

        public string GetUuid()
        {
            return uuid;
        }

        public string GetOwnerUuid()
        {
            return ownerUuid;
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
            if (ownerUuid != null ? !ownerUuid.Equals(that.ownerUuid) : that.ownerUuid != null)
            {
                return false;
            }
            if (uuid != null ? !uuid.Equals(that.uuid) : that.uuid != null)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int result = uuid != null ? uuid.GetHashCode() : 0;
            result = 31*result + (ownerUuid != null ? ownerUuid.GetHashCode() : 0);
            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("ClientPrincipal{");
            sb.Append("uuid='").Append(uuid).Append('\'');
            sb.Append(", ownerUuid='").Append(ownerUuid).Append('\'');
            sb.Append('}');
            return sb.ToString();
        }
    }
}
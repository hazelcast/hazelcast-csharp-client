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

namespace Hazelcast.Client
{
    internal sealed class ClientPrincipal : IEquatable<ClientPrincipal>
    {
        public Guid OwnerUuid { get; }
        public Guid Uuid { get; }

        public ClientPrincipal()
        {
        }

        public ClientPrincipal(Guid uuid, Guid ownerUuid)
        {
            Uuid = uuid;
            OwnerUuid = ownerUuid;
        }

        public bool Equals(ClientPrincipal other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return OwnerUuid.Equals(other.OwnerUuid) && Uuid.Equals(other.Uuid);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ClientPrincipal other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (OwnerUuid.GetHashCode() * 397) ^ Uuid.GetHashCode();
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder("ClientPrincipal{");
            sb.Append("uuid='").Append(Uuid).Append('\'');
            sb.Append(", ownerUuid='").Append(OwnerUuid).Append('\'');
            sb.Append('}');
            return sb.ToString();
        }
    }
}
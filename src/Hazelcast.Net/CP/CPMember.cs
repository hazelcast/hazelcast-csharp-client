// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Networking;
namespace Hazelcast.CP
{
    internal class CPMember
    {
        public CPMember(Guid uuid, NetworkAddress address)
        {
            Uuid = uuid;
            Address = address;
        }

        public Guid Uuid { get; }
        public NetworkAddress Address { get; }

        public override string ToString()
        {
            return $"CPMember: {{ UUID: {Uuid}, Address: {Address} }}";
        }

        public override bool Equals(object obj)
        {
            if(this == obj) return true;
            
            if (obj is not CPMember other) return false;

            return Uuid == other.Uuid && Address.Equals(other.Address);
        }

        public override int GetHashCode()
        {
            return 31 * Uuid.GetHashCode() + Address.GetHashCode();
        }
    }
}

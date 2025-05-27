// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Serialization;

namespace Hazelcast.Models
{
    internal class EndpointQualifier : IIdentifiedDataSerializable, IEquatable<EndpointQualifier>
    {
        public EndpointQualifier(ProtocolType type, string identifier)
        {
            Type = type;
            Identifier = identifier;
        }

        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            Type = (ProtocolType) input.ReadInt();
            Identifier = input.ReadString();
        }

        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.WriteInt((int) Type);
            output.WriteString(Identifier);
        }

        public int FactoryId => 0; // ClusterDataSerializerHook.F_ID

        public int ClassId => 17; // ClusterDataSerializerHook.ENDPOINT_QUALIFIER

        public string Identifier { get; private set; }

        public ProtocolType Type { get; private set; }

        public bool Equals(EndpointQualifier other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                Identifier == other.Identifier &&
                Type == other.Type;
        }

        public override bool Equals(object obj)
            => Equals(obj as EndpointQualifier);

        public static bool operator ==(EndpointQualifier left, EndpointQualifier right)
            => left is null ? right is null : left.Equals(right);

        public static bool operator !=(EndpointQualifier left, EndpointQualifier right)
            => !(left == right);

        public override string ToString()
            => $"(EndpointQualifier Identifier = '{Identifier}', Type = '{Type}')";

        public override int GetHashCode()
        {
            return HashCode.Combine(Identifier, (int) Type);
        }
    }
}

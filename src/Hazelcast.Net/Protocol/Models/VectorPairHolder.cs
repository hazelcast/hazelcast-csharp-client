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
using System.Linq;
namespace Hazelcast.Protocol.Models
{
    public class VectorPairHolder
    {
        public const string SingleVectorName = "";
        public const byte DenseFloatVector = 0;
        public byte Type { get; }


        public string Name { get; }
        public float[] Vector { get; }

        public VectorPairHolder(string name, byte type, float[] vector)
        {
            Type = type;
            Name = name;
            Vector = vector;
        }


        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            var that = (VectorPairHolder) obj;
            return Name.Equals(that.Name) && Vector.SequenceEqual(that.Vector);
        }

        public override int GetHashCode()
        {
            var result = 31 * Name.GetHashCode();
            result = 31 * result + Type.GetHashCode();
            result = 31 * result + Vector.GetHashCode();
            return result;
        }

    }
}

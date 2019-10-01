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

using System;

namespace Hazelcast.CP
{
    class RaftGroupId : IEquatable<RaftGroupId>
    {
        public RaftGroupId(string name, long seed, long commitIndex)
        {
            Name = name;
            Seed = seed;
            Id = commitIndex;
        }

        public long Id { get; }

        public long Seed { get; }

        public string Name { get; }

        public bool Equals(RaftGroupId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Seed == other.Seed && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RaftGroupId) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Seed.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(RaftGroupId left, RaftGroupId right) => Equals(left, right);

        public static bool operator !=(RaftGroupId left, RaftGroupId right) => !Equals(left, right);

        public override string ToString() => $"{nameof(Id)}: {Id}, {nameof(Seed)}: {Seed}, {nameof(Name)}: {Name}";
    }
}
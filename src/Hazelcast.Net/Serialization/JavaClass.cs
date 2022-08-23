// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization
{
    /// <summary>
    ///  Class for serializing/deserializing Java Class types
    /// </summary>
    public class JavaClass
    {
        public JavaClass(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return obj is JavaClass thing && EqualsN(this, thing);
        }

        protected bool Equals(JavaClass other)
        {
            if (other is null) return false;
            return ReferenceEquals(this, other) || EqualsN(this, other);
        }

        public static bool Equals(JavaClass left, JavaClass right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return EqualsN(left, right);
        }

        private static bool EqualsN(JavaClass left, JavaClass right)
            => string.Equals(left.Name, right.Name, StringComparison.Ordinal);

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode(StringComparison.Ordinal) : 0;
        }

        public override string ToString()
        {
            return $"Name: {Name}";
        }

    }
}

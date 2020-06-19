﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
    ///  Class for serializing/deserializing Java enums
    /// </summary>
    public class JavaEnum
    {
        public string Type { get; private set; }
        public string Value { get; private set; }

        public JavaEnum(string type, string value)
        {
            Type = type;
            Value = value;
        }

        protected bool Equals(JavaEnum other)
        {
            if (other is null) return false;
            return string.Equals(Type, other.Type, StringComparison.Ordinal) &&
                   string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((JavaEnum) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return ((Type != null ? Type.GetHashCode(StringComparison.Ordinal) : 0)*397) ^ (Value != null ? Value.GetHashCode(StringComparison.Ordinal) : 0);
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        public override string ToString()
        {
            return $"Type: {Type}, Value: {Value}";
        }
    }
}

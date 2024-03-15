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
using Hazelcast.Serialization;

namespace Hazelcast.Query
{
    internal class EqualPredicate : IPredicate, IIdentifiedDataSerializable
    {
        protected string AttributeName { get; private set; }

        protected object Value { get; private set; }

        public EqualPredicate()
        { }

        public EqualPredicate(string attributeName, object value)
        {
            AttributeName = attributeName;
            Value = value;
        }

        public void ReadData(IObjectDataInput input)
        {
            AttributeName = input.ReadString();
            Value = input.ReadObject<object>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteString(AttributeName);
            output.WriteObject(Value);
        }

        public int FactoryId => FactoryIds.PredicateFactoryId;

        public virtual int ClassId => PredicateDataSerializerHook.EqualPredicate;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is EqualPredicate other && Equals(this, other);
        }

        private static bool Equals(EqualPredicate left, EqualPredicate right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

            return left.AttributeName == right.AttributeName &&
                   Equals(left.Value, right.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return ((AttributeName != null ? AttributeName.GetHashCode(StringComparison.Ordinal) : 0)*397) ^
                       (Value != null ? Value.GetHashCode() : 0);
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        public override string ToString()
        {
            return "(" + AttributeName + " == " + Value + ")";
        }
    }
}

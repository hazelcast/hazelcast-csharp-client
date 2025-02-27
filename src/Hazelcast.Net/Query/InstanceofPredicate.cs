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

namespace Hazelcast.Query
{
    internal class InstanceofPredicate : IPredicate, IIdentifiedDataSerializable
    {
        private string _className;

        public InstanceofPredicate()
        {
        }

        public InstanceofPredicate(string className)
        {
            _className = className;
        }

        public void ReadData(IObjectDataInput input)
        {
            _className = input.ReadString();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteString(_className);
        }

        public int FactoryId => FactoryIds.PredicateFactoryId;

        public int ClassId => PredicateDataSerializerHook.InstanceofPredicate;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is InstanceofPredicate other && Equals(this, other);
        }

        private static bool Equals(InstanceofPredicate left, InstanceofPredicate right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

            return left._className == right._className;
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return _className != null ? _className.GetHashCode(StringComparison.Ordinal) : 0;
            // ReSharper enable NonReadonlyMemberInGetHashCode
        }

        public override string ToString()
        {
            return "INSTANCEOF(" + _className + ")";
        }
    }
}

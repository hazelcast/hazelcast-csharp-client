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

using Hazelcast.Data;
using Hazelcast.Serialization;

namespace Hazelcast.Predicates
{
    public class InstanceofPredicate : IPredicate
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
            _className = input.ReadUtf();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUtf(_className);
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.InstanceofPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is InstanceofPredicate other && Equals(this, other);
        }

        private static bool Equals(InstanceofPredicate obj1, InstanceofPredicate obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 is null) return false;

            return obj1._className == obj2._className;
        }

        public override int GetHashCode()
        {
            return _className != null ? _className.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return " InstanceOf " + _className;
        }
    }
}
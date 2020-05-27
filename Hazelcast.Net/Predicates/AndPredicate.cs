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

using System.Linq;
using Hazelcast.Serialization;

namespace Hazelcast.Predicates
{
    public class AndPredicate : IPredicate
    {
        private IPredicate[] _predicates;

        public AndPredicate()
        {
        }

        public AndPredicate(params IPredicate[] predicates)
        {
            _predicates = predicates;
        }

        public void ReadData(IObjectDataInput input)
        {
            var size = input.ReadInt();
            _predicates = new IPredicate[size];
            for (var i = 0; i < size; i++)
            {
                _predicates[i] = input.ReadObject<IPredicate>();
            }
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(_predicates.Length);
            foreach (var predicate in _predicates)
            {
                output.WriteObject(predicate);
            }
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.AndPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is AndPredicate other && Equals(this, other);
        }

        private static bool Equals(AndPredicate obj1, AndPredicate obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 is null) return false;

            return obj1._predicates.SequenceEqual(obj2._predicates);
        }

        public override int GetHashCode() => _predicates != null ? _predicates.GetHashCode() : 0;

        public override string ToString()
        {
            return string.Join(" AND ", _predicates.GetEnumerator());
        }
    }
}
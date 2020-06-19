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

using System;
using Hazelcast.Serialization;

namespace Hazelcast.Predicates
{
    public class NotPredicate : IPredicate
    {
        private IPredicate _predicate;

        public NotPredicate()
        {
        }

        public NotPredicate(IPredicate predicate)
        {
            _predicate = predicate;
        }

        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            _predicate = input.ReadObject<IPredicate>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.WriteObject(_predicate);
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.NotPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is NotPredicate other && Equals(this, other);
        }

        private static bool Equals(NotPredicate obj1, NotPredicate obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 is null) return false;

            return Equals(obj1._predicate, obj2._predicate);
        }

        public override int GetHashCode()
        {
            return (_predicate != null ? _predicate.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return "NOT(" + _predicate + ")";
        }
    }
}

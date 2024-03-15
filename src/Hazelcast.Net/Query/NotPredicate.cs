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

using Hazelcast.Serialization;

namespace Hazelcast.Query
{
    internal class NotPredicate : IPredicate, IIdentifiedDataSerializable
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
            _predicate = input.ReadObject<IPredicate>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteObject(_predicate);
        }

        public int FactoryId => FactoryIds.PredicateFactoryId;

        public int ClassId => PredicateDataSerializerHook.NotPredicate;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is NotPredicate other && Equals(this, other);
        }

        private static bool Equals(NotPredicate left, NotPredicate right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

            return Equals(left._predicate, right._predicate);
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

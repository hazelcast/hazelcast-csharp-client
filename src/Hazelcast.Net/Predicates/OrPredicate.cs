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
using System.Linq;
using Hazelcast.Serialization;

namespace Hazelcast.Predicates
{
    internal abstract class LogicalPredicateBase : IPredicate, IIdentifiedDataSerializable
    {
        // TODO: consider using a list instead of an array?
        private IPredicate[] _predicates;

        internal LogicalPredicateBase(IPredicate[] predicates)
        {
            _predicates = predicates;
        }

        public int FactoryId => FactoryIds.PredicateFactoryId;

        public abstract int ClassId { get; }

        protected internal IPredicate[] ConcatInternal(IPredicate predicate)
        {
            var predicates = new IPredicate[_predicates.Length + 1];
            Array.Copy(_predicates, predicates, _predicates.Length);
            predicates[_predicates.Length] = predicate;
            return predicates;
        }

        protected internal IPredicate Last
        {
            get => _predicates[^1];
            set => _predicates[^1] = value;
        }

        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var size = input.ReadInt();
            _predicates = new IPredicate[size];
            for (var i = 0; i < size; i++)
            {
                _predicates[i] = input.ReadObject<IPredicate>();
            }
        }

        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            output.Write(_predicates.Length);
            foreach (var predicate in _predicates)
            {
                output.WriteObject(predicate);
            }
        }

        private static bool Equals(LogicalPredicateBase left, LogicalPredicateBase right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

            return left._predicates.SequenceEqual(right._predicates);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is LogicalPredicateBase other &&
                   obj.GetType() == GetType() &&
                   Equals(this, other);
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return _predicates != null ? _predicates.GetHashCode() : 0;
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        public override string ToString()
        {
            var op = GetType() == typeof (OrPredicate) ? "OR" : "AND";
            return op + "(" + string.Join(", ", _predicates.Select(x => x.ToString())) + ")";
        }
    }

    internal class OrPredicate : LogicalPredicateBase
    {
        public OrPredicate()
            : this(Array.Empty<IPredicate>())
        { }

        public OrPredicate(params IPredicate[] predicates)
            : base(predicates)
        { }

        public override int ClassId => PredicateDataSerializerHook.OrPredicate;

        public OrPredicate Concat(IPredicate predicate)
            => new OrPredicate(ConcatInternal(predicate));
    }
}

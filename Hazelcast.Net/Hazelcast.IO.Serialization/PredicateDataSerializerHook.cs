// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;

namespace Hazelcast.IO.Serialization
{
    public class PredicateDataSerializerHook : IDataSerializerHook
    {
        private const int FactoryId = FactoryIds.PredicateFactoryId;
        public const int SqlPredicate = 0;
        public const int AndPredicate = 1;
        public const int BetweenPredicate = 2;
        public const int EqualPredicate = 3;
        public const int GreaterLessPredicate = 4;
        public const int LikePredicate = 5;
        public const int ILikePredicate = 6;
        public const int InPredicate = 7;
        public const int InstanceofPredicate = 8;
        public const int NotEqualPredicate = 9;
        public const int NotPredicate = 10;
        public const int OrPredicate = 11;
        public const int RegexPredicate = 12;
        public const int FalsePredicate = 13;
        public const int TruePredicate = 14;
        public const int PagingPredicate = 15;

        public IDataSerializableFactory CreateFactory()
        {
            var constructors = new Func<IIdentifiedDataSerializable>[PagingPredicate];
            constructors[SqlPredicate] = () => new SqlPredicate();
            constructors[AndPredicate] = () => new AndPredicate();
            constructors[BetweenPredicate] = () => new BetweenPredicate();
            constructors[EqualPredicate] = () => new EqualPredicate();
            constructors[GreaterLessPredicate] = () => new GreaterLessPredicate();
            constructors[LikePredicate] = () => new LikePredicate();
            constructors[ILikePredicate] = () => new ILikePredicate();
            constructors[InPredicate] = () => new InPredicate();
            constructors[InstanceofPredicate] = () => new InstanceofPredicate();
            constructors[NotEqualPredicate] = () => new NotEqualPredicate();
            constructors[NotPredicate] = () => new NotPredicate();
            constructors[OrPredicate] = () => new OrPredicate();
            constructors[RegexPredicate] = () => new RegexPredicate();
            constructors[FalsePredicate] = () => new FalsePredicate();
            constructors[TruePredicate] = () => new TruePredicate();
            return new ArrayDataSerializableFactory(constructors);
        }

        public int GetFactoryId()
        {
            return FactoryId;
        }
    }
}
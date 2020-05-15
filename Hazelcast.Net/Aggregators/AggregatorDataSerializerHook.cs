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

namespace Hazelcast.Aggregators
{
    /// <summary>
    /// Provides an <see cref="IDataSerializableFactory"/> for aggregators.
    /// </summary>
    internal class AggregatorDataSerializerHook : IDataSerializerHook
    {
        public const int FactoryId = FactoryIds.AggregatorDsFactoryId;
        public const int BigDecimalAvg = 0;
        public const int BigDecimalSum = 1;
        public const int BigIntAvg = 2;
        public const int BigIntSum = 3;
        public const int Count = 4;
        public const int DistinctValues = 5;
        public const int DoubleAvg = 6;
        public const int DoubleSum = 7;
        public const int FixedSum = 8;
        public const int FloatingPointSum = 9;
        public const int IntAvg = 10;
        public const int IntSum = 11;
        public const int LongAvg = 12;
        public const int LongSum = 13;
        public const int Max = 14;
        public const int Min = 15;
        public const int NumberAvg = 16;

        private const int Len = NumberAvg + 1;

        /// <inheritdoc />
        public IDataSerializableFactory CreateFactory()
        {
            var constructors = new Func<IIdentifiedDataSerializable>[Len];
            constructors[BigIntSum] = () => new BigIntegerSumAggregator();
            constructors[Count] = () => new CountAggregator();
            constructors[DoubleAvg] = () => new DoubleAverageAggregator();
            constructors[DoubleSum] = () => new DoubleSumAggregator();
            constructors[FixedSum] = () => new FixedSumAggregator();
            constructors[FloatingPointSum] = () => new FloatingPointSumAggregator();
            constructors[IntAvg] = () => new IntegerAverageAggregator();
            constructors[IntSum] = () => new IntegerSumAggregator();
            constructors[LongAvg] = () => new LongAverageAggregator();
            constructors[LongSum] = () => new LongSumAggregator();
            constructors[Max] = () => new MaxAggregator<object>();
            constructors[Min] = () => new MinAggregator<object>();
            constructors[NumberAvg] = () => new NumberAverageAggregator();
            return new ArrayDataSerializableFactory(constructors);
        }

        /// <inheritdoc />
        public int GetFactoryId()
        {
            return FactoryId;
        }
    }
}
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
using Hazelcast.Aggregating;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Aggregating
{
    [TestFixture]
    public class AggregatingTests
    {
        private ISerializationService _serializationService;

        [SetUp]
        public void SetUp()
        {
            _serializationService = new SerializationServiceBuilder(new NullLoggerFactory())
                .AddHook<AggregatorDataSerializerHook>() // not pretty in tests, eh?
                .Build();
        }

        [Test]
        public void Tests()
        {
            AssertAggregator((CountAggregator) Aggregate.Count("name"), AggregatorDataSerializerHook.Count);

            AssertAggregator((BigIntegerSumAggregator) Aggregate.BigIntegerSum("name"), AggregatorDataSerializerHook.BigIntSum);
            AssertAggregator((DoubleSumAggregator) Aggregate.DoubleSum("name"), AggregatorDataSerializerHook.DoubleSum);
            AssertAggregator((FixedSumAggregator) Aggregate.FixedPointSum("name"), AggregatorDataSerializerHook.FixedSum);
            AssertAggregator((LongSumAggregator) Aggregate.LongSum("name"), AggregatorDataSerializerHook.LongSum);
            AssertAggregator((FloatingPointSumAggregator) Aggregate.FloatingPointSum("name"), AggregatorDataSerializerHook.FloatingPointSum);
            AssertAggregator((IntegerSumAggregator) Aggregate.IntegerSum("name"), AggregatorDataSerializerHook.IntSum);

            AssertAggregator((NumberAverageAggregator) Aggregate.NumberAvg("name"), AggregatorDataSerializerHook.NumberAvg);
            AssertAggregator((DoubleAverageAggregator) Aggregate.DoubleAvg("name"), AggregatorDataSerializerHook.DoubleAvg);
            AssertAggregator((IntegerAverageAggregator) Aggregate.IntegerAvg("name"), AggregatorDataSerializerHook.IntAvg);
            AssertAggregator((LongAverageAggregator) Aggregate.LongAvg("name"), AggregatorDataSerializerHook.LongAvg);

            AssertAggregator((MaxAggregator<int>) Aggregate.Max<int>("name"), AggregatorDataSerializerHook.Max);
            AssertAggregator((MinAggregator<int>) Aggregate.Min<int>("name"), AggregatorDataSerializerHook.Min);

            AssertAggregator((CountAggregator) Aggregate.Count(), AggregatorDataSerializerHook.Count);

            AssertAggregator((BigIntegerSumAggregator) Aggregate.BigIntegerSum(), AggregatorDataSerializerHook.BigIntSum);
            AssertAggregator((DoubleSumAggregator) Aggregate.DoubleSum(), AggregatorDataSerializerHook.DoubleSum);
            AssertAggregator((FixedSumAggregator) Aggregate.FixedPointSum(), AggregatorDataSerializerHook.FixedSum);
            AssertAggregator((LongSumAggregator) Aggregate.LongSum(), AggregatorDataSerializerHook.LongSum);
            AssertAggregator((FloatingPointSumAggregator) Aggregate.FloatingPointSum(), AggregatorDataSerializerHook.FloatingPointSum);
            AssertAggregator((IntegerSumAggregator) Aggregate.IntegerSum(), AggregatorDataSerializerHook.IntSum);

            AssertAggregator((NumberAverageAggregator) Aggregate.NumberAvg(), AggregatorDataSerializerHook.NumberAvg);
            AssertAggregator((DoubleAverageAggregator) Aggregate.DoubleAvg(), AggregatorDataSerializerHook.DoubleAvg);
            AssertAggregator((IntegerAverageAggregator) Aggregate.IntegerAvg(), AggregatorDataSerializerHook.IntAvg);
            AssertAggregator((LongAverageAggregator) Aggregate.LongAvg(), AggregatorDataSerializerHook.LongAvg);

            AssertAggregator((MaxAggregator<int>) Aggregate.Max<int>(), AggregatorDataSerializerHook.Max);
            AssertAggregator((MinAggregator<int>) Aggregate.Min<int>(), AggregatorDataSerializerHook.Min);
        }

        private void AssertAggregator<TResult>(AggregatorBase<TResult> aggregator, int classId)
        {
            var aggregatorType = aggregator.GetType();

            Assert.That(aggregator.FactoryId, Is.EqualTo(FactoryIds.AggregatorDsFactoryId));
            Assert.That(aggregator.ClassId, Is.EqualTo(classId));

            Assert.Throws<ArgumentException>(() => _ = Aggregate.Count(""));
            Assert.Throws<ArgumentException>(() => _ = Aggregate.Count(null));

            Assert.Throws<ArgumentNullException>(() => aggregator.WriteData(null));
            Assert.Throws<ArgumentNullException>(() => aggregator.ReadData(null));

            using var output = new ByteArrayObjectDataOutput(1024, _serializationService, Endianness.Unspecified);
            aggregator.WriteData(output);

            using var input = new ByteArrayObjectDataInput(output.Buffer, _serializationService, Endianness.Unspecified);
            var a = (AggregatorBase<TResult>) Activator.CreateInstance(aggregatorType);
            a.ReadData(input);

            Assert.That(a.AttributePath, Is.EqualTo(aggregator.AttributePath));

            var data = _serializationService.ToData(aggregator);

            if (aggregatorType.IsGenericType)
            {
                // doh - cannot deserialize generic types?
                IAggregator<object> x = null;

                if (aggregatorType.GetGenericTypeDefinition() == typeof(MaxAggregator<>))
                    x = _serializationService.ToObject<MaxAggregator<object>>(data);
                else if (aggregatorType.GetGenericTypeDefinition() == typeof(MinAggregator<>))
                    x = _serializationService.ToObject<MinAggregator<object>>(data);
                else Assert.Fail("Unsupported generic aggregator type.");

                Assert.That(x.AttributePath, Is.EqualTo(aggregator.AttributePath));
            }
            else
            {
                var x = _serializationService.ToObject<IAggregator<TResult>>(data);
                Assert.That(x.AttributePath, Is.EqualTo(aggregator.AttributePath));
            }
        }
    }
}

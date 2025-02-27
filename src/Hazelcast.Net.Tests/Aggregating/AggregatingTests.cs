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
using Hazelcast.Aggregation;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Aggregating
{
    [TestFixture]
    public class AggregatingTests
    {
        private SerializationService _serializationService;

        [SetUp]
        public void SetUp()
        {
            _serializationService = new SerializationServiceBuilder(new NullLoggerFactory())
                .AddDefinitions(new ConstantSerializerDefinitions()) // use constant serializers not CLR serialization
                .AddHook<AggregatorDataSerializerHook>()
                .Build();
        }

        [Test]
        public void Tests()
        {
            AssertAggregator((CountAggregator) Aggregators.Count("name"), AggregatorDataSerializerHook.Count);

            AssertAggregator((BigIntegerSumAggregator) Aggregators.BigIntegerSum("name"), AggregatorDataSerializerHook.BigIntSum);
            AssertAggregator((DoubleSumAggregator) Aggregators.DoubleSum("name"), AggregatorDataSerializerHook.DoubleSum);
            AssertAggregator((FixedSumAggregator) Aggregators.FixedPointSum("name"), AggregatorDataSerializerHook.FixedSum);
            AssertAggregator((LongSumAggregator) Aggregators.LongSum("name"), AggregatorDataSerializerHook.LongSum);
            AssertAggregator((FloatingPointSumAggregator) Aggregators.FloatingPointSum("name"), AggregatorDataSerializerHook.FloatingPointSum);
            AssertAggregator((IntegerSumAggregator) Aggregators.IntegerSum("name"), AggregatorDataSerializerHook.IntSum);

            AssertAggregator((NumberAverageAggregator) Aggregators.NumberAvg("name"), AggregatorDataSerializerHook.NumberAvg);
            AssertAggregator((DoubleAverageAggregator) Aggregators.DoubleAvg("name"), AggregatorDataSerializerHook.DoubleAvg);
            AssertAggregator((IntegerAverageAggregator) Aggregators.IntegerAvg("name"), AggregatorDataSerializerHook.IntAvg);
            AssertAggregator((LongAverageAggregator) Aggregators.LongAvg("name"), AggregatorDataSerializerHook.LongAvg);

            AssertAggregator((MaxAggregator<int>) Aggregators.Max<int>("name"), AggregatorDataSerializerHook.Max);
            AssertAggregator((MinAggregator<int>) Aggregators.Min<int>("name"), AggregatorDataSerializerHook.Min);

            AssertAggregator((CountAggregator) Aggregators.Count(), AggregatorDataSerializerHook.Count);

            AssertAggregator((BigIntegerSumAggregator) Aggregators.BigIntegerSum(), AggregatorDataSerializerHook.BigIntSum);
            AssertAggregator((DoubleSumAggregator) Aggregators.DoubleSum(), AggregatorDataSerializerHook.DoubleSum);
            AssertAggregator((FixedSumAggregator) Aggregators.FixedPointSum(), AggregatorDataSerializerHook.FixedSum);
            AssertAggregator((LongSumAggregator) Aggregators.LongSum(), AggregatorDataSerializerHook.LongSum);
            AssertAggregator((FloatingPointSumAggregator) Aggregators.FloatingPointSum(), AggregatorDataSerializerHook.FloatingPointSum);
            AssertAggregator((IntegerSumAggregator) Aggregators.IntegerSum(), AggregatorDataSerializerHook.IntSum);

            AssertAggregator((NumberAverageAggregator) Aggregators.NumberAvg(), AggregatorDataSerializerHook.NumberAvg);
            AssertAggregator((DoubleAverageAggregator) Aggregators.DoubleAvg(), AggregatorDataSerializerHook.DoubleAvg);
            AssertAggregator((IntegerAverageAggregator) Aggregators.IntegerAvg(), AggregatorDataSerializerHook.IntAvg);
            AssertAggregator((LongAverageAggregator) Aggregators.LongAvg(), AggregatorDataSerializerHook.LongAvg);

            AssertAggregator((MaxAggregator<int>) Aggregators.Max<int>(), AggregatorDataSerializerHook.Max);
            AssertAggregator((MinAggregator<int>) Aggregators.Min<int>(), AggregatorDataSerializerHook.Min);
        }

        private void AssertAggregator<TResult>(AggregatorBase<TResult> aggregator, int classId)
        {
            var aggregatorType = aggregator.GetType();

            Assert.That(aggregator.FactoryId, Is.EqualTo(FactoryIds.AggregatorDsFactoryId));
            Assert.That(aggregator.ClassId, Is.EqualTo(classId));

            Assert.Throws<ArgumentException>(() => _ = Aggregators.Count(""));
            Assert.Throws<ArgumentException>(() => _ = Aggregators.Count(null));

            Assert.Throws<ArgumentNullException>(() => aggregator.WriteData(null));
            Assert.Throws<ArgumentNullException>(() => aggregator.ReadData(null));

            using var output = new ObjectDataOutput(1024, _serializationService, Endianness.BigEndian);
            aggregator.WriteData(output);

            using var input = new ObjectDataInput(output.Buffer, _serializationService, Endianness.BigEndian);
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

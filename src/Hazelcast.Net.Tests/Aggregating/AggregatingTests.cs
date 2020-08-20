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
            AssertAggregator((CountAggregator) Aggregator.Count("name"), AggregatorDataSerializerHook.Count);

            AssertAggregator((BigIntegerSumAggregator) Aggregator.BigIntegerSum("name"), AggregatorDataSerializerHook.BigIntSum);
            AssertAggregator((DoubleSumAggregator) Aggregator.DoubleSum("name"), AggregatorDataSerializerHook.DoubleSum);
            AssertAggregator((FixedSumAggregator) Aggregator.FixedPointSum("name"), AggregatorDataSerializerHook.FixedSum);
            AssertAggregator((LongSumAggregator) Aggregator.LongSum("name"), AggregatorDataSerializerHook.LongSum);
            AssertAggregator((FloatingPointSumAggregator) Aggregator.FloatingPointSum("name"), AggregatorDataSerializerHook.FloatingPointSum);
            AssertAggregator((IntegerSumAggregator) Aggregator.IntegerSum("name"), AggregatorDataSerializerHook.IntSum);

            AssertAggregator((NumberAverageAggregator) Aggregator.NumberAvg("name"), AggregatorDataSerializerHook.NumberAvg);
            AssertAggregator((DoubleAverageAggregator) Aggregator.DoubleAvg("name"), AggregatorDataSerializerHook.DoubleAvg);
            AssertAggregator((IntegerAverageAggregator) Aggregator.IntegerAvg("name"), AggregatorDataSerializerHook.IntAvg);
            AssertAggregator((LongAverageAggregator) Aggregator.LongAvg("name"), AggregatorDataSerializerHook.LongAvg);

            AssertAggregator((MaxAggregator<int>) Aggregator.Max<int>("name"), AggregatorDataSerializerHook.Max);
            AssertAggregator((MinAggregator<int>) Aggregator.Min<int>("name"), AggregatorDataSerializerHook.Min);

            AssertAggregator((CountAggregator) Aggregator.Count(), AggregatorDataSerializerHook.Count);

            AssertAggregator((BigIntegerSumAggregator) Aggregator.BigIntegerSum(), AggregatorDataSerializerHook.BigIntSum);
            AssertAggregator((DoubleSumAggregator) Aggregator.DoubleSum(), AggregatorDataSerializerHook.DoubleSum);
            AssertAggregator((FixedSumAggregator) Aggregator.FixedPointSum(), AggregatorDataSerializerHook.FixedSum);
            AssertAggregator((LongSumAggregator) Aggregator.LongSum(), AggregatorDataSerializerHook.LongSum);
            AssertAggregator((FloatingPointSumAggregator) Aggregator.FloatingPointSum(), AggregatorDataSerializerHook.FloatingPointSum);
            AssertAggregator((IntegerSumAggregator) Aggregator.IntegerSum(), AggregatorDataSerializerHook.IntSum);

            AssertAggregator((NumberAverageAggregator) Aggregator.NumberAvg(), AggregatorDataSerializerHook.NumberAvg);
            AssertAggregator((DoubleAverageAggregator) Aggregator.DoubleAvg(), AggregatorDataSerializerHook.DoubleAvg);
            AssertAggregator((IntegerAverageAggregator) Aggregator.IntegerAvg(), AggregatorDataSerializerHook.IntAvg);
            AssertAggregator((LongAverageAggregator) Aggregator.LongAvg(), AggregatorDataSerializerHook.LongAvg);

            AssertAggregator((MaxAggregator<int>) Aggregator.Max<int>(), AggregatorDataSerializerHook.Max);
            AssertAggregator((MinAggregator<int>) Aggregator.Min<int>(), AggregatorDataSerializerHook.Min);
        }

        private void AssertAggregator<TAggregator>(TAggregator aggregator, int classId)
            where TAggregator : IAggregator
        {
            Assert.That(aggregator.FactoryId, Is.EqualTo(FactoryIds.AggregatorDsFactoryId));
            Assert.That(aggregator.ClassId, Is.EqualTo(classId));

            Assert.Throws<ArgumentException>(() => _ = Aggregator.Count(""));
            Assert.Throws<ArgumentException>(() => _ = Aggregator.Count(null));

            Assert.Throws<ArgumentNullException>(() => aggregator.WriteData(null));
            Assert.Throws<ArgumentNullException>(() => aggregator.ReadData(null));

            using var output = new ByteArrayObjectDataOutput(1024, _serializationService, Endianness.Unspecified);
            aggregator.WriteData(output);

            using var input = new ByteArrayObjectDataInput(output.Buffer, _serializationService, Endianness.Unspecified);
            var a = (TAggregator) Activator.CreateInstance(typeof(TAggregator));
            a.ReadData(input);

            Assert.That(a.AttributePath, Is.EqualTo(aggregator.AttributePath));

            var data = _serializationService.ToData(aggregator);

            IAggregator x = null;
            if (typeof (TAggregator).IsGenericType)
            {
                // doh - cannot deserialize generic types?

                if (typeof (TAggregator).GetGenericTypeDefinition() == typeof(MaxAggregator<>))
                    x = _serializationService.ToObject<MaxAggregator<object>>(data);
                else if (typeof(TAggregator).GetGenericTypeDefinition() == typeof(MinAggregator<>))
                    x = _serializationService.ToObject<MinAggregator<object>>(data);
                else Assert.Fail("Unsupported generic aggregator type.");
            }
            else
            {
                x = _serializationService.ToObject<TAggregator>(data);
            }

            Assert.That(x.AttributePath, Is.EqualTo(aggregator.AttributePath));
        }
    }
}

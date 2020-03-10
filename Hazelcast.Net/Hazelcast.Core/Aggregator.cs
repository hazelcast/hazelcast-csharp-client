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

using System.Numerics;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Core
{
    /// <summary>
    /// Simple interface for marking aggregators. An aggregator implementain must support hazelcast serialization and
    /// have a counterpart on server side.
    /// </summary>
    /// <typeparam name="TResult">aggregated result type</typeparam>
    public class IAggregator<TResult>
    {
    }

    /// <summary>
    /// Base builtin aggregator
    /// </summary>
    public abstract class AbstractAggregator<R> : IAggregator<R>
    {
        protected string attributePath;

        public int GetFactoryId()
        {
            return FactoryIds.AggregatorDsFactoryId;
        }
    }

//    TODO: BigDecimalAverageAggregator
//    TODO: BigDecimalSumAggregator
//    TODO: BigIntegerAverageAggregator
//    TODO DistinctValuesAggregator returns java serializable

    /// <summary>
    /// An aggregator that calculates the sum of the input values.
    /// Does NOT accept null input values.
    /// Accepts only BigInteger input values.
    /// </summary>
    public sealed class BigIntegerSumAggregator : AbstractAggregator<BigInteger>, IIdentifiedDataSerializable
    {
        public BigIntegerSumAggregator()
        {
        }

        public BigIntegerSumAggregator(string attributePath)
        {
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
            //member side field not used on client
            input.ReadObject<BigInteger>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
            //member side field not used on client
            output.WriteObject(BigInteger.Zero);
        }

        public int GetId()
        {
            return AggregatorDataSerializerHook.BigIntSum;
        }
    }

    /// <summary>
    /// an aggregator that counts the input values. Accepts nulls as input values.
    /// </summary>
    public sealed class CountAggregator : AbstractAggregator<long>, IIdentifiedDataSerializable
    {
        public CountAggregator()
        {
        }

        public CountAggregator(string attributePath)
        {
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
            //member side field not used on client
            input.ReadLong();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
            //member side field not used on client
            output.WriteLong(0);
        }

        public int GetId()
        {
            return AggregatorDataSerializerHook.Count;
        }
    }

    /// <summary>
    /// an aggregator that calculates the average of the input values.
    /// Does NOT accept null input values.
    /// </summary>
    public sealed class DoubleAverageAggregator : AbstractAggregator<double>, IIdentifiedDataSerializable
    {
        public DoubleAverageAggregator()
        {
        }

        public DoubleAverageAggregator(string attributePath)
        {
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
            //member side field not used on client
            input.ReadDouble();
            input.ReadLong();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
            //member side field not used on client
            output.WriteDouble(0.0d);
            output.WriteLong(0);
        }

        public int GetId()
        {
            return AggregatorDataSerializerHook.DoubleAvg;
        }
    }

    /// <summary>
    /// An aggregator that calculates the sum of the input values extracted from the given attributePath.
    /// Does NOT accept null input values nor null extracted values.
    /// Accepts only double input values.
    /// </summary>
    public sealed class DoubleSumAggregator : AbstractAggregator<double>, IIdentifiedDataSerializable
    {
        public DoubleSumAggregator()
        {
        }

        public DoubleSumAggregator(string attributePath)
        {
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
            //member side field not used on client
            input.ReadDouble();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
            //member side field not used on client
            output.WriteDouble(0.0d);
        }

        public int GetId()
        {
            return AggregatorDataSerializerHook.DoubleSum;
        }
    }

    /// <summary>
    /// An aggregator that calculates the sum of the input values extracted from the given attributePath.
    /// Does NOT accept null input values nor null extracted values.
    /// Accepts float or double input values.
    /// </summary>
    public sealed class FixedSumAggregator : AbstractAggregator<long>, IIdentifiedDataSerializable
    {
        public FixedSumAggregator()
        {
        }

        public FixedSumAggregator(string attributePath)
        {
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
            //member side field not used on client
            input.ReadLong();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
            //member side field not used on client
            output.WriteLong(0);
        }

        public int GetId()
        {
            return AggregatorDataSerializerHook.FixedSum;
        }
    }

    /// <summary>
    /// an aggregator that calculates the sum of the input values.
    /// Does NOT accept null input values.
    /// Accepts float or double input values.
    /// </summary>
    public sealed class FloatingPointSumAggregator : AbstractAggregator<double>, IIdentifiedDataSerializable
    {
        public FloatingPointSumAggregator()
        {
        }

        public FloatingPointSumAggregator(string attributePath)
        {
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
            //member side field not used on client
            input.ReadDouble();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
            //member side field not used on client
            output.WriteDouble(0.0d);
        }

        public int GetId()
        {
            return AggregatorDataSerializerHook.FloatingPointSum;
        }
    }

    /// <summary>
    /// an aggregator that calculates the average of the input values extracted from the given attributePath.
    /// Does NOT accept null input values nor null extracted values.
    /// Accepts only int input values
    /// </summary>
    public sealed class IntegerAverageAggregator : AbstractAggregator<double>, IIdentifiedDataSerializable
    {
        public IntegerAverageAggregator()
        {
        }

        public IntegerAverageAggregator(string attributePath)
        {
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
            //member side field not used on client
            input.ReadLong();
            input.ReadLong();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
            //member side field not used on client
            output.WriteLong(0);
            output.WriteLong(0);
        }

        public int GetId()
        {
            return AggregatorDataSerializerHook.IntAvg;
        }
    }

    /// <summary>
    /// an aggregator that calculates the sum of the input values.
    /// Does NOT accept null input values.
    /// Accepts only int input values.
    /// </summary>
    public sealed class IntegerSumAggregator : AbstractAggregator<long>, IIdentifiedDataSerializable
    {
        public IntegerSumAggregator()
        {
        }

        public IntegerSumAggregator(string attributePath)
        {
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
            //member side field not used on client
            input.ReadLong();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
            //member side field not used on client
            output.WriteLong(0);
        }

        public int GetId()
        {
            return AggregatorDataSerializerHook.IntSum;
        }
    }

    /// <summary>
    /// an aggregator that calculates the average of the input values.
    /// Does NOT accept null input values.
    /// Accepts only long input values
    /// </summary>
    public sealed class LongAverageAggregator : AbstractAggregator<double>, IIdentifiedDataSerializable
    {
        public LongAverageAggregator()
        {
        }

        public LongAverageAggregator(string attributePath)
        {
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
            //member side field not used on client
            input.ReadLong();
            input.ReadLong();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
            //member side field not used on client
            output.WriteLong(0);
            output.WriteLong(0);
        }

        public int GetId()
        {
            return AggregatorDataSerializerHook.LongAvg;
        }
    }

    /// <summary>
    /// an aggregator that calculates the sum of the input values.
    /// Does NOT accept null input values.
    /// Accepts only long input values.
    /// </summary>
    public sealed class LongSumAggregator : AbstractAggregator<long>, IIdentifiedDataSerializable
    {
        public LongSumAggregator()
        {
        }

        public LongSumAggregator(string attributePath)
        {
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
            //member side field not used on client
            input.ReadLong();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
            //member side field not used on client
            output.WriteLong(0);
        }

        public int GetId()
        {
            return AggregatorDataSerializerHook.LongSum;
        }
    }

    /// <summary>
    /// an aggregator that calculates the max of the input values.
    /// Accepts null input values
    /// </summary>
    public sealed class MaxAggregator<TResult> : AbstractAggregator<TResult>, IIdentifiedDataSerializable
    {
        public MaxAggregator()
        {
        }

        public MaxAggregator(string attributePath)
        {
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
            //member side field not used on client
            input.ReadObject<object>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
            //member side field not used on client
            output.WriteObject(null);
        }

        public int GetId()
        {
            return AggregatorDataSerializerHook.Max;
        }
    }

    /// <summary>
    /// an aggregator that calculates the min of the input values.
    /// Accepts null input values
    /// </summary>
    public sealed class MinAggregator<TResult> : AbstractAggregator<TResult>, IIdentifiedDataSerializable
    {
        public MinAggregator()
        {
        }

        public MinAggregator(string attributePath)
        {
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
            //member side field not used on client
            input.ReadObject<object>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
            //member side field not used on client
            output.WriteObject(null);
        }

        public int GetId()
        {
            return AggregatorDataSerializerHook.Min;
        }
    }

    /// <summary>
    /// an aggregator that calculates the average of the input values.
    /// Does NOT accept null input values nor null extracted values.
    /// Accepts float or double input values.
    /// </summary>
    public sealed class NumberAverageAggregator : AbstractAggregator<double>, IIdentifiedDataSerializable
    {
        public NumberAverageAggregator()
        {
        }

        public NumberAverageAggregator(string attributePath)
        {
            this.attributePath = attributePath;
        }

        public void ReadData(IObjectDataInput input)
        {
            attributePath = input.ReadUTF();
            //member side field not used on client
            input.ReadDouble();
            input.ReadLong();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(attributePath);
            //member side field not used on client
            output.WriteDouble(0.0d);
            output.WriteLong(0);
        }

        public int GetId()
        {
            return AggregatorDataSerializerHook.NumberAvg;
        }
    }
}
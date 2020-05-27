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
using Hazelcast.Exceptions;
using Hazelcast.Serialization;

namespace Hazelcast.Aggregators
{
    /// <summary>
    /// Provides a base class for all <see cref="IAggregator{TResult}"/> implementations.
    /// </summary>
    public abstract class AggregatorBase<TResult> : IAggregator<TResult>
    {
        private string _attributePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregatorBase{TResult}"/> class.
        /// </summary>
        protected AggregatorBase()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregatorBase{TResult}"/> class.
        /// </summary>
        /// <param name="attributePath">The attribute path.</param>
        protected AggregatorBase(string attributePath)
        {
            if (string.IsNullOrWhiteSpace(attributePath)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(attributePath));
            _attributePath = attributePath;
        }

        /// <inheritdoc />
        public void ReadData(IObjectDataInput input)
        {
            _attributePath = input.ReadUtf();
            ReadAggregatorData(input);
        }

        /// <summary>
        /// Deserializes the aggregator by reading from an <see cref="IObjectDataInput"/>.
        /// </summary>
        /// <param name="input">The input serialized data.</param>
        /// <remarks>The attribute path has already been deserialized.</remarks>
        protected abstract void ReadAggregatorData(IObjectDataInput input);

        /// <inheritdoc />
        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUtf(_attributePath);
            WriteAggregatorData(output);
        }

        /// <summary>
        /// Serializes the object by writing to an <see cref="IObjectDataOutput"/>.
        /// </summary>
        /// <param name="output">The output serialized data.</param>
        /// <remarks>The attribute path has already been serialized.</remarks>
        protected abstract void WriteAggregatorData(IObjectDataOutput output);

        /// <inheritdoc />
        public int GetFactoryId() => FactoryIds.AggregatorDsFactoryId;

        /// <inheritdoc />
        public abstract int GetId();
    }

    // TODO: implement BigDecimalAverageAggregator
    // TODO: implement BigDecimalSumAggregator
    // TODO: implement BigIntegerAverageAggregator
    // TODO: implement DistinctValuesAggregator (returns java serializable)
}
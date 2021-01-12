// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Aggregation
{
    /// <summary>
    /// Represents an aggregator that sums the input values.
    /// </summary>
    /// <remarks>
    /// <para>Null input values not accepted.</para>
    /// </remarks>
    internal sealed class FloatingPointSumAggregator : AggregatorBase<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FloatingPointSumAggregator"/> class.
        /// </summary>
        public FloatingPointSumAggregator()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatingPointSumAggregator"/> class.
        /// </summary>
        /// <param name="attributePath">The attribute path.</param>
        public FloatingPointSumAggregator(string attributePath)
            : base(attributePath)
        { }

        /// <inheritdoc />
        protected override void ReadAggregatorData(IObjectDataInput input)
        {
            input.ReadDouble(); // member side field not used on client
        }

        /// <inheritdoc />
        protected override void WriteAggregatorData(IObjectDataOutput output)
        {
            output.WriteDouble(0.0d); // member side field not used on client
        }

        /// <inheritdoc />
        public override int ClassId => AggregatorDataSerializerHook.FloatingPointSum;
    }
}

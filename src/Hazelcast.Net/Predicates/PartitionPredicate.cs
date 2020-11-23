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

namespace Hazelcast.Predicates
{
    /// <summary>
    /// A builtin predicate that restricts the execution of a target Predicate to a single Partition.
    /// </summary>
    /// <remarks>
    /// This can help to speed up query execution since only a single instead of all partitions needs to be queried.
    ///
    /// This predicate can only be used as an outer predicate
    /// </remarks>
    internal class PartitionPredicate : IPredicate, IIdentifiedDataSerializable
    {
        public PartitionPredicate()
        { }

        public PartitionPredicate(object partitionKey, IPredicate predicate)
        {
            Target = predicate;
            PartitionKey = partitionKey;
        }

        /// <summary>
        /// Returns the predicate that will run on target partition
        /// </summary>
        /// <returns></returns>
        public IPredicate Target { get; private set; }

        /// <summary>
        /// Returns the partition key that determines the partition the target Predicate is going to execute on
        /// </summary>
        /// <returns>the partition key</returns>
        public object PartitionKey { get; private set; }

        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            PartitionKey = input.ReadObject<object>();
            Target = input.ReadObject<IPredicate>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            output.WriteObject(PartitionKey);
            output.WriteObject(Target);
        }

        public int FactoryId => FactoryIds.PredicateFactoryId;

        public int ClassId => PredicateDataSerializerHook.PartitionPredicate;
    }
}

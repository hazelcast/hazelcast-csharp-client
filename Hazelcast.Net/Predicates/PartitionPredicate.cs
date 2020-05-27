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
    public class PartitionPredicate : IPredicate
    {
        private IPredicate predicate;
        private object partitionKey;

        public PartitionPredicate()
        {
        }

        public PartitionPredicate(object partitionKey, IPredicate predicate)
        {
            this.predicate = predicate;
            this.partitionKey = partitionKey;
        }

        /// <summary>
        /// Returns the predicate that will run on target partition
        /// </summary>
        /// <returns></returns>
        public IPredicate GetTarget()
        {
            return predicate;
        }

        /// <summary>
        /// Returns the partition key that determines the partition the target Predicate is going to execute on
        /// </summary>
        /// <returns>the partition key</returns>
        public object GetPartitionKey()
        {
            return partitionKey;
        }

        public void ReadData(IObjectDataInput input)
        {
            partitionKey = input.ReadObject<object>();
            predicate = input.ReadObject<IPredicate>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteObject(partitionKey);
            output.WriteObject(predicate);
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.PartitionPredicate;
        }

    }
}
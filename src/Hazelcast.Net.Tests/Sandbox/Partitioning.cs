using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Serialization;
using NUnit.Framework;

namespace Hazelcast.Tests.Sandbox
{
    [TestFixture]
    public class Partitioning
    {
        public class Strategy1 : IPartitioningStrategy
        {
            public object GetPartitionKey(object o)
            {
                // for some obvious-enough cases, directly return the hash
                // this is probably faster than the default implementation
                // though it still allocates a PartitionHashData, don't see
                // how to work around it - apart from modifying the logic
                // to accept an int partition key as a direct hash?
                //
                // see note in SerializationService.CalculatePartitionHash

                return o switch
                {
                    int i => i,
                    string s => s.GetHashCode(),
                    // etc?
                    _ => null
                };
            }
        }
    }
}

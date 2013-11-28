using System;

namespace Hazelcast.Core
{
    /// <summary>Thrown when IHazelcastInstance is not active during an invocation.</summary>
    /// <remarks>Thrown when IHazelcastInstance is not active during an invocation.</remarks>
    [Serializable]
    public class HazelcastInstanceNotActiveException : InvalidOperationException
    {
        public HazelcastInstanceNotActiveException() : base("Hazelcast instance is not active!")
        {
        }
    }
}
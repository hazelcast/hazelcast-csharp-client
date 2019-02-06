// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core
{
    /// <summary>
    /// Base Hazelcast exception.
    /// </summary>
    [Serializable]
    public class HazelcastException : SystemException
    {
        public HazelcastException()
        {
        }

        public HazelcastException(string message) : base(message)
        {
        }

        public HazelcastException(string message, Exception cause) : base(message, cause)
        {
        }

        public HazelcastException(Exception cause) : base(cause.Message)
        {
        }
    }

    [Serializable]
    public class QueryException : HazelcastException
    {
        public QueryException()
        {
        }

        public QueryException(string message) : base(message)
        {
        }

        public QueryException(string message, Exception cause) : base(message, cause)
        {
        }

        public QueryException(Exception cause) : base(cause.Message)
        {
        }
    }

    /// <summary>
    /// A
    /// <see cref="Hazelcast.Core.HazelcastException"/>
    /// that indicates that a
    /// <see cref="Hazelcast.Core.IDistributedObject"/>
    /// access was attempted, but the object is destroyed.
    /// </summary>
    [Serializable]
    public class DistributedObjectDestroyedException : HazelcastException
    {
        public DistributedObjectDestroyedException()
        {
        }

        public DistributedObjectDestroyedException(string message) : base(message)
        {
        }

        public DistributedObjectDestroyedException(string message, Exception cause) : base(message, cause)
        {
        }

        public DistributedObjectDestroyedException(Exception cause) : base(cause.Message)
        {
        }
    }

    /// <summary>
    /// An exception that is thrown when accessing an item in the <see cref="IRingbuffer{E}">IRingbuffer</see> using a 
    /// sequence that is smaller than the current head sequence. This means that the and old item is read, 
    /// but it isn't available anymore in the ringbuffer.
    /// </summary>
    [Serializable]
    public class StaleSequenceException : HazelcastException
    {
        public StaleSequenceException()
        {
        }

        public StaleSequenceException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// An exception that is thrown when the session guarantees have been lost
    /// </summary>
    [Serializable]
    public class ConsistencyLostException : HazelcastException
    {
        public ConsistencyLostException()
        {
        }

        public ConsistencyLostException(string message) : base(message)
        {
        }
    }

    /// <summary>Thrown when invoke operations on a CRDT failed because the cluster does not contain any data members.</summary>
    /// <remarks>Thrown when invoke operations on a CRDT failed because the cluster does not contain any data members.</remarks>
    [Serializable]
    public class NoDataMemberInClusterException : HazelcastException
    {
        public NoDataMemberInClusterException()
        {
        }
        public NoDataMemberInClusterException(string message) : base(message)
        {
        }
    }
}
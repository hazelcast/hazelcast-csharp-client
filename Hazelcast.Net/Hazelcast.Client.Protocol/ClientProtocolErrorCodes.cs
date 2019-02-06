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

namespace Hazelcast.Client.Protocol
{
    /// <summary>Each exception that are defined in client protocol have unique identifier which are error code.</summary>
    /// <remarks>
    ///     Each exception that are defined in client protocol have unique identifier which are error code.
    ///     All error codes defined in protocol are listed in this class.
    /// </remarks>
    internal enum ClientProtocolErrorCodes
    {
        Undefined = 0,
        ArrayIndexOutOfBounds = 1,
        ArrayStore = 2,
        Authentication = 3,
        Cache = 4,
        CacheLoader = 5,
        CacheNotExists = 6,
        CacheWriter = 7,
        CallerNotMember = 8,
        Cancellation = 9,
        ClassCast = 10,
        ClassNotFound = 11,
        ConcurrentModification = 12,
        ConfigMismatch = 13,
        Configuration = 14,
        DistributedObjectDestroyed = 15,
        DuplicateInstanceName = 16,
        Eof = 17,
        EntryProcessor = 18,
        Execution = 19,
        Hazelcast = 20,
        HazelcastInstanceNotActive = 21,
        HazelcastOverload = 22,
        HazelcastSerialization = 23,
        Io = 24,
        IllegalArgument = 25,
        IllegalAccessException = 26,
        IllegalAccessError = 27,
        IllegalMonitorState = 28,
        IllegalState = 29,
        IllegalThreadState = 30,
        IndexOutOfBounds = 31,
        Interrupted = 32,
        InvalidAddress = 33,
        InvalidConfiguration = 34,
        MemberLeft = 35,
        NegativeArraySize = 36,
        NoSuchElement = 37,
        NotSerializable = 38,
        NullPointer = 39,
        OperationTimeout = 40,
        PartitionMigrating = 41,
        Query = 42,
        QueryResultSizeExceeded = 43,
        Quorum = 44,
        ReachedMaxSize = 45,
        RejectedExecution = 46,
        RemoteMapReduce = 47,
        ResponseAlreadySent = 48,
        RetryableHazelcast = 49,
        RetryableIo = 50,
        Runtime = 51,
        Security = 52,
        Socket = 53,
        StaleSequence = 54,
        TargetDisconnected = 55,
        TargetNotMember = 56,
        Timeout = 57,
        TopicOverload = 58,
        TopologyChanged = 59,
        Transaction = 60,
        TransactionNotActive = 61,
        TransactionTimedOut = 62,
        UriSyntax = 63,
        UtfDataFormat = 64,
        UnsupportedOperation = 65,
        WrongTarget = 66,
        Xa = 67,
        AccessControl = 68,
        Login = 69,
        UnsupportedCallback = 70,
        NoDataMemeber = 71,
        ReplicatedMapCantBeCreated = 72,
        MaxMessageSizeExceeded = 73,
        WANReplicationQueueFull = 74,
        AssertionError = 75,
        OutOfMemory = 76,
        StackOverflowError = 77,
        NativeOutOfMemoryError = 78,
        ServiceNotFound = 79,
        StaleTaskId = 80,
        DuplicateTask = 81,
        StaleTask = 82,
        LocalMemberReset = 83,
        IndeterminateOperationState = 84,
        FlakeIdNodeIdOutOfRangeException = 85,
        TargetNotReplicaException = 86,
        MutationDisallowedException = 87,
        ConsistencyLostException = 88
    }
}
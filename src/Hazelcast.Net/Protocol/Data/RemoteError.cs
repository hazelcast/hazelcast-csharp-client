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

namespace Hazelcast.Protocol.Data
{
    /// <summary>
    /// Defines the error codes that can be returned by a remote server.
    /// </summary>
    public enum RemoteError
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
        DistributedObjectDestroyed = 14,
        Eof = 15,
        EntryProcessor = 16,
        Execution = 17,
        Hazelcast = 18,
        HazelcastInstanceNotActive = 19,
        HazelcastOverload = 20,
        HazelcastSerialization = 21,
        IO = 22,
        IllegalArgument = 23,
        IllegalAccessException = 24,
        IllegalAccessError = 25,
        IllegalMonitorState = 26,
        IllegalState = 27,
        IllegalThreadState = 28,
        IndexOutOfBounds = 29,
        Interrupted = 30,
        InvalidAddress = 31,
        InvalidConfiguration = 32,
        MemberLeft = 33,
        NegativeArraySize = 34,
        NoSuchElement = 35,
        NotSerializable = 36,
        NullPointer = 37,
        OperationTimeout = 38,
        PartitionMigrating = 39,
        Query = 40,
        QueryResultSizeExceeded = 41,
        SplitBrainProtection = 42,
        ReachedMaxSize = 43,
        RejectedExecution = 44,
        ResponseAlreadySent = 45,
        RetryableHazelcast = 46,
        RetryableIO = 47,
        Runtime = 48,
        Security = 49,
        Socket = 50,
        StaleSequence = 51,
        TargetDisconnected = 52,
        TargetNotMember = 53,
        Timeout = 54,
        TopicOverload = 55,
        Transaction = 56,
        TransactionNotActive = 57,
        TransactionTimedOut = 58,
        UriSyntax = 59,
        UTFDataFormat = 60,
        UnsupportedOperation = 61,
        WrongTarget = 62,
        Xa = 63,
        AccessControl = 64,
        Login = 65,
        UnsupportedCallback = 66,
        NoDataMember = 67,
        ReplicatedMapCantBeCreated = 68,
        MaxMessageSizeExceeded = 69,
        WanReplicationQueueFull = 70,
        AssertionError = 71,
        OutOfMemoryError = 72,
        StackOverflowError = 73,
        NativeOutOfMemoryError = 74,
        ServiceNotFound = 75,
        StaleTaskId = 76,
        DuplicateTask = 77,
        StaleTask = 78,
        LocalMemberReset = 79,
        IndeterminateOperationState = 80,
        FlakeIdNodeIdOutOfRangeException = 81,
        TargetNotReplicaException = 82,
        MutationDisallowedException = 83,
        ConsistencyLostException = 84,
        SessionExpiredException = 85,
        WaitKeyCancelledException = 86,
        LockAcquireLimitReachedException = 87,
        LockOwnershipLostException = 88,
        CpGroupDestroyedException = 89,
        CannotReplicateException = 90,
        LeaderDemotedException = 91,
        StaleAppendRequestException = 92,
        NotLeaderException = 93,
        VersionMismatchException = 94,
        NoSuchMethodError = 95,
        NoSuchMethodException = 96,
        NoSuchFieldError = 97,
        NoSuchFieldException = 98,
        NoClassDefinitionFound = 99,

        // These exception codes are reserved to by used by hazelcast-jet project
        JetExceptionsRangeStart = 500,
        JetExceptionsRangeEnd = 600,

        // These codes onwards are reserved to be used by the end-user
        UserExceptionsRangeStart = 1000,
    }
}

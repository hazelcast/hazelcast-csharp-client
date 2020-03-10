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

namespace Hazelcast.Client.Protocol
{
    /// <summary>Each exception that are defined in client protocol have unique identifier which are error code.</summary>
    /// <remarks>
    ///     Each exception that are defined in client protocol have unique identifier which are error code.
    ///     All error codes defined in protocol are listed in this class.
    /// </remarks>
    internal static class ClientProtocolErrorCodes
    {
        public const int Undefined = 0;
        public const int ArrayIndexOutOfBounds = 1;
        public const int ArrayStore = 2;
        public const int Authentication = 3;
        public const int Cache = 4;
        public const int CacheLoader = 5;
        public const int CacheNotExists = 6;
        public const int CacheWriter = 7;
        public const int CallerNotMember = 8;
        public const int Cancellation = 9;
        public const int ClassCast = 10;
        public const int ClassNotFound = 11;
        public const int ConcurrentModification = 12;
        public const int ConfigMismatch = 13;
        public const int DistributedObjectDestroyed = 14;
        public const int Eof = 15;
        public const int EntryProcessor = 16;
        public const int Execution = 17;
        public const int Hazelcast = 18;
        public const int HazelcastInstanceNotActive = 19;
        public const int HazelcastOverload = 20;
        public const int HazelcastSerialization = 21;
        public const int IO = 22;
        public const int IllegalArgument = 23;
        public const int IllegalAccessException = 24;
        public const int IllegalAccessError = 25;
        public const int IllegalMonitorState = 26;
        public const int IllegalState = 27;
        public const int IllegalThreadState = 28;
        public const int IndexOutOfBounds = 29;
        public const int Interrupted = 30;
        public const int InvalidAddress = 31;
        public const int InvalidConfiguration = 32;
        public const int MemberLeft = 33;
        public const int NegativeArraySize = 34;
        public const int NoSuchElement = 35;
        public const int NotSerializable = 36;
        public const int NullPointer = 37;
        public const int OperationTimeout = 38;
        public const int PartitionMigrating = 39;
        public const int Query = 40;
        public const int QueryResultSizeExceeded = 41;
        public const int SplitBrainProtection = 42;
        public const int ReachedMaxSize = 43;
        public const int RejectedExecution = 44;
        public const int ResponseAlreadySent = 45;
        public const int RetryableHazelcast = 46;
        public const int RetryableIO = 47;
        public const int Runtime = 48;
        public const int Security = 49;
        public const int Socket = 50;
        public const int StaleSequence = 51;
        public const int TargetDisconnected = 52;
        public const int TargetNotMember = 53;
        public const int Timeout = 54;
        public const int TopicOverload = 55;
        public const int Transaction = 56;
        public const int TransactionNotActive = 57;
        public const int TransactionTimedOut = 58;
        public const int UriSyntax = 59;
        public const int UTFDataFormat = 60;
        public const int UnsupportedOperation = 61;
        public const int WrongTarget = 62;
        public const int Xa = 63;
        public const int AccessControl = 64;
        public const int Login = 65;
        public const int UnsupportedCallback = 66;
        public const int NoDataMember = 67;
        public const int ReplicatedMapCantBeCreated = 68;
        public const int MaxMessageSizeExceeded = 69;
        public const int WanReplicationQueueFull = 70;
        public const int AssertionError = 71;
        public const int OutOfMemoryError = 72;
        public const int StackOverflowError = 73;
        public const int NativeOutOfMemoryError = 74;
        public const int ServiceNotFound = 75;
        public const int StaleTaskId = 76;
        public const int DuplicateTask = 77;
        public const int StaleTask = 78;
        public const int LocalMemberReset = 79;
        public const int IndeterminateOperationState = 80;
        public const int FlakeIdNodeIdOutOfRangeException = 81;
        public const int TargetNotReplicaException = 82;
        public const int MutationDisallowedException = 83;
        public const int ConsistencyLostException = 84;
        public const int SessionExpiredException = 85;
        public const int WaitKeyCancelledException = 86;
        public const int LockAcquireLimitReachedException = 87;
        public const int LockOwnershipLostException = 88;
        public const int CpGroupDestroyedException = 89;
        public const int CannotReplicateException = 90;
        public const int LeaderDemotedException = 91;
        public const int StaleAppendRequestException = 92;
        public const int NotLeaderException = 93;
        public const int VersionMismatchException = 94;

        // These exception codes are reserved to by used by hazelcast-jet project
        public const int JetExceptionsRangeStart = 500;
        public const int JetExceptionsRangeEnd = 600;

        // These codes onwards are reserved to be used by the end-user
        public const int UserExceptionsRangeStart = 1000;
    }
}
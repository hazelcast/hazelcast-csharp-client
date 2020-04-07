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

namespace Hazelcast.Messaging.FrameFields
{
    /// <summary>
    /// Defines constants representing the offset of frame fields.
    /// </summary>
    public static class Offset
    {
        /// <summary>
        /// Gets the offset of the length field.
        /// </summary>
        public const int Length = 0;

        /// <summary>
        /// Gets the offset of the flags field.
        /// </summary>
        public const int Flags = SizeOf.Length;

        /// <summary>
        /// Gets the offset of the bytes array.
        /// </summary>
        public const int Bytes = Flags + SizeOf.Flags;

        /// <summary>
        /// Gets the offset of the message type field.
        /// </summary>
        public const int MessageType = 0;

        /// <summary>
        /// Gets the offset of the fragment id field.
        /// </summary>
        public const int FragmentId = 0;

        /// <summary>
        /// Gets the offset of the correlation id field.
        /// </summary>
        public const int CorrelationId = SizeOf.MessageType;

        /// <summary>
        /// Gets the offset of the partition id field.
        /// </summary>
        public const int PartitionId = CorrelationId + SizeOf.CorrelationId;

        /// <summary>
        /// Gets the offset of the response backup acknowledgement field.
        /// </summary>
        public const int ResponseBackupAcks = CorrelationId + SizeOf.CorrelationId;
    }
}
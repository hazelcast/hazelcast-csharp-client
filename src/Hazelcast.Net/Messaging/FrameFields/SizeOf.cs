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
    /// Defines constants representing the size of frame fields.
    /// </summary>
    internal static class SizeOf
    {
        /// <summary>
        /// Gets the size of the length field.
        /// </summary>
        public const int Length = sizeof(int);

        /// <summary>
        /// Gets the size of the flags field.
        /// </summary>
        public const int Flags = sizeof(ushort);

        /// <summary>
        /// Gets the size of the length+flags fields.
        /// </summary>
        public const int LengthAndFlags = Length + Flags;

        /// <summary>
        /// Gets the size of the message type field.
        /// </summary>
        public const int MessageType = sizeof(int);

        /// <summary>
        /// Gets the size of the fragment id field.
        /// </summary>
        public const int FragmentId = sizeof(long);

        /// <summary>
        /// Gets the size of the correlation id field.
        /// </summary>
        public const int CorrelationId = sizeof(long);

        /// <summary>
        /// Gets the size of the partition id field.
        /// </summary>
        public const int PartitionId = sizeof(int);

        /// <summary>
        /// Gets the size of the response backup acknowledgement field.
        /// </summary>
        public const int ResponseBackupAcks = sizeof(byte);
    }
}

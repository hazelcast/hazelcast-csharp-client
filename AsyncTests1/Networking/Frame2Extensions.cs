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

namespace AsyncTests1.Networking
{
    public static class Frame2Extensions
    {
        private static class SizeOf
        {
            public const int MessageType = sizeof(int);
            public const int CorrelationId = sizeof(long);
            public const int PartitionId = sizeof(int);
            public const int ResponseBackupAcks = sizeof(int); // ???
        }

        private static class Offset
        {
            // structure is
            // type (int) | correlationId (long) | partitionId (int)
            //                                   | responseBackupAcks (???)
            // fragmentId (???)

            public const int MessageType = 0;
            public const int FragmentId = 0;
            public const int CorrelationId = SizeOf.MessageType;
            public const int PartitionId = CorrelationId + SizeOf.CorrelationId;
            public const int ResponseBackupAcks = CorrelationId + SizeOf.CorrelationId;
        }

        public static int GetMessageType(this Frame2 frame)
            => frame.Bytes.ReadInt32(Offset.MessageType);

        public static void SetMessageType(this Frame2 frame, int type)
            => frame.Bytes.WriteInt32(Offset.MessageType, type);

        public static long GetCorrelationId(this Frame2 frame)
            => frame.Bytes.ReadInt64(Offset.CorrelationId);

        public static void SetCorrelationId(this Frame2 frame, long correlationId)
            => frame.Bytes.WriteInt64(Offset.CorrelationId, correlationId);

        public static int GetPartitionId(this Frame2 frame)
            => frame.Bytes.ReadInt32(Offset.PartitionId);

        public static void SetPartitionId(this Frame2 frame, int partionId)
            => frame.Bytes.WriteInt32(Offset.PartitionId, partionId);
    }
}
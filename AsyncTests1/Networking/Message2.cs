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

using System;

namespace AsyncTests1.Networking
{
    public class Message2
    {
        public Message2(Frame2 firstFrame = null)
        {
            FirstFrame = LastFrame = firstFrame;
            if (firstFrame != null) firstFrame.Next = null;
        }

        // ???
        public bool IsRetryable { get; set; }
        public string OperationName { get; set; }

        public Frame2 FirstFrame { get; private set; }

        public Frame2 LastFrame { get; private set; }

        // TODO test it returns Default when no frames
        public MessageFlags2 Flags => (MessageFlags2) FirstFrame?.Flags;

        public Message2 Append(Frame2 frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));

            frame.Next = null;

            if (FirstFrame == null)
            {
                FirstFrame = LastFrame = frame;
            }
            else
            {
                LastFrame.Next = frame;
                LastFrame = frame;
            }

            return this;
        }

        public Message2 Append(Message2 fragment)
        {
            if (fragment == null) throw new ArgumentNullException(nameof(fragment));

            // skip the first frame of the fragment,
            // the first frame is just an empty Begin frame marking the segment

            LastFrame.Next = fragment.FirstFrame.Next;
            LastFrame = fragment.LastFrame;

            return this;
        }

        public bool IsBackupAware => Flags.Has(MessageFlags2.BackupAware);

        public bool IsBackupEvent => Flags.Has(MessageFlags2.BackupEvent);

        public bool IsEvent => Flags.Has(MessageFlags2.Event);

        public int MessageType
        {
            get => FirstFrame.GetMessageType();
            set => FirstFrame.SetMessageType(value);
        }

        public long CorrelationId
        {
            get => FirstFrame.GetCorrelationId();
            set => FirstFrame.SetCorrelationId(value);
        }

        public int PartitionId
        {
            get => FirstFrame.GetPartitionId();
            set => FirstFrame.SetPartitionId(value);
        }

        public bool IsException => MessageType == 0; // values, consts ???
    }
}
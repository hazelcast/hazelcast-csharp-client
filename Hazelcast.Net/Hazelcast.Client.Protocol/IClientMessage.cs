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

using System.Collections.Generic;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Protocol
{
    /// <summary>
    ///     Client Message is the carrier framed data as defined below.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         Client Message is the carrier framed data as defined below.
    ///     </p>
    ///     <p>
    ///         Any request parameter, response or event data will be carried in
    ///         the payload.
    ///     </p>
    ///     <p />
    ///     <pre>
    ///         0                   1                   2                   3
    ///         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    ///         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///         |R|                      Frame Length                           |
    ///         +-------------+---------------+---------------------------------+
    ///         |  Version    |B|E|  Flags    |               Type              |
    ///         +-------------+---------------+---------------------------------+
    ///         |                                                               |
    ///         +                       CorrelationId                           +
    ///         |                                                               |
    ///         +---------------------------------------------------------------+
    ///         |                        PartitionId                            |
    ///         +-----------------------------+---------------------------------+
    ///         |        Data Offset          |                                 |
    ///         +-----------------------------+                                 |
    ///         |                      Message Payload Data                    ...
    ///         |                                                              ...
    ///     </pre>
    /// </remarks>
    public interface IClientMessage
    {
        /// <summary>Sets the flags field value.</summary>
        /// <param name="flags">The value to set in the flags field.</param>
        /// <returns>The ClientMessage with the new flags field value.</returns>
        IClientMessage AddFlag(short flags);

        /// <param name="listenerEventFlag">Check this flag to see if it is set.</param>
        /// <returns>true if the given flag is set, false otherwise.</returns>
        bool IsFlagSet(short listenerEventFlag);

        /// <summary>Returns the flags field value.</summary>
        /// <returns>The flags field value.</returns>
        short GetFlags();

        /// <summary>Returns the message type field.</summary>
        /// <returns>The message type field value.</returns>
        int GetMessageType();

        /// <summary>Returns the correlation ID field.</summary>
        /// <returns>The correlation ID field.</returns>
        long GetCorrelationId();

        /// <summary>Sets the correlation ID field.</summary>
        /// <param name="correlationId">The value to set in the correlation ID field.</param>
        /// <returns>The ClientMessage with the new correlation ID field value.</returns>
        IClientMessage SetCorrelationId(long correlationId);

        /// <summary>Returns the partition ID field.</summary>
        /// <returns>The partition ID field.</returns>
        int GetPartitionId();

        /// <summary>Sets the partition ID field.</summary>
        /// <param name="partitionId">The value to set in the partitions ID field.</param>
        /// <returns>The ClientMessage with the new partitions ID field value.</returns>
        IClientMessage SetPartitionId(int partitionId);

        /// <summary>Returns the frame length field.</summary>
        /// <returns>The frame length field.</returns>
        int GetFrameLength();

        /// <summary>Returns the version field value.</summary>
        /// <returns>The version field value.</returns>
        short GetVersion();
        
        /// <summary>Checks the frame size and total data size to validate the message size.</summary>
        /// <returns>true if the message is constructed.</returns>
        bool IsComplete();

        bool IsRetryable();
        bool GetBoolean();
        byte GetByte();
        IData GetData();
        short GetShort();
        int GetInt();
        long GetLong();
        KeyValuePair<IData, IData> GetMapEntry();
        string GetStringUtf8();
    }
}
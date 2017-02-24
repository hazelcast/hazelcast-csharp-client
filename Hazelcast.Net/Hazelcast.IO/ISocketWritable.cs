// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Client.Connection;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    /// <summary>
    /// Represents something that can be written to a
    /// <see cref="ClientConnection">Clientconnection</see>.
    /// </summary>
    internal interface ISocketWritable
    {
        /// <summary>Checks if this SocketWritable is urgent.</summary>
        /// <remarks>
        /// Checks if this SocketWritable is urgent.
        /// SocketWritable that are urgent, have priority above regular SocketWritable. This is useful to implement
        /// System Operations so that they can be send faster than regular operations; especially when the system is
        /// under load you want these operations have precedence.
        /// </remarks>
        /// <returns>true if urgent, false otherwise.</returns>
        bool IsUrgent();

        /// <summary>Asks the SocketWritable to write its content to the destination ByteBuffer.
        /// 	</summary>
        /// <remarks>Asks the SocketWritable to write its content to the destination ByteBuffer.
        /// 	</remarks>
        /// <param name="destination">the ByteBuffer to write to.</param>
        /// <returns>todo: unclear what return value means.</returns>
        bool WriteTo(ByteBuffer destination);
    }
}
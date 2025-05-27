// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Clustering;

namespace Hazelcast.Events
{
    /// <summary>
    /// Represents event data for the connection closed event.
    /// </summary>
    internal class ConnectionClosedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionClosedEventArgs"/> class.
        /// </summary>
        /// <param name="connection">The connection that was closed.</param>
        public ConnectionClosedEventArgs(MemberConnection connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// Gets the connection that was closed.
        /// </summary>
        public MemberConnection Connection { get; }
    }
}

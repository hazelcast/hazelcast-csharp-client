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

namespace Hazelcast.Events
{
    /// <summary>
    /// Represents event data for connection lifecycle events.
    /// </summary>
    internal class ConnectionLifecycleEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionLifecycleEventArgs"/> class.
        /// </summary>
        ///// <param name="client">The client.</param>
        public ConnectionLifecycleEventArgs(/*Client client*/)
        {
            //Client = client;
        }

        // TODO: shall the event be internal entirely? or else?

        ///// <summary>
        ///// Gets the client.
        ///// </summary>
        //public Client Client { get; }
    }
}

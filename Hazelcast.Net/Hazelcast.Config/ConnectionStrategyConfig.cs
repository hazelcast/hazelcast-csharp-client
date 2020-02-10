// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Config
{
    ///<summary>Reconnect options.</summary>
    public enum ReconnectMode
    {
        ///<summary>Prevent reconnect to cluster after a disconnect</summary>
        OFF,

        ///<summary>Reconnect to cluster by blocking invocations</summary>
        ON,

        ///<summary>Reconnect to cluster without blocking invocations. Invocations will receive <see cref="HazelcastClientOfflineException"/></summary>
        ASYNC
    }

    public class ConnectionStrategyConfig
    {
        public bool AsyncStart { get; set; }
        public ReconnectMode ReconnectMode { get; set; } = ReconnectMode.ON;
        public ConnectionRetryConfig ConnectionRetryConfig { get; set; } = new ConnectionRetryConfig();
    }
}
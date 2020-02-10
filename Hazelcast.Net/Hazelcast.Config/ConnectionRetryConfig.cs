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
    public class ConnectionRetryConfig
    {
        private const int DefaultInitialBackoffMillis = 1000;
        private const int DefaultMaxBackoffMillis = 30000;
        private const long DefaultClusterConnectTimeoutMillis = 20000;
        private const double DefaultJitter = 0;

        public int InitialBackoffMillis { get; set; } = DefaultInitialBackoffMillis;
        public int MaxBackoffMillis { get; set; } = DefaultMaxBackoffMillis;
        public double Multiplier { get; set; } = 1;
        public long ClusterConnectTimeoutMillis { get; set; } = DefaultClusterConnectTimeoutMillis;
        public double Jitter { get; set; } = DefaultJitter;
    }
}
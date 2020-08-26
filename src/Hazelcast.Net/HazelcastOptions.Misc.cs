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

using Hazelcast.Logging;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;

namespace Hazelcast
{
    public partial class HazelcastOptions // Misc
    {
        // NOTE
        // AsyncStart is not an option for the CSharp client,
        // as it would make little sense for our full-async code

        /// <summary>
        /// Gets the logging options.
        /// </summary>
        public LoggingOptions Logging { get; } = new LoggingOptions();

        /// <summary>
        /// Gets the serialization options.
        /// </summary>
        public SerializationOptions Serialization { get; } = new SerializationOptions();

        /// <summary>
        /// Gets  the NearCache options.
        /// </summary>
        public NearCacheOptions NearCache { get; } = new NearCacheOptions();
    }
}

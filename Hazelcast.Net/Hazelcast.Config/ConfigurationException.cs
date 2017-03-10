// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;

namespace Hazelcast.Config
{
    /// <summary>
    /// A
    /// <see cref="Hazelcast.Core.HazelcastException"/>
    /// that is thrown when something is wrong with the server or client configuration.
    /// </summary>
    [Serializable]
    public class ConfigurationException : HazelcastException
    {
        public ConfigurationException(string itemName, string candidate, string duplicate)
            : this(
                string.Format(
                    "Found ambiguous configurations for item \"{0}\": \"{1}\" vs. \"{2}\"\nPlease specify your configuration.",
                    itemName, candidate, duplicate))
        {
        }

        public ConfigurationException(string message)
            : base(message)
        {
        }
    }
}
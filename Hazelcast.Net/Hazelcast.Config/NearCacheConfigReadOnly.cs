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

namespace Hazelcast.Config
{
    /// <summary>
    /// Readonly version of <see cref="NearCacheConfig"/>
    /// </summary>
    public class NearCacheConfigReadOnly : NearCacheConfig
    {

        public NearCacheConfigReadOnly(NearCacheConfig config) : base(config)
        {
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetEvictionPolicy(string evictionPolicy)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetInMemoryFormat(InMemoryFormat inMemoryFormat)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetInMemoryFormat(string inMemoryFormat)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetInvalidateOnChange(bool invalidateOnChange)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetMaxIdleSeconds(int maxIdleSeconds)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetMaxSize(int maxSize)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetName(string name)
        {
            throw new NotSupportedException("This config is read-only");
        }

        /// <summary>
        /// Not supported function in readonly config, throws <exception cref="NotSupportedException"></exception>
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override NearCacheConfig SetTimeToLiveSeconds(int timeToLiveSeconds)
        {
            throw new NotSupportedException("This config is read-only");
        }
    }
}
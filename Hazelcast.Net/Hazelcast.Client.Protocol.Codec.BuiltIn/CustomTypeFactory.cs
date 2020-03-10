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
using System.Collections.Generic;
using System.Threading;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class CustomTypeFactory
    {
        public static Address CreateAddress(string host, int port)
        {
            try
            {
                // The creation of the address uses https://docs.microsoft.com/en-us/dotnet/api/system.net.dns.gethostaddresses
                // This method may throw ArgumentException, SocketException, ArgumentOutOfRangeException, ArgumentNullException
                // Java implementation may throw https://docs.oracle.com/javase/7/docs/api/java/net/UnknownHostException.html
                return new Address(host, port);
            }
            catch (Exception e)
            {
                throw new HazelcastException(e);
            }
        }

        public static SimpleEntryView<IData, IData> CreateSimpleEntryView(IData key, IData value, long cost, long creationTime,
            long expirationTime, long hits, long lastAccessTime, long lastStoredTime, long lastUpdateTime, long version, long ttl,
            long maxIdle)
        {
            return new SimpleEntryView<IData, IData>
            {
                Key = key,
                Value = value,
                Cost = cost,
                CreationTime = creationTime,
                ExpirationTime = expirationTime,
                Hits = hits,
                LastAccessTime = lastAccessTime,
                LastStoredTime = lastStoredTime,
                LastUpdateTime = lastUpdateTime,
                Version = version,
                Ttl = ttl,
                MaxIdle = maxIdle
            };
        }

        public static IndexConfig CreateIndexConfig(string name, int indexType, List<string> attributes,
            BitmapIndexOptions bitmapIndexOptions)
        {
            return new IndexConfig
            {
                Name = name, Type = (IndexType) indexType, Attributes = attributes, BitmapIndexOptions = bitmapIndexOptions
            };
        }

        public static BitmapIndexOptions CreateBitmapIndexOptions(string uniqueKey, int uniqueKeyTransformation)
        {
            return new BitmapIndexOptions
            {
                UniqueKey = uniqueKey, UniqueKeyTransformation = (UniqueKeyTransformation) uniqueKeyTransformation
            };
        }
    }
}
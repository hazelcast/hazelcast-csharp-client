// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Examples.Org.Website.Samples
{
    public class GlobalSerializer : IStreamSerializer<object>
    {
        public int GetTypeId()
        {
            return 20;
        }

        public void Destroy()
        {
        }

        public void Write(IObjectDataOutput output, object obj)
        {
            // out.write(MyFavoriteSerializer.serialize(obj))
        }

        public object Read(IObjectDataInput input)
        {
            // return MyFavoriteSerializer.deserialize(input);
            return null;
        }
    }

    public class GlobalSerializerSample
    {
        public static void Run(string[] args)
        {
            var clientConfig = new ClientConfig();
            clientConfig.GetSerializationConfig().SetGlobalSerializerConfig(
                new GlobalSerializerConfig().SetImplementation(new GlobalSerializer())
            );
            // Start the Hazelcast Client and connect to an already running Hazelcast Cluster on 127.0.0.1
            var hz = HazelcastClient.NewHazelcastClient(clientConfig);
            
            //GlobalSerializer will serialize/deserialize all non-builtin types 

            // Shutdown this Hazelcast Client
            hz.Shutdown();
        }
    }
}
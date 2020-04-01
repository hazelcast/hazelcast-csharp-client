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

using System.Text;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Examples.Org.Website.Samples
{
    internal class CustomSerializableType
    {
        public string Value { get; set; }
    }


    internal class CustomSerializer : IStreamSerializer<CustomSerializableType>
    {
        public int GetTypeId()
        {
            return 10;
        }

        public void Destroy()
        {
        }

        public void Write(IObjectDataOutput output, CustomSerializableType t)
        {
            var array = Encoding.UTF8.GetBytes(t.Value);
            output.WriteInt(array.Length);
            output.Write(array);
        }

        public CustomSerializableType Read(IObjectDataInput input)
        {
            var len = input.ReadInt();
            var array = new byte[len];
            input.ReadFully(array);
            return new CustomSerializableType {Value = Encoding.UTF8.GetString(array)};
        }
    }

    public class CustomSerializerSample
    {
        public static void Run(string[] args)
        {
            var clientConfig = new ClientConfig();
            clientConfig.GetSerializationConfig()
                .AddSerializerConfig(new SerializerConfig()
                    .SetImplementation(new CustomSerializer())
                    .SetTypeClass(typeof(CustomSerializableType)));
            
            // Start the Hazelcast Client and connect to an already running Hazelcast Cluster on 127.0.0.1
            var hz = HazelcastClient.NewHazelcastClient(clientConfig);
            //CustomSerializer will serialize/deserialize CustomSerializable objects
            
            // Shutdown this Hazelcast Client
            hz.Shutdown();
        }
    }
}
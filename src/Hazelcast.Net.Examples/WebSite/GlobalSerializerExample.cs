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

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Hazelcast.Serialization;

namespace Hazelcast.Examples.WebSite
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

        public object Read(IObjectDataInput input)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream(input.ReadByteArray());
            return formatter.Deserialize(stream);
        }

        public void Write(IObjectDataOutput output, object obj)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, obj);
            output.WriteArray(stream.GetBuffer());
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class GlobalSerializerExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // create an Hazelcast client and connect to a server running on localhost
            var options = BuildExampleOptions(args);
            options.Serialization.DefaultSerializer = new SerializerOptions
            {
                Creator = () => new GlobalSerializer()
            };
            await using var client = new HazelcastClientFactory(options).CreateClient();
            await client.ConnectAsync();

            //GlobalSerializer will serialize/deserialize all non-builtin types
        }
    }
}

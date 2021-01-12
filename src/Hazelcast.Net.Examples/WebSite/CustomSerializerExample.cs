// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Serialization;

namespace Hazelcast.Examples.WebSite
{
    internal class CustomSerializableType
    {
        public string Value { get; set; }
    }


    internal class CustomSerializer : IStreamSerializer<CustomSerializableType>
    {
        public int TypeId => 10;

        public void Dispose()
        { }

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
            input.Read(array);
            return new CustomSerializableType {Value = Encoding.UTF8.GetString(array)};
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class CustomSerializerExample : ExampleBase
    {
        public async Task Run(string[] args)
        {
            // create an Hazelcast client and connect to a server running on localhost
            var options = BuildExampleOptions(args);
            options.Serialization.Serializers.Add(new SerializerOptions
            {
                SerializedType = typeof(CustomSerializableType),
                Creator = () => new CustomSerializer()
            });
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            //CustomSerializer will serialize/deserialize CustomSerializable objects
        }
    }
}

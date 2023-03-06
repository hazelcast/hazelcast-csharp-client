// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization
{
    // note
    //
    // "The BinaryFormatter type is dangerous and is not recommended for data processing. Applications
    // should stop using BinaryFormatter as soon as possible, even if they believe the data they're
    // processing to be trustworthy. BinaryFormatter is insecure and can't be made secure."
    //
    // see: https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2300
    // see: https://docs.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide
    //
    // TODO: replace BinaryFormatter with something else (breaking?)

    /// <summary>
    /// Serialize using default .NET serialization
    /// </summary>
    internal class SerializableSerializer : SingletonSerializerBase<object>
    {
        public override int TypeId => SerializationConstants.CsharpClrSerializationType;

        public override object Read(IObjectDataInput input)
        {
#pragma warning disable SYSLIB0011
            var formatter = new BinaryFormatter();
            using var stream = new MemoryStream(input.ReadByteArray());
#pragma warning disable CA2300 // Do not use insecure deserializer BinaryFormatter - see note above
#pragma warning disable CA2301
            return formatter.Deserialize(stream);
#pragma warning restore CA2300
#pragma warning restore CA2301
#pragma warning restore SYSLIB0011
        }

        public override void Write(IObjectDataOutput output, object obj)
        {
#pragma warning disable SYSLIB0011
            var formatter = new BinaryFormatter();
            using var stream = new MemoryStream();
            formatter.Serialize(stream, obj);
            output.WriteByteArray(stream.GetBuffer());
#pragma warning restore SYSLIB0011
        }
    }
}

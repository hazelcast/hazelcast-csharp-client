// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal interface ISerializationServiceBuilder
    {
        ISerializationServiceBuilder AddClassDefinition(IClassDefinition cd);
        ISerializationServiceBuilder AddDataSerializableFactory(int id, IDataSerializableFactory factory);
        ISerializationServiceBuilder AddPortableFactory(int id, IPortableFactory factory);
        ISerializationService Build();
        ISerializationServiceBuilder SetByteOrder(ByteOrder byteOrder);
        ISerializationServiceBuilder SetCheckClassDefErrors(bool checkClassDefErrors);
        ISerializationServiceBuilder SetConfig(SerializationConfig config);
        ISerializationServiceBuilder SetEnableCompression(bool enableCompression);
        ISerializationServiceBuilder SetEnableSharedObject(bool enableSharedObject);
        ISerializationServiceBuilder SetHazelcastInstance(IHazelcastInstance hazelcastInstance);
        ISerializationServiceBuilder SetInitialOutputBufferSize(int initialOutputBufferSize);
        ISerializationServiceBuilder SetManagedContext(IManagedContext managedContext);
        ISerializationServiceBuilder SetPartitioningStrategy(IPartitioningStrategy partitionStrategy);
        ISerializationServiceBuilder SetPortableVersion(int version);
        ISerializationServiceBuilder SetUseNativeByteOrder(bool useNativeByteOrder);
        ISerializationServiceBuilder SetVersion(byte version);
    }
}
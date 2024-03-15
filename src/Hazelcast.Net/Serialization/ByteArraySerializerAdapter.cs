﻿// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization
{
    internal class ByteArraySerializerAdapter<T> : ISerializerAdapter
    {
        private readonly IByteArraySerializer<T> _serializer;

        public ByteArraySerializerAdapter(IByteArraySerializer<T> serializer)
        {
            _serializer = serializer;
        }

        public int TypeId => _serializer.TypeId;

        public ISerializer Serializer => _serializer;

        public void Write(IObjectDataOutput output, object obj)
        {
            var bytes = _serializer.Write((T) obj);
            output.WriteByteArray(bytes);
        }

        public virtual object Read(IObjectDataInput input)
        {
            var bytes = input.ReadByteArray();
            if (bytes == null || bytes.Length == 0) return null;
            return _serializer.Read(bytes);
        }

        public virtual void Dispose() => _serializer.Dispose();
    }
}

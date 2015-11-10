// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.IO.Serialization
{
    internal class ByteArraySerializerAdapter<T> : ISerializerAdapter
    {
        private readonly IByteArraySerializer<T> _serializer;

        public ByteArraySerializerAdapter(IByteArraySerializer<T> serializer)
        {
            _serializer = serializer;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void Write(IObjectDataOutput output, object obj)
        {
            var bytes = _serializer.Write((T) obj);
            output.WriteByteArray(bytes);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual object Read(IObjectDataInput @in)
        {
            var bytes = @in.ReadByteArray();
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }
            return _serializer.Read(bytes);
        }

        public int GetTypeId()
        {
            return _serializer.GetTypeId();
        }

        public virtual void Destroy()
        {
            _serializer.Destroy();
        }

        public virtual ISerializer GetImpl()
        {
            return _serializer;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }
            var that = (ByteArraySerializerAdapter<T>) o;
            if (_serializer != null ? !_serializer.Equals(that._serializer) : that._serializer != null)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return _serializer != null ? _serializer.GetHashCode() : 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("SerializerAdapter{");
            sb.Append("serializer=").Append(_serializer);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
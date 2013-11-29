using System.Text;

namespace Hazelcast.IO.Serialization
{
    internal sealed class ByteArraySerializerAdapter<T> : ISerializerAdapter
    {
        private readonly IByteArraySerializer<T> _serializer;

        public ByteArraySerializerAdapter(IByteArraySerializer<T> serializer)
        {
            _serializer = serializer;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Write(IObjectDataOutput output, object obj)
        {
            byte[] bytes = _serializer.Write((T) obj);
            output.WriteInt(bytes != null ? bytes.Length : 0);
            output.Write(bytes);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public object Read(IObjectDataInput input)
        {
            int len = input.ReadInt();
            if (len > 0)
            {
                var bytes = new byte[len];
                input.ReadFully(bytes);
                return _serializer.Read(bytes);
            }
            return null;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public byte[] Write(object obj)
        {
            return _serializer.Write((T) obj);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public object Read(Data data)
        {
            return _serializer.Read(data.buffer);
        }

        public int GetTypeId()
        {
            return _serializer.GetTypeId();
        }

        public void Destroy()
        {
            _serializer.Destroy();
        }

        public ISerializer GetImpl()
        {
            return _serializer;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("SerializerAdapter{");
            sb.Append("serializer=").Append(_serializer);
            sb.Append('}');
            return sb.ToString();
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
    }
}
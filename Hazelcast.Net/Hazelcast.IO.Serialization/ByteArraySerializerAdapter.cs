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
            byte[] bytes = _serializer.Write((T) obj);
            output.WriteByteArray(bytes);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual object Read(IObjectDataInput @in)
        {
            byte[] bytes = @in.ReadByteArray();
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }
            return _serializer.Read(bytes);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual IData ToData(object obj, int partitionHash)
        {
            byte[] data = _serializer.Write((T) obj);
            return new Data(_serializer.GetTypeId(), data, partitionHash);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual object ToObject(IData data)
        {
            return _serializer.Read(data.GetData());
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
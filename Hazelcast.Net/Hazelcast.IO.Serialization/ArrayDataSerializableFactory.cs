using System;

namespace Hazelcast.IO.Serialization
{
    internal sealed class ArrayDataSerializableFactory : IDataSerializableFactory
    {
        private readonly Func<int, IIdentifiedDataSerializable>[] _constructors;

        private readonly int _length;

        public ArrayDataSerializableFactory(Func<int, IIdentifiedDataSerializable>[] ctorArray)
        {
            if (ctorArray != null && ctorArray.Length > 0)
            {
                _length = ctorArray.Length;
                _constructors = new Func<int, IIdentifiedDataSerializable>[_length];
                Array.Copy(ctorArray, 0, _constructors, 0, _length);
            }
            else
            {
                _length = 0;
                _constructors = new Func<int, IIdentifiedDataSerializable>[_length];
            }
        }

        public IIdentifiedDataSerializable Create(int typeId)
        {
            if (typeId >= 0 && typeId < _length)
            {
                Func<int, IIdentifiedDataSerializable> factory = _constructors[typeId];
                return factory != null ? factory(typeId) : null;
            }
            return null;
        }
    }
}
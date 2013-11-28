using System;

namespace Hazelcast.IO.Serialization
{
    public sealed class ArrayDataSerializableFactory : IDataSerializableFactory
    {
        private readonly Func<int, IIdentifiedDataSerializable>[] constructors;

        private readonly int len;

        public ArrayDataSerializableFactory(Func<int, IIdentifiedDataSerializable>[] ctorArray)
        {
            if (ctorArray != null && ctorArray.Length > 0)
            {
                len = ctorArray.Length;
                constructors = new Func<int, IIdentifiedDataSerializable>[len];
                Array.Copy(ctorArray, 0, constructors, 0, len);
            }
            else
            {
                len = 0;
                constructors = new Func<int, IIdentifiedDataSerializable>[len];
            }
        }

        public IIdentifiedDataSerializable Create(int typeId)
        {
            if (typeId >= 0 && typeId < len)
            {
                Func<int, IIdentifiedDataSerializable> factory = constructors[typeId];
                return factory != null ? factory(typeId) : null;
            }
            return null;
        }
    }
}
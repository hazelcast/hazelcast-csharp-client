using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hazelcast.IO.Serialization
{
    internal class ArrayDataSerializableFactory : IDataSerializableFactory
    {
        private readonly Func<IIdentifiedDataSerializable>[] _consturctors;

        public ArrayDataSerializableFactory(Func<IIdentifiedDataSerializable>[] consturctors)
        {
            _consturctors = consturctors;
        }

        public IIdentifiedDataSerializable Create(int typeId)
        {
            if (typeId < 0 && typeId >= _consturctors.Length)
            {
                return null;
            }
            return _consturctors[typeId]();
        }
    }
}

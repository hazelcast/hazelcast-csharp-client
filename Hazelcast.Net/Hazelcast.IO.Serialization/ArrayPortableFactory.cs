using System;

namespace Hazelcast.IO.Serialization
{
    internal class ArrayPortableFactory : IPortableFactory
    {
        private readonly Func<int, IPortable>[] _constructors;

        private readonly int _length;

        public ArrayPortableFactory(Func<int, IPortable>[] ctorArray)
        {
            if (ctorArray != null && ctorArray.Length > 0)
            {
                _length = ctorArray.Length;
                _constructors = new Func<int, IPortable>[_length];
                Array.Copy(ctorArray, 0, _constructors, 0, _length);
            }
            else
            {
                _length = 0;
                _constructors = new Func<int, IPortable>[_length];
            }
        }

        public virtual IPortable Create(int classId)
        {
            if (classId < 0 || classId >= _length) return null;
            Func<int, IPortable> factory = _constructors[classId];
            return factory != null ? factory(classId) : null;
        }
    }
}
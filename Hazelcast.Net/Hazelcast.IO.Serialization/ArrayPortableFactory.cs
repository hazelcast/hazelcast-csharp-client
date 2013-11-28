using System;

namespace Hazelcast.IO.Serialization
{
    public class ArrayPortableFactory : IPortableFactory
    {
        private readonly Func<int, IPortable>[] _constructors;

        private readonly int len;

        public ArrayPortableFactory(Func<int, IPortable>[] ctorArray)
        {
            if (ctorArray != null && ctorArray.Length > 0)
            {
                len = ctorArray.Length;
                _constructors = new Func<int, IPortable>[len];
                Array.Copy(ctorArray, 0, _constructors, 0, len);
            }
            else
            {
                len = 0;
                _constructors = new Func<int, IPortable>[len];
            }
        }

        public virtual IPortable Create(int classId)
        {
            if (classId >= 0 && classId < len)
            {
                Func<int, IPortable> factory = _constructors[classId];
                return factory != null ? factory(classId) : null;
            }
            return null;
        }
    }
}
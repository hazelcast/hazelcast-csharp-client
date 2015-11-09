using System;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Examples.Models
{
    class ExamplePortableFactory : IPortableFactory
    {
        public const int Id = 1;

        public IPortable Create(int classId)
        {
            if (classId == Customer.ClassId)
            {
                return new Customer();
            } 

            throw new ArgumentException("Unknown class id " + classId);
        }
    }
}

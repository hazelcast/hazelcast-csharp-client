using System;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Examples.Models
{
    public class Employee : IIdentifiedDataSerializable
    {
        public const int TypeId = 100;

        public int Id { get; set; }
        public string Name { get; set; }

        public void ReadData(IObjectDataInput input)
        {
            Id = input.ReadInt();
            Name = input.ReadUTF();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(Id);
            output.WriteUTF(Name);
        }

        public int GetFactoryId()
        {
            return ExampleDataSerializableFactory.FactoryId;
        }

        public int GetId()
        {
            return TypeId;
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Name: {1}", Id, Name);
        }
    }

    public class ExampleDataSerializableFactory : IDataSerializableFactory
    {
        public const int FactoryId = 1000;
        public IIdentifiedDataSerializable Create(int typeId)
        {
            if (typeId == 100) return new Employee();
            throw new InvalidOperationException("Unknown type id");
        }
    }
}

using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Examples.Models
{
    class Person : IDataSerializable
    {
        public string Name { get; private set; }
        public int Age { get; private set; }
        public int Id { get; private set; }

        public Person()
        {
            
        }

        public Person(int id , string name, int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(Id);
            output.WriteUTF(Name);
            output.WriteInt(Age);
        }

        public void ReadData(IObjectDataInput input)
        {
            Id = input.ReadInt();
            Name = input.ReadUTF();
            Age = input.ReadInt();
        }

        public string GetJavaClassName()
        {
            return "com.hazelcast.examples.Person";
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, Age: {1}, Id: {2}", Name, Age, Id);
        }
    }
}

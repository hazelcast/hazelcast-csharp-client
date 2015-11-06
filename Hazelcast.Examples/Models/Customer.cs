using System;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Examples.Models
{
    public class Customer : IPortable
    {
        public const int ClassId = 1;

        public string Name { get; set; }
        public int Id { get; set; }
        public DateTime LastOrder { get; set; }

        public int GetFactoryId()
        {
            return ExamplePortableFactory.Id;
        }

        public int GetClassId()
        {
            return ClassId;
        }

        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteInt("id", Id);
            writer.WriteUTF("name", Name);
            writer.WriteLong("lastOrder", LastOrder.ToFileTimeUtc());
        }

        public void ReadPortable(IPortableReader reader)
        {
            Id = reader.ReadInt("id");
            Name = reader.ReadUTF("name");
            LastOrder = DateTime.FromFileTimeUtc(reader.ReadLong("lastOrder"));
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, Id: {1}, LastOrder: {2}", Name, Id, LastOrder);
        }
    }
}

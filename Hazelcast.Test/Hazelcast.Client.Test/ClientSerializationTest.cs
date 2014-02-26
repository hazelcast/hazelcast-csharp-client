using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientSerializationTest
    {

         [SetUp]
        public void Init()
        {
        }

        [TearDown]
        public static void Destroy()
        {
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestISerializable()
        {
            var person = new Person(125, "john", "doe", 30);
            BinaryFormatter bf= new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms,person);

            byte[] array = ms.ToArray();

            Assert.IsNotEmpty(array);
        }
        
        [Test]
        public virtual void TestXMLSerializable()
        {
            var person = new Person(125, "john", "doe", 30);
            var bf = new SoapFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms,person);

            byte[] array = ms.ToArray();

            string text = Encoding.UTF8.GetString(array);
            Console.WriteLine(text);
            Assert.IsNotEmpty(text);

        }
        [Test]
        public virtual void TestXMLDESerializable()
        {
            var person = new Person(125, "john", "doe", 30);
            var bf = new SoapFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, person);

            byte[] array = ms.ToArray();

            //string text = Encoding.UTF8.GetString(array);

            string textD = "<SOAP-ENV:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:clr=\"http://schemas.microsoft.com/soap/encoding/clr/1.0\" SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n<SOAP-ENV:Body>\r\n<a1:Person id=\"ref-1\" xmlns:a1=\"http://schemas.microsoft.com/clr/nsassem/Hazelcast.Client.Test/Hazelcast.Test%2C%20Version%3D1.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Dnull\">\r\n<id>125</id>\r\n<name id=\"ref-3\">john</name>\r\n<surname id=\"ref-4\">doe</surname>\r\n<age>30</age>\r\n</a1:Person>\r\n</SOAP-ENV:Body>\r\n</SOAP-ENV:Envelope>";
            var bf1 = new SoapFormatter();
            var ms1 = new MemoryStream();

            byte[] buffer = array;// Encoding.UTF8.GetBytes(text);

            ms1.Write(buffer, 0,buffer.Count());
            Person deserialize = (Person) bf1.Deserialize(ms1);

            var person1 = new Person(125, "john", "doe", 30);
            Assert.AreEqual(deserialize,person1);

        }
    }

    [Serializable]
    class Person//:ISerializable
    {
        private int id;
        private string name;
        private string surname;
        private int age;

        public Person(int id, string name, string surname, int age)
        {
            this.id = id;
            this.name = name;
            this.surname = surname;
            this.age = age;
        }

        protected Person(SerializationInfo info, StreamingContext context)
        {
            this.id = (int)info.GetValue("id", typeof(int));
            this.name = (string)info.GetValue("name", typeof(string));
            this.surname = (string)info.GetValue("surname", typeof(string));
            this.age = (int)info.GetValue("age", typeof(int));
        }

        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("id", id, typeof(int));
        //    info.AddValue("name", name, typeof(string));
        //    info.AddValue("surname", surname, typeof(string));
        //    info.AddValue("age", age, typeof(int));
        //}

        public override bool Equals(object obj)
        {
            var other = obj as Person;
            if (other == null)
            {
                return false;
            }
            return
                id == other.id &&
                name != null && name.Equals(other.name) &&
                surname != null && surname.Equals(other.surname) &&
                age == other.age;
        }
    }
}

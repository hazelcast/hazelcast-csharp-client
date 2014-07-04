using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientSerializationTest//:HazelcastBaseTest
    {

        internal static IMap<object, object> map;

        ////
        //[SetUp]
        //public void Init()
        //{
        //    map = client.GetMap<object, object>(Name);
        //}

        //[TearDown]
        //public static void Destroy()
        //{
        //    map.Clear();
        //}


        //protected override IHazelcastInstance NewHazelcastClient()
        //{
        //    var config = new ClientConfig();
        //    config.GetNetworkConfig().AddAddress("127.0.0.1");
        //    config.GetSerializationConfig().SetPortableVersion(99);
        //    config.GetSerializationConfig().AddPortableFactory(1, new MyPortableFactory());
        //    return HazelcastClient.NewHazelcastClient(config);
        //}


        //private void TestDataTypes(ISerializationService serviceIn, ISerializationService serviceOut)
        //{
        //    int d1 = Int32.MaxValue;
        //    long d2 = Int64.MaxValue;
        //    float d3 = Single.MaxValue;
        //    double d4 = Double.MaxValue;
        //    char d5 = 'a';
        //    string d6 = "abc";
        //    byte d7 = Byte.MaxValue;
        //    short d8 = Int16.MaxValue;
        //    bool d9 = true;

        //    int[] d11 = new[] { Int32.MaxValue };
        //    long[] d21 = new[] { Int64.MaxValue };
        //    float[] d31 = new[] { Single.MaxValue };
        //    double[] d41 = new[] { Double.MaxValue };
        //    char[] d51 = new[] { 'a' };
        //    byte[] d71 = new[] { Byte.MaxValue };
        //    short[] d81 = new[] { Int16.MaxValue };

        //    Assert.AreEqual(d11, serviceOut.ToObject<int[]>(serviceIn.ToData(d11)));
        //    Assert.AreEqual(d21, serviceOut.ToObject<long[]>(serviceIn.ToData(d21)));
        //    Assert.AreEqual(d31, serviceOut.ToObject<float[]>(serviceIn.ToData(d31)));
        //    Assert.AreEqual(d41, serviceOut.ToObject<double[]>(serviceIn.ToData(d41)));
        //    Assert.AreEqual(d51, serviceOut.ToObject<char[]>(serviceIn.ToData(d51)));
        //    Assert.AreEqual(d71, serviceOut.ToObject<byte[]>(serviceIn.ToData(d71)));
        //    Assert.AreEqual(d81, serviceOut.ToObject<short[]>(serviceIn.ToData(d81)));

        //    Assert.AreEqual(d1, serviceOut.ToObject<int>(serviceIn.ToData(d1)));
        //    Assert.AreEqual(d2, serviceOut.ToObject<long>(serviceIn.ToData(d2)));
        //    Assert.AreEqual(d3, serviceOut.ToObject<float>(serviceIn.ToData(d3)));
        //    Assert.AreEqual(d4, serviceOut.ToObject<double>(serviceIn.ToData(d4)));
        //    Assert.AreEqual(d5, serviceOut.ToObject<char>(serviceIn.ToData(d5)));
        //    Assert.AreEqual(d6, serviceOut.ToObject<string>(serviceIn.ToData(d6)));
        //    Assert.AreEqual(d7, serviceOut.ToObject<byte>(serviceIn.ToData(d7)));
        //    Assert.AreEqual(d8, serviceOut.ToObject<short>(serviceIn.ToData(d8)));
        //    Assert.AreEqual(d9, serviceOut.ToObject<bool>(serviceIn.ToData(d9)));

        //}

        //[Test]
        //public virtual void TestDataTypesSerialization()
        //{
        //    ISerializationService service = ((HazelcastClientProxy) client).GetSerializationService();

        //    TestDataTypes(service, service);
        //}

        //[Test]
        //public virtual void TestPortableVersion()
        //{
        //    var cc = new ClientConfig();
        //    cc.GetNetworkConfig().AddAddress("127.0.0.1");
        //    cc.GetSerializationConfig().SetPortableVersion(10);
        //    IHazelcastInstance instance = HazelcastClient.NewHazelcastClient(cc);
        //    ISerializationService serviceV10 = ((HazelcastClientProxy)instance).GetSerializationService();

        //    ISerializationService service = ((HazelcastClientProxy)client).GetSerializationService();

        //    TestDataTypes(service, serviceV10);
        //    TestDataTypes(serviceV10, service);

        //    instance.Shutdown();
        //}

        //[Test]
        public virtual void TestIPortable()
        {
            var foo1 = new Foo("FOO1");
            var foo2 = new Foo("FOO2");
            var foo3 = new Foo("FOO3");

            map.Put("k1", foo1);
            map.Put("k2", foo2);
            map.Put("k3", foo3);

            Assert.AreEqual(foo1,map.Get("k1"));
            Assert.AreEqual(foo2,map.Get("k2"));
            Assert.AreEqual(foo3,map.Get("k3"));
        }

        //[Test]
        public virtual void TestIPortablePredicate()
        {
            var foo1 = new Foo("FOO1");
            var foo2 = new Foo("FOO2");
            var foo3 = new Foo("FOO3");

            map.Put("k1", foo1);
            map.Put("k2", foo2);
            map.Put("k3", foo3);

            ISet<KeyValuePair<object, object>> set = map.EntrySet(new SqlPredicate("this.foo == 'FOO2'"));
            Assert.AreEqual(1,set.Count);

            IEnumerator<KeyValuePair<object, object>> enumerator = set.GetEnumerator();
            enumerator.MoveNext();

            Assert.AreEqual(foo2, enumerator.Current.Value);
        }

        //[Test]
        public virtual void TestTypeName()
        {
            var foo1 = new Foo("FOO1");

            Console.WriteLine(foo1.GetType().Name);
            string fullName = foo1.GetType().FullName;
            Console.WriteLine(fullName);
            
            ObjectHandle instanceFrom = Activator.CreateInstance(null, fullName);

            object unwrap = instanceFrom.Unwrap();

            Console.WriteLine(unwrap.GetType().FullName);
        }


        ///// <exception cref="System.Exception"></exception>
        //[Test]
        //public virtual void TestISerializable()
        //{
        //    var person = new Person(125, "john", "doe", 30);
        //    BinaryFormatter bf= new BinaryFormatter();
        //    MemoryStream ms = new MemoryStream();
        //    bf.Serialize(ms,person);

        //    byte[] array = ms.ToArray();

        //    Assert.IsNotEmpty(array);
        //}
        
        //[Test]
        //public virtual void TestXMLSerializable()
        //{
        //    var person = new Person(125, "john", "doe", 30);
        //    var bf = new SoapFormatter();
        //    var ms = new MemoryStream();
        //    bf.Serialize(ms,person);

        //    byte[] array = ms.ToArray();

        //    string text = Encoding.UTF8.GetString(array);
        //    Console.WriteLine(text);
        //    Assert.IsNotEmpty(text);

        //}
        //[Test]
        //public virtual void TestXMLDESerializable()
        //{
        //    var person = new Person(125, "john", "doe", 30);
        //    var bf = new SoapFormatter();
        //    var ms = new MemoryStream();
        //    bf.Serialize(ms, person);

        //    byte[] array = ms.ToArray();

        //    //string text = Encoding.UTF8.GetString(array);

        //    string textD = "<SOAP-ENV:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:clr=\"http://schemas.microsoft.com/soap/encoding/clr/1.0\" SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n<SOAP-ENV:Body>\r\n<a1:Person id=\"ref-1\" xmlns:a1=\"http://schemas.microsoft.com/clr/nsassem/Hazelcast.Client.Test/Hazelcast.Test%2C%20Version%3D1.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Dnull\">\r\n<id>125</id>\r\n<name id=\"ref-3\">john</name>\r\n<surname id=\"ref-4\">doe</surname>\r\n<age>30</age>\r\n</a1:Person>\r\n</SOAP-ENV:Body>\r\n</SOAP-ENV:Envelope>";
        //    var bf1 = new SoapFormatter();
        //    var ms1 = new MemoryStream();

        //    byte[] buffer = array;// Encoding.UTF8.GetBytes(text);

        //    ms1.Write(buffer, 0,buffer.Count());
        //    Person deserialize = (Person) bf1.Deserialize(ms1);

        //    var person1 = new Person(125, "john", "doe", 30);
        //    Assert.AreEqual(deserialize,person1);

        //}
    }

    class Foo : IPortable
    {
        public static int ID = 5;
        private String _foo;

        public Foo() { }

        public Foo(string foo=null)
        {
            this._foo = foo;
        }

        public string FooValue
        {
            get { return _foo; }
            set { _foo = value; }
        }

        public int GetFactoryId()
        {
                return 1;
        }

        public int GetClassId()
        {
            return ID;
        }

        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("foo", _foo);
        }

        public void ReadPortable(IPortableReader reader)
        {
            _foo = reader.ReadUTF("foo");
        }

        protected bool Equals(Foo other)
        {
            return string.Equals(_foo, other._foo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Foo) obj);
        }

        public override int GetHashCode()
        {
            return (_foo != null ? _foo.GetHashCode() : 0);
        }
    }

    class MyPortableFactory : IPortableFactory {

        public IPortable Create(int classId) {
            if (Foo.ID == classId)
                return new Foo();
            else return null;
        }
    }

    //[Serializable]
    //class Person//:ISerializable
    //{
    //    private int id;
    //    private string name;
    //    private string surname;
    //    private int age;

    //    public Person(int id, string name, string surname, int age)
    //    {
    //        this.id = id;
    //        this.name = name;
    //        this.surname = surname;
    //        this.age = age;
    //    }

    //    protected Person(SerializationInfo info, StreamingContext context)
    //    {
    //        this.id = (int)info.GetValue("id", typeof(int));
    //        this.name = (string)info.GetValue("name", typeof(string));
    //        this.surname = (string)info.GetValue("surname", typeof(string));
    //        this.age = (int)info.GetValue("age", typeof(int));
    //    }

    //    //public void GetObjectData(SerializationInfo info, StreamingContext context)
    //    //{
    //    //    info.AddValue("id", id, typeof(int));
    //    //    info.AddValue("name", name, typeof(string));
    //    //    info.AddValue("surname", surname, typeof(string));
    //    //    info.AddValue("age", age, typeof(int));
    //    //}

    //    public override bool Equals(object obj)
    //    {
    //        var other = obj as Person;
    //        if (other == null)
    //        {
    //            return false;
    //        }
    //        return
    //            id == other.id &&
    //            name != null && name.Equals(other.name) &&
    //            surname != null && surname.Equals(other.surname) &&
    //            age == other.age;
    //    }
    //}
}

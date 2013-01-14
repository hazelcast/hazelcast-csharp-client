using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.IO;
namespace Hazelcast.Client.Tests
{

    public class Customer : Hazelcast.IO.DataSerializable
    {
        private string p;
        private string p_2;
        public Customer()
        {

        }

        public Customer(string p, string p_2)
        {
            // TODO: Complete member initialization
            this.p = p;
            this.p_2 = p_2;
        }
        public string p1
        {
            get { return p; }
            set { p = value; }
        }
        public string p2
        {
            get { return p_2; }
            set { p_2 = value; }
        }
        public void writeData(IDataOutput dout)
        {
            dout.writeUTF(p);
            dout.writeUTF(p_2);
        }

        public void readData(IDataInput din)
        {
            p = din.readUTF();
            p_2 = din.readUTF();
        }
    }
    public class MyTypeConverter : Hazelcast.IO.ITypeConverter
    {
        public string getJavaName(Type type)
        {
            if (type.Equals(typeof(Customer)))
                return "HazelCastClient.Customer";

            return null;
        }

        public Type getType(String javaName)
        {
            if ("HazelCastClient.Customer".Equals(javaName))
                return typeof(Customer);

            return null;
        }
    }
}

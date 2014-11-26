using System;

namespace Hazelcast.Net.Ext
{
    /// <summary>
    /// Represent the endienness 
    /// </summary>
    public class ByteOrder
    {
        /// <summary>
        /// Big Endian
        /// </summary>
        public static readonly ByteOrder BigEndian = new ByteOrder("BIG_ENDIAN");
        /// <summary>
        /// Little endian
        /// </summary>
        public static readonly ByteOrder LittleEndian = new ByteOrder("LITTLE_ENDIAN");

        private string _name;

        private ByteOrder(String name)
        {
            this._name = name;
        }

        public static ByteOrder NativeOrder()
        {
            return BitConverter.IsLittleEndian ? LittleEndian : BigEndian;
        }
    }
}
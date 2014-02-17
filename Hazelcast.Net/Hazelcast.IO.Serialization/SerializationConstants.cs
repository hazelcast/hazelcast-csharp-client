namespace Hazelcast.IO.Serialization
{
    internal sealed class SerializationConstants
    {
        public const int ConstantTypePortable = -1;

        public const int ConstantTypeData = -2;

        public const int ConstantTypeByte = -3;

        public const int ConstantTypeBoolean = -4;

        public const int ConstantTypeChar = -5;

        public const int ConstantTypeShort = -6;

        public const int ConstantTypeInteger = -7;

        public const int ConstantTypeLong = -8;

        public const int ConstantTypeFloat = -9;

        public const int ConstantTypeDouble = -10;

        public const int ConstantTypeString = -11;

        public const int ConstantTypeByteArray = -12;

        public const int ConstantTypeCharArray = -13;

        public const int ConstantTypeShortArray = -14;

        public const int ConstantTypeIntegerArray = -15;

        public const int ConstantTypeLongArray = -16;

        public const int ConstantTypeFloatArray = -17;

        public const int ConstantTypeDoubleArray = -18;

        public const int ConstantSerializersLength = 18;

        public const int DefaultTypeClass = -19;

        public const int DefaultTypeDate = -20;

        public const int DefaultTypeBigInteger = -21;

        public const int DefaultTypeBigDecimal = -22;

        public const int DefaultTypeObject = -23;

        public const int DefaultTypeExternalizable = -24;

        public const int DefaultTypeEnum = -25;

        private SerializationConstants()
        {
        }

        // WARNING: DON'T CHANGE VALUES!
        // WARNING: DON'T ADD ANY NEW CONSTANT SERIALIZER!
        // NUMBER OF CONSTANT SERIALIZERS...
        // ------------------------------------------------------------
        // DEFAULT SERIALIZERS
    }
}
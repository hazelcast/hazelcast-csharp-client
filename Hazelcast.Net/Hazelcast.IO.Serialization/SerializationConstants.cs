// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Hazelcast.IO.Serialization
{
    internal sealed class SerializationConstants
    {
        public const int ConstantTypeNull = 0;
        public const int ConstantTypePortable = -1;
        public const int ConstantTypeDataSerializable = -2;
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
        public const int ConstantTypeBooleanArray = -13;
        public const int ConstantTypeCharArray = -14;
        public const int ConstantTypeShortArray = -15;
        public const int ConstantTypeIntegerArray = -16;
        public const int ConstantTypeLongArray = -17;
        public const int ConstantTypeFloatArray = -18;
        public const int ConstantTypeDoubleArray = -19;
        public const int ConstantTypeStringArray = -20;

        public const int DefaultTypeJavaClass = -21;
        public const int DefaultTypeDate = -22;
        public const int DefaultTypeBigInteger = -23;
        public const int DefaultTypeBigDecimal = -24;
        public const int DefaultTypeJavaEnum = -25;
        public const int DefaultTypeArrayList = -26;
        public const int DefaultTypeLinkedList = -27;

        public const int ConstantSerializersLength = 28;

        public const int DefaultTypeSerializable = -110; //C# serializable
    }

    internal static class FactoryIds
    {
        public const int PredicateFactoryId = -32;
        public const int AggregatorDsFactoryId = -41;
        public const int ProjectionDsFactoryId = -42;
    }
}
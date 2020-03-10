// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
        // WARNING: DON'T CHANGE VALUES!
        // WARNING: DON'T ADD ANY NEW CONSTANT SERIALIZER!

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
        public const int ConstantTypeUuid = -21;
        public const int ConstantTypeSimpleEntry = -22;
        public const int ConstantTypeSimpleImmutableEntry = -23;

        // ------------------------------------------------------------
        // DEFAULT SERIALIZERS

        public const int JavaDefaultTypeClass = -24;
        public const int JavaDefaultTypeDate = -25;
        public const int JavaDefaultTypeBigInteger = -26;
        public const int JavaDefaultTypeBigDecimal = -27;
        public const int JavaDefaultTypeArray = -28;
        public const int JavaDefaultTypeArrayList = -29;
        public const int JavaDefaultTypeLinkedList = -30;
        public const int JavaDefaultTypeCopyOnWriteArrayList = -31;
        public const int JavaDefaultTypeHashMap = -32;
        public const int JavaDefaultTypeConcurrentSkipListMap = -33;
        public const int JavaDefaultTypeConcurrentHashMap = -34;
        public const int JavaDefaultTypeLinkedHashMap = -35;
        public const int JavaDefaultTypeTreeMap = -36;
        public const int JavaDefaultTypeHashSet = -37;
        public const int JavaDefaultTypeTreeSet = -38;
        public const int JavaDefaultTypeLinkedHashSet = -39;
        public const int JavaDefaultTypeCopyOnWriteArraySet = -40;
        public const int JavaDefaultTypeConcurrentSkipListSet = -41;
        public const int JavaDefaultTypeArrayDeque = -42;
        public const int JavaDefaultTypeLinkedBlockingQueue = -43;
        public const int JavaDefaultTypeArrayBlockingQueue = -44;
        public const int JavaDefaultTypePriorityBlockingQueue = -45;
        public const int JavaDefaultTypeDelayQueue = -46;
        public const int JavaDefaultTypeSynchronousQueue = -47;
        public const int JavaDefaultTypeLinkedTransferQueue = -48;
        public const int JavaDefaultTypePriorityQueue = -49;
        public const int JavaDefaultTypeEnum = -50;

        // ------------------------------------------------------------
        // JAVA SERIALIZATION

        public const int JavaDefaultTypeSerializable = -100;
        public const int JavaDefaultTypeExternalizable = -101;

        // ------------------------------------------------------------
        // LANGUAGE SPECIFIC SERIALIZERS
        // USED BY CLIENTS (Not deserialized by server)

        public const int CsharpClrSerializationType = -110;
        public const int PythonPickleSerializationType = -120;
        public const int JavascriptJsonSerializationType = -130;
        public const int GoGobSerializationType = -140;

        public const int ConstantSerializersArraySize = 200;
        
        // RESERVED FOR  HIBERNATE SERIALIZERS: -200 to -300
        
        // RESERVED FOR JET: -300 to -400
    }

    internal static class FactoryIds
    {
        public const int PredicateFactoryId = -20;
        public const int AggregatorDsFactoryId = -29;
        public const int ProjectionDsFactoryId = -30;
    }
}
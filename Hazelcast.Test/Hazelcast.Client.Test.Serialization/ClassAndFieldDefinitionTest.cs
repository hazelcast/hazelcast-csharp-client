﻿// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.IO.Serialization;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    public class ClassAndFieldDefinitionTest
    {
        private static readonly string[] fieldNames = {"f1", "f2", "f3"};
        private ClassDefinition classDefinition;

        [SetUp]
        public virtual void SetUp()
        {
            var builder = new ClassDefinitionBuilder(1, 2, 3);
            foreach (var fieldName in fieldNames)
            {
                builder.AddByteField(fieldName);
            }
            classDefinition = (ClassDefinition) builder.Build();
        }

        [Test]
        public virtual void TestClassDef_equal_hashCode()
        {
            var cdEmpty1 = (ClassDefinition) new ClassDefinitionBuilder(1, 2, 3).Build();
            var cdEmpty2 = (ClassDefinition) new ClassDefinitionBuilder(1, 2, 3).Build();
            var cd1 = (ClassDefinition) new ClassDefinitionBuilder(1, 2, 5).Build();
            var cd2 = (ClassDefinition) new ClassDefinitionBuilder(2, 2, 3).Build();
            var cd3 = (ClassDefinition) new ClassDefinitionBuilder(1, 9, 3).Build();
            var cdWithField =
                (ClassDefinition) new ClassDefinitionBuilder(1, 2, 3).AddIntField("f1").Build();
            Assert.AreEqual(cdEmpty1, cdEmpty2);
            Assert.AreNotEqual(cd1, cdEmpty1);
            Assert.AreNotEqual(cd2, cdEmpty1);
            Assert.AreNotEqual(cd3, cdEmpty1);
            Assert.AreNotEqual(cdWithField, classDefinition);
            Assert.AreNotEqual(cdEmpty1, classDefinition);
            Assert.AreNotEqual(classDefinition, null);
            Assert.AreNotEqual(classDefinition, "Another Class");
            Assert.AreNotEqual(0, cd1.GetHashCode());
        }

        public virtual void TestClassDef_getField_HigherThenSizeIndex()
        {
            classDefinition.GetField(classDefinition.GetFieldCount());
        }

        public virtual void TestClassDef_getField_negativeIndex()
        {
            classDefinition.GetField(-1);
        }

        [Test]
        public virtual void TestClassDef_getField_properIndex()
        {
            for (var i = 0; i < classDefinition.GetFieldCount(); i++)
            {
                var field = classDefinition.GetField(i);
                Assert.IsNotNull(field);
            }
        }

        [Test]
        public virtual void TestClassDef_getFieldClassId()
        {
            foreach (var fieldName in fieldNames)
            {
                var classId = classDefinition.GetFieldClassId(fieldName);
                Assert.AreEqual(0, classId);
            }
        }

        public virtual void TestClassDef_getFieldClassId_invalidField()
        {
            classDefinition.GetFieldClassId("The Invalid Field");
        }

        [Test]
        public virtual void TestClassDef_getFieldType()
        {
            foreach (var fieldName in fieldNames)
            {
                var fieldType = classDefinition.GetFieldType(fieldName);
                Assert.IsNotNull(fieldType);
            }
        }

        public virtual void TestClassDef_getFieldType_invalidField()
        {
            classDefinition.GetFieldType("The Invalid Field");
        }

        [Test]
        public virtual void TestClassDef_getter_setter()
        {
            var cd = (ClassDefinition) new ClassDefinitionBuilder(1, 2).Build();
            cd.SetVersionIfNotSet(3);
            cd.SetVersionIfNotSet(5);
            Assert.AreEqual(1, cd.GetFactoryId());
            Assert.AreEqual(2, cd.GetClassId());
            Assert.AreEqual(3, cd.GetVersion());
            Assert.AreEqual(3, classDefinition.GetFieldCount());
        }

        [Test]
        public virtual void TestClassDef_hasField()
        {
            for (var i = 0; i < classDefinition.GetFieldCount(); i++)
            {
                var fieldName = fieldNames[i];
                var hasField = classDefinition.HasField(fieldName);
                Assert.IsTrue(hasField);
            }
        }

        [Test]
        public virtual void TestClassDef_toString()
        {
            Assert.IsNotNull(classDefinition.ToString());
        }

        [Test]
        public virtual void TestFieldDef_equal_hashCode()
        {
            var fd0 = new FieldDefinition(0, "name", FieldType.Boolean);
            var fd0_1 = new FieldDefinition(0, "name", FieldType.Int);
            var fd1 = new FieldDefinition(1, "name", FieldType.Boolean);
            var fd2 = new FieldDefinition(0, "namex", FieldType.Boolean);
            Assert.AreNotEqual(fd0, fd0_1);
            Assert.AreNotEqual(fd0, fd1);
            Assert.AreNotEqual(fd0, fd2);
            Assert.AreNotEqual(fd0, null);
            Assert.AreNotEqual(fd0, "Another Class");
            Assert.AreNotEqual(0, fd0.GetHashCode());
        }

        [Test]
        public virtual void TestFieldDef_getter_setter()
        {
            var field0 = classDefinition.GetField(0);
            var field = classDefinition.GetField("f1");
            var fd = new FieldDefinition(9, "name", FieldType.Portable, 5, 6);
            var fd_nullName = new FieldDefinition(10, null, FieldType.Portable, 15, 16);
            Assert.AreEqual(field, field0);
            Assert.AreEqual(0, field.GetFactoryId());
            Assert.AreEqual(0, field.GetClassId());
            Assert.AreEqual(0, field.GetIndex());
            Assert.AreEqual("f1", field.GetName());
            Assert.AreEqual(FieldType.Byte, field.GetFieldType());
            Assert.AreEqual(5, fd.GetFactoryId());
            Assert.AreEqual(6, fd.GetClassId());
            Assert.AreEqual(9, fd.GetIndex());
            Assert.AreEqual("name", fd.GetName());
            Assert.AreEqual(FieldType.Portable, fd.GetFieldType());
            Assert.AreEqual(15, fd_nullName.GetFactoryId());
            Assert.AreEqual(16, fd_nullName.GetClassId());
            Assert.AreEqual(10, fd_nullName.GetIndex());
            Assert.AreEqual(null, fd_nullName.GetName());
            Assert.AreEqual(FieldType.Portable, fd_nullName.GetFieldType());
        }

        [Test]
        public virtual void TestFieldDef_toString()
        {
            Assert.IsNotNull(new FieldDefinition(0, "name", FieldType.Boolean).ToString());
        }
    }
}
// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using System;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Configuration.Binding;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Hazelcast.Tests.Configuration
{
    // this is the complete ConfigurationBinder test class from MS runtime source code
    // just to make sure our own binder does not break anything

    public class ConfigurationCollectionBinding
    {
        [Test]
        public void GetList()
        {
            var input = new Dictionary<string, string>
            {
                {"StringList:0", "val0"},
                {"StringList:1", "val1"},
                {"StringList:2", "val2"},
                {"StringList:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();
            var list = new List<string>();
            config.GetSection("StringList").HzBind(list);

            Assert.AreEqual(4, list.Count);

            Assert.AreEqual("val0", list[0]);
            Assert.AreEqual("val1", list[1]);
            Assert.AreEqual("val2", list[2]);
            Assert.AreEqual("valx", list[3]);
        }

        [Test]
        public void GetListNullValues()
        {
            var input = new Dictionary<string, string>
            {
                {"StringList:0", null},
                {"StringList:1", null},
                {"StringList:2", null},
                {"StringList:x", null}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();
            var list = new List<string>();
            config.GetSection("StringList").HzBind(list);

            Assert.IsEmpty(list);
        }

        [Test]
        [Ignore("Our binder throws on purpose.")]
        public void GetListInvalidValues()
        {
            var input = new Dictionary<string, string>
            {
                {"InvalidList:0", "true"},
                {"InvalidList:1", "invalid"},
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();
            var list = new List<bool>();
            config.GetSection("InvalidList").HzBind(list);

            //Assert.Single(list);
            Assert.AreEqual(list.Count, 1);
            Assert.True(list[0]);
        }

        [Test]
        public void BindList()
        {
            var input = new Dictionary<string, string>
            {
                {"StringList:0", "val0"},
                {"StringList:1", "val1"},
                {"StringList:2", "val2"},
                {"StringList:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var list = new List<string>();
            config.GetSection("StringList").HzBind(list);

            Assert.AreEqual(4, list.Count);

            Assert.AreEqual("val0", list[0]);
            Assert.AreEqual("val1", list[1]);
            Assert.AreEqual("val2", list[2]);
            Assert.AreEqual("valx", list[3]);
        }

        [Test]
        public void GetObjectList()
        {
            var input = new Dictionary<string, string>
            {
                {"ObjectList:0:Integer", "30"},
                {"ObjectList:1:Integer", "31"},
                {"ObjectList:2:Integer", "32"},
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new List<NestedOptions>();
            config.GetSection("ObjectList").HzBind(options);

            Assert.AreEqual(3, options.Count);

            Assert.AreEqual(30, options[0].Integer);
            Assert.AreEqual(31, options[1].Integer);
            Assert.AreEqual(32, options[2].Integer);
        }

        [Test]
        public void GetStringDictionary()
        {
            var input = new Dictionary<string, string>
            {
                {"StringDictionary:abc", "val_1"},
                {"StringDictionary:def", "val_2"},
                {"StringDictionary:ghi", "val_3"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new Dictionary<string, string>();
            config.GetSection("StringDictionary").HzBind(options);

            Assert.AreEqual(3, options.Count);

            Assert.AreEqual("val_1", options["abc"]);
            Assert.AreEqual("val_2", options["def"]);
            Assert.AreEqual("val_3", options["ghi"]);
        }

        [Test]
        public void GetEnumDictionary()
        {
            var input = new Dictionary<string, string>
            {
                {"EnumDictionary:abc", "val_1"},
                {"EnumDictionary:def", "val_2"},
                {"EnumDictionary:ghi", "val_3"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new Dictionary<KeyEnum, string>();
            config.GetSection("EnumDictionary").HzBind(options);

            Assert.AreEqual(3, options.Count);

            Assert.AreEqual("val_1", options[KeyEnum.abc]);
            Assert.AreEqual("val_2", options[KeyEnum.def]);
            Assert.AreEqual("val_3", options[KeyEnum.ghi]);
        }

        [Test]
        public void GetUintEnumDictionary()
        {
            var input = new Dictionary<string, string>
            {
                {"EnumDictionary:abc", "val_1"},
                {"EnumDictionary:def", "val_2"},
                {"EnumDictionary:ghi", "val_3"}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();
            var options = new Dictionary<KeyUintEnum, string>();
            config.GetSection("EnumDictionary").HzBind(options);
            Assert.AreEqual(3, options.Count);
            Assert.AreEqual("val_1", options[KeyUintEnum.abc]);
            Assert.AreEqual("val_2", options[KeyUintEnum.def]);
            Assert.AreEqual("val_3", options[KeyUintEnum.ghi]);
        }

        [Test]
        public void GetStringList()
        {
            var input = new Dictionary<string, string>
            {
                {"StringList:0", "val0"},
                {"StringList:1", "val1"},
                {"StringList:2", "val2"},
                {"StringList:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();
            var options = new OptionsWithLists();
            config.HzBind(options);

            var list = options.StringList;

            Assert.AreEqual(4, list.Count);

            Assert.AreEqual("val0", list[0]);
            Assert.AreEqual("val1", list[1]);
            Assert.AreEqual("val2", list[2]);
            Assert.AreEqual("valx", list[3]);
        }

        [Test]
        public void BindStringList()
        {
            var input = new Dictionary<string, string>
            {
                {"StringList:0", "val0"},
                {"StringList:1", "val1"},
                {"StringList:2", "val2"},
                {"StringList:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();
            var options = new OptionsWithLists();
            config.HzBind(options);

            var list = options.StringList;

            Assert.AreEqual(4, list.Count);

            Assert.AreEqual("val0", list[0]);
            Assert.AreEqual("val1", list[1]);
            Assert.AreEqual("val2", list[2]);
            Assert.AreEqual("valx", list[3]);
        }

        [Test]
        public void GetIntList()
        {
            var input = new Dictionary<string, string>
            {
                {"IntList:0", "42"},
                {"IntList:1", "43"},
                {"IntList:2", "44"},
                {"IntList:x", "45"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithLists();
            config.HzBind(options);

            var list = options.IntList;

            Assert.AreEqual(4, list.Count);

            Assert.AreEqual(42, list[0]);
            Assert.AreEqual(43, list[1]);
            Assert.AreEqual(44, list[2]);
            Assert.AreEqual(45, list[3]);
        }

        [Test]
        public void BindIntList()
        {
            var input = new Dictionary<string, string>
            {
                {"IntList:0", "42"},
                {"IntList:1", "43"},
                {"IntList:2", "44"},
                {"IntList:x", "45"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithLists();
            config.HzBind(options);

            var list = options.IntList;

            Assert.AreEqual(4, list.Count);

            Assert.AreEqual(42, list[0]);
            Assert.AreEqual(43, list[1]);
            Assert.AreEqual(44, list[2]);
            Assert.AreEqual(45, list[3]);
        }

        [Test]
        public void AlreadyInitializedListBinding()
        {
            var input = new Dictionary<string, string>
            {
                {"AlreadyInitializedList:0", "val0"},
                {"AlreadyInitializedList:1", "val1"},
                {"AlreadyInitializedList:2", "val2"},
                {"AlreadyInitializedList:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithLists();
            config.HzBind(options);

            var list = options.AlreadyInitializedList;

            Assert.AreEqual(5, list.Count);

            Assert.AreEqual("This was here before", list[0]);
            Assert.AreEqual("val0", list[1]);
            Assert.AreEqual("val1", list[2]);
            Assert.AreEqual("val2", list[3]);
            Assert.AreEqual("valx", list[4]);
        }

        [Test]
        public void AlreadyInitializedListInterfaceBinding()
        {
            var input = new Dictionary<string, string>
            {
                {"AlreadyInitializedListInterface:0", "val0"},
                {"AlreadyInitializedListInterface:1", "val1"},
                {"AlreadyInitializedListInterface:2", "val2"},
                {"AlreadyInitializedListInterface:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithLists();
            config.HzBind(options);

            var list = options.AlreadyInitializedListInterface;

            Assert.AreEqual(5, list.Count);

            Assert.AreEqual("This was here too", list[0]);
            Assert.AreEqual("val0", list[1]);
            Assert.AreEqual("val1", list[2]);
            Assert.AreEqual("val2", list[3]);
            Assert.AreEqual("valx", list[4]);
        }

        [Test]
        public void CustomListBinding()
        {
            var input = new Dictionary<string, string>
            {
                {"CustomList:0", "val0"},
                {"CustomList:1", "val1"},
                {"CustomList:2", "val2"},
                {"CustomList:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithLists();
            config.HzBind(options);

            var list = options.CustomList;

            Assert.AreEqual(4, list.Count);

            Assert.AreEqual("val0", list[0]);
            Assert.AreEqual("val1", list[1]);
            Assert.AreEqual("val2", list[2]);
            Assert.AreEqual("valx", list[3]);
        }

        [Test]
        public void ObjectListBinding()
        {
            var input = new Dictionary<string, string>
            {
                {"ObjectList:0:Integer", "30"},
                {"ObjectList:1:Integer", "31"},
                {"ObjectList:2:Integer", "32"},
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithLists();
            config.HzBind(options);

            Assert.AreEqual(3, options.ObjectList.Count);

            Assert.AreEqual(30, options.ObjectList[0].Integer);
            Assert.AreEqual(31, options.ObjectList[1].Integer);
            Assert.AreEqual(32, options.ObjectList[2].Integer);
        }

        [Test]
        public void NestedListsBinding()
        {
            var input = new Dictionary<string, string>
            {
                {"NestedLists:0:0", "val00"},
                {"NestedLists:0:1", "val01"},
                {"NestedLists:1:0", "val10"},
                {"NestedLists:1:1", "val11"},
                {"NestedLists:1:2", "val12"},
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithLists();
            config.HzBind(options);

            Assert.AreEqual(2, options.NestedLists.Count);
            Assert.AreEqual(2, options.NestedLists[0].Count);
            Assert.AreEqual(3, options.NestedLists[1].Count);

            Assert.AreEqual("val00", options.NestedLists[0][0]);
            Assert.AreEqual("val01", options.NestedLists[0][1]);
            Assert.AreEqual("val10", options.NestedLists[1][0]);
            Assert.AreEqual("val11", options.NestedLists[1][1]);
            Assert.AreEqual("val12", options.NestedLists[1][2]);
        }

        [Test]
        public void StringDictionaryBinding()
        {
            var input = new Dictionary<string, string>
            {
                {"StringDictionary:abc", "val_1"},
                {"StringDictionary:def", "val_2"},
                {"StringDictionary:ghi", "val_3"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithDictionary();
            config.HzBind(options);

            Assert.AreEqual(3, options.StringDictionary.Count);

            Assert.AreEqual("val_1", options.StringDictionary["abc"]);
            Assert.AreEqual("val_2", options.StringDictionary["def"]);
            Assert.AreEqual("val_3", options.StringDictionary["ghi"]);
        }

        [Test]
        public void AlreadyInitializedStringDictionaryBinding()
        {
            var input = new Dictionary<string, string>
            {
                {"AlreadyInitializedStringDictionaryInterface:abc", "val_1"},
                {"AlreadyInitializedStringDictionaryInterface:def", "val_2"},
                {"AlreadyInitializedStringDictionaryInterface:ghi", "val_3"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithDictionary();
            config.HzBind(options);

            Assert.NotNull(options.AlreadyInitializedStringDictionaryInterface);
            Assert.AreEqual(4, options.AlreadyInitializedStringDictionaryInterface.Count);

            Assert.AreEqual("This was already here", options.AlreadyInitializedStringDictionaryInterface["123"]);
            Assert.AreEqual("val_1", options.AlreadyInitializedStringDictionaryInterface["abc"]);
            Assert.AreEqual("val_2", options.AlreadyInitializedStringDictionaryInterface["def"]);
            Assert.AreEqual("val_3", options.AlreadyInitializedStringDictionaryInterface["ghi"]);
        }

        [Test]
        public void CanOverrideExistingDictionaryKey()
        {
            var input = new Dictionary<string, string>
            {
                {"abc", "override"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new Dictionary<string, string>
            {
                {"abc", "default"}
            };

            config.HzBind(options);

            var optionsCount = options.Count;

            Assert.AreEqual(1, optionsCount);
            Assert.AreEqual("override", options["abc"]);
        }

        [Test]
        public void IntDictionaryBinding()
        {
            var input = new Dictionary<string, string>
            {
                {"IntDictionary:abc", "42"},
                {"IntDictionary:def", "43"},
                {"IntDictionary:ghi", "44"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithDictionary();
            config.HzBind(options);

            Assert.AreEqual(3, options.IntDictionary.Count);

            Assert.AreEqual(42, options.IntDictionary["abc"]);
            Assert.AreEqual(43, options.IntDictionary["def"]);
            Assert.AreEqual(44, options.IntDictionary["ghi"]);
        }

        [Test]
        public void ObjectDictionary()
        {
            var input = new Dictionary<string, string>
            {
                {"ObjectDictionary:abc:Integer", "1"},
                {"ObjectDictionary:def:Integer", "2"},
                {"ObjectDictionary:ghi:Integer", "3"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithDictionary();
            config.HzBind(options);

            Assert.AreEqual(3, options.ObjectDictionary.Count);

            Assert.AreEqual(1, options.ObjectDictionary["abc"].Integer);
            Assert.AreEqual(2, options.ObjectDictionary["def"].Integer);
            Assert.AreEqual(3, options.ObjectDictionary["ghi"].Integer);
        }

        [Test]
        public void ListDictionary()
        {
            var input = new Dictionary<string, string>
            {
                {"ListDictionary:abc:0", "abc_0"},
                {"ListDictionary:abc:1", "abc_1"},
                {"ListDictionary:def:0", "def_0"},
                {"ListDictionary:def:1", "def_1"},
                {"ListDictionary:def:2", "def_2"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithDictionary();
            config.HzBind(options);

            Assert.AreEqual(2, options.ListDictionary.Count);
            Assert.AreEqual(2, options.ListDictionary["abc"].Count);
            Assert.AreEqual(3, options.ListDictionary["def"].Count);

            Assert.AreEqual("abc_0", options.ListDictionary["abc"][0]);
            Assert.AreEqual("abc_1", options.ListDictionary["abc"][1]);
            Assert.AreEqual("def_0", options.ListDictionary["def"][0]);
            Assert.AreEqual("def_1", options.ListDictionary["def"][1]);
            Assert.AreEqual("def_2", options.ListDictionary["def"][2]);
        }

        [Test]
        public void ListInNestedOptionBinding()
        {
            var input = new Dictionary<string, string>
            {
                {"ObjectList:0:ListInNestedOption:0", "00"},
                {"ObjectList:0:ListInNestedOption:1", "01"},
                {"ObjectList:1:ListInNestedOption:0", "10"},
                {"ObjectList:1:ListInNestedOption:1", "11"},
                {"ObjectList:1:ListInNestedOption:2", "12"},
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithLists();
            config.HzBind(options);

            Assert.AreEqual(2, options.ObjectList.Count);
            Assert.AreEqual(2, options.ObjectList[0].ListInNestedOption.Count);
            Assert.AreEqual(3, options.ObjectList[1].ListInNestedOption.Count);

            Assert.AreEqual("00", options.ObjectList[0].ListInNestedOption[0]);
            Assert.AreEqual("01", options.ObjectList[0].ListInNestedOption[1]);
            Assert.AreEqual("10", options.ObjectList[1].ListInNestedOption[0]);
            Assert.AreEqual("11", options.ObjectList[1].ListInNestedOption[1]);
            Assert.AreEqual("12", options.ObjectList[1].ListInNestedOption[2]);
        }

        [Test]
        public void NonStringKeyDictionaryBinding()
        {
            var input = new Dictionary<string, string>
            {
                {"NonStringKeyDictionary:abc", "val_1"},
                {"NonStringKeyDictionary:def", "val_2"},
                {"NonStringKeyDictionary:ghi", "val_3"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithDictionary();
            config.HzBind(options);

            Assert.IsEmpty(options.NonStringKeyDictionary);
        }

        [Test]
        public void GetStringArray()
        {
            var input = new Dictionary<string, string>
            {
                {"StringArray:0", "val0"},
                {"StringArray:1", "val1"},
                {"StringArray:2", "val2"},
                {"StringArray:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithArrays();
            config.HzBind(options);

            var array = options.StringArray;

            Assert.AreEqual(4, array.Length);

            Assert.AreEqual("val0", array[0]);
            Assert.AreEqual("val1", array[1]);
            Assert.AreEqual("val2", array[2]);
            Assert.AreEqual("valx", array[3]);
        }


        [Test]
        public void BindStringArray()
        {
            var input = new Dictionary<string, string>
            {
                {"StringArray:0", "val0"},
                {"StringArray:1", "val1"},
                {"StringArray:2", "val2"},
                {"StringArray:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var instance = new OptionsWithArrays();
            config.HzBind(instance);

            var array = instance.StringArray;

            Assert.AreEqual(4, array.Length);

            Assert.AreEqual("val0", array[0]);
            Assert.AreEqual("val1", array[1]);
            Assert.AreEqual("val2", array[2]);
            Assert.AreEqual("valx", array[3]);
        }

        [Test]
        public void GetAlreadyInitializedArray()
        {
            var input = new Dictionary<string, string>
            {
                {"AlreadyInitializedArray:0", "val0"},
                {"AlreadyInitializedArray:1", "val1"},
                {"AlreadyInitializedArray:2", "val2"},
                {"AlreadyInitializedArray:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithArrays();
            config.HzBind(options);
            var array = options.AlreadyInitializedArray;

            Assert.AreEqual(7, array.Length);

            Assert.AreEqual(OptionsWithArrays.InitialValue, array[0]);
            Assert.Null(array[1]);
            Assert.Null(array[2]);
            Assert.AreEqual("val0", array[3]);
            Assert.AreEqual("val1", array[4]);
            Assert.AreEqual("val2", array[5]);
            Assert.AreEqual("valx", array[6]);
        }

        [Test]
        public void BindAlreadyInitializedArray()
        {
            var input = new Dictionary<string, string>
            {
                {"AlreadyInitializedArray:0", "val0"},
                {"AlreadyInitializedArray:1", "val1"},
                {"AlreadyInitializedArray:2", "val2"},
                {"AlreadyInitializedArray:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new OptionsWithArrays();
            config.HzBind(options);

            var array = options.AlreadyInitializedArray;

            Assert.AreEqual(7, array.Length);

            Assert.AreEqual(OptionsWithArrays.InitialValue, array[0]);
            Assert.Null(array[1]);
            Assert.Null(array[2]);
            Assert.AreEqual("val0", array[3]);
            Assert.AreEqual("val1", array[4]);
            Assert.AreEqual("val2", array[5]);
            Assert.AreEqual("valx", array[6]);
        }

        [Test]
        public void ArrayInNestedOptionBinding()
        {
            var input = new Dictionary<string, string>
            {
                {"ObjectArray:0:ArrayInNestedOption:0", "0"},
                {"ObjectArray:0:ArrayInNestedOption:1", "1"},
                {"ObjectArray:1:ArrayInNestedOption:0", "10"},
                {"ObjectArray:1:ArrayInNestedOption:1", "11"},
                {"ObjectArray:1:ArrayInNestedOption:2", "12"},
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();
            var options = new OptionsWithArrays();
            config.HzBind(options);

            Assert.AreEqual(2, options.ObjectArray.Length);
            Assert.AreEqual(2, options.ObjectArray[0].ArrayInNestedOption.Length);
            Assert.AreEqual(3, options.ObjectArray[1].ArrayInNestedOption.Length);

            Assert.AreEqual(0, options.ObjectArray[0].ArrayInNestedOption[0]);
            Assert.AreEqual(1, options.ObjectArray[0].ArrayInNestedOption[1]);
            Assert.AreEqual(10, options.ObjectArray[1].ArrayInNestedOption[0]);
            Assert.AreEqual(11, options.ObjectArray[1].ArrayInNestedOption[1]);
            Assert.AreEqual(12, options.ObjectArray[1].ArrayInNestedOption[2]);
        }

        [Test]
        public void UnsupportedMultidimensionalArrays()
        {
            var input = new Dictionary<string, string>
            {
                {"DimensionalArray:0:0", "a"},
                {"DimensionalArray:0:1", "b"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();
            var options = new OptionsWithArrays();

            var exception = Assert.Throws<InvalidOperationException>(
                () => config.HzBind(options));
            //Assert.AreEqual(
            //    Resources.FormatError_UnsupportedMultidimensionalArray(typeof(string[,])),
            //    exception.Message);
        }

        [Test]
        public void JaggedArrayBinding()
        {
            var input = new Dictionary<string, string>
            {
                {"JaggedArray:0:0", "00"},
                {"JaggedArray:0:1", "01"},
                {"JaggedArray:1:0", "10"},
                {"JaggedArray:1:1", "11"},
                {"JaggedArray:1:2", "12"},
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();
            var options = new OptionsWithArrays();
            config.HzBind(options);

            Assert.AreEqual(2, options.JaggedArray.Length);
            Assert.AreEqual(2, options.JaggedArray[0].Length);
            Assert.AreEqual(3, options.JaggedArray[1].Length);

            Assert.AreEqual("00", options.JaggedArray[0][0]);
            Assert.AreEqual("01", options.JaggedArray[0][1]);
            Assert.AreEqual("10", options.JaggedArray[1][0]);
            Assert.AreEqual("11", options.JaggedArray[1][1]);
            Assert.AreEqual("12", options.JaggedArray[1][2]);
        }

        [Test]
        public void CanBindUninitializedIEnumerable()
        {
            var input = new Dictionary<string, string>
            {
                {"IEnumerable:0", "val0"},
                {"IEnumerable:1", "val1"},
                {"IEnumerable:2", "val2"},
                {"IEnumerable:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new UnintializedCollectionsOptions();
            config.HzBind(options);

            var array = options.IEnumerable.ToArray();

            Assert.AreEqual(4, array.Length);

            Assert.AreEqual("val0", array[0]);
            Assert.AreEqual("val1", array[1]);
            Assert.AreEqual("val2", array[2]);
            Assert.AreEqual("valx", array[3]);
        }

        [Test]
        public void CanBindUninitializedICollection()
        {
            var input = new Dictionary<string, string>
            {
                {"ICollection:0", "val0"},
                {"ICollection:1", "val1"},
                {"ICollection:2", "val2"},
                {"ICollection:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new UnintializedCollectionsOptions();
            config.HzBind(options);

            var array = options.ICollection.ToArray();

            Assert.AreEqual(4, array.Length);

            Assert.AreEqual("val0", array[0]);
            Assert.AreEqual("val1", array[1]);
            Assert.AreEqual("val2", array[2]);
            Assert.AreEqual("valx", array[3]);
        }

        [Test]
        public void CanBindUninitializedIReadOnlyCollection()
        {
            var input = new Dictionary<string, string>
            {
                {"IReadOnlyCollection:0", "val0"},
                {"IReadOnlyCollection:1", "val1"},
                {"IReadOnlyCollection:2", "val2"},
                {"IReadOnlyCollection:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new UnintializedCollectionsOptions();
            config.HzBind(options);

            var array = options.IReadOnlyCollection.ToArray();

            Assert.AreEqual(4, array.Length);

            Assert.AreEqual("val0", array[0]);
            Assert.AreEqual("val1", array[1]);
            Assert.AreEqual("val2", array[2]);
            Assert.AreEqual("valx", array[3]);
        }

        [Test]
        public void CanBindUninitializedIReadOnlyList()
        {
            var input = new Dictionary<string, string>
            {
                {"IReadOnlyList:0", "val0"},
                {"IReadOnlyList:1", "val1"},
                {"IReadOnlyList:2", "val2"},
                {"IReadOnlyList:x", "valx"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new UnintializedCollectionsOptions();
            config.HzBind(options);

            var array = options.IReadOnlyList.ToArray();

            Assert.AreEqual(4, array.Length);

            Assert.AreEqual("val0", array[0]);
            Assert.AreEqual("val1", array[1]);
            Assert.AreEqual("val2", array[2]);
            Assert.AreEqual("valx", array[3]);
        }

        [Test]
        public void CanBindUninitializedIDictionary()
        {
            var input = new Dictionary<string, string>
            {
                {"IDictionary:abc", "val_1"},
                {"IDictionary:def", "val_2"},
                {"IDictionary:ghi", "val_3"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new UnintializedCollectionsOptions();
            config.HzBind(options);

            Assert.AreEqual(3, options.IDictionary.Count);

            Assert.AreEqual("val_1", options.IDictionary["abc"]);
            Assert.AreEqual("val_2", options.IDictionary["def"]);
            Assert.AreEqual("val_3", options.IDictionary["ghi"]);
        }

        [Test]
        public void CanBindUninitializedIReadOnlyDictionary()
        {
            var input = new Dictionary<string, string>
            {
                {"IReadOnlyDictionary:abc", "val_1"},
                {"IReadOnlyDictionary:def", "val_2"},
                {"IReadOnlyDictionary:ghi", "val_3"}
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(input);
            var config = configurationBuilder.Build();

            var options = new UnintializedCollectionsOptions();
            config.HzBind(options);

            Assert.AreEqual(3, options.IReadOnlyDictionary.Count);

            Assert.AreEqual("val_1", options.IReadOnlyDictionary["abc"]);
            Assert.AreEqual("val_2", options.IReadOnlyDictionary["def"]);
            Assert.AreEqual("val_3", options.IReadOnlyDictionary["ghi"]);
        }

        private class UnintializedCollectionsOptions
        {
            public IEnumerable<string> IEnumerable { get; set; }
            public IDictionary<string, string> IDictionary { get; set; }
            public ICollection<string> ICollection { get; set; }
            public IList<string> IList { get; set; }
            public IReadOnlyCollection<string> IReadOnlyCollection { get; set; }
            public IReadOnlyList<string> IReadOnlyList { get; set; }
            public IReadOnlyDictionary<string, string> IReadOnlyDictionary { get; set; }
        }

        private class CustomList : List<string>
        {
            // Add an overload, just to make sure binding picks the right Add method
            public void Add(string a, string b)
            {
            }
        }

        private class CustomDictionary<T> : Dictionary<string, T>
        {
        }

        private class NestedOptions
        {
            public int Integer { get; set; }

            public List<string> ListInNestedOption { get; set; }

            public int[] ArrayInNestedOption { get; set; }
        }

        private enum KeyEnum
        {
            abc,
            def,
            ghi
        }

        private enum KeyUintEnum : uint
        {
            abc,
            def,
            ghi
        }

        private class OptionsWithArrays
        {
            public const string InitialValue = "This was here before";

            public OptionsWithArrays()
            {
                AlreadyInitializedArray = new string[] { InitialValue, null, null };
            }

            public string[] AlreadyInitializedArray { get; set; }

            public string[] StringArray { get; set; }

            // this should throw becase we do not support multidimensional arrays
            public string[,] DimensionalArray { get; set; }

            public string[][] JaggedArray { get; set; }

            public NestedOptions[] ObjectArray { get; set; }
        }

        private class OptionsWithLists
        {
            public OptionsWithLists()
            {
                AlreadyInitializedList = new List<string>
                {
                    "This was here before"
                };
                AlreadyInitializedListInterface = new List<string>
                {
                    "This was here too"
                };
            }

            public CustomList CustomList { get; set; }

            public List<string> StringList { get; set; }

            public List<int> IntList { get; set; }

            // This cannot be initialized because we cannot
            // activate an interface
            public IList<string> StringListInterface { get; set; }

            public List<List<string>> NestedLists { get; set; }

            public List<string> AlreadyInitializedList { get; set; }

            public List<NestedOptions> ObjectList { get; set; }

            public IList<string> AlreadyInitializedListInterface { get; set; }
        }

        private class OptionsWithDictionary
        {
            public OptionsWithDictionary()
            {
                AlreadyInitializedStringDictionaryInterface = new Dictionary<string, string>
                {
                    ["123"] = "This was already here"
                };
            }

            public Dictionary<string, int> IntDictionary { get; set; }

            public Dictionary<string, string> StringDictionary { get; set; }

            public Dictionary<string, NestedOptions> ObjectDictionary { get; set; }

            public Dictionary<string, List<string>> ListDictionary { get; set; }

            public Dictionary<NestedOptions, string> NonStringKeyDictionary { get; set; }

            // This cannot be initialized because we cannot
            // activate an interface
            public IDictionary<string, string> StringDictionaryInterface { get; set; }

            public IDictionary<string, string> AlreadyInitializedStringDictionaryInterface { get; set; }
        }
    }
}

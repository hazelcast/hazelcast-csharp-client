﻿// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using NUnit.Framework;
using Address = Hazelcast.IO.Address;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class EntryEventTest
    {
        private ISerializationService _serializationService;
        private DataAwareEntryEvent<string, int?> _dataAwareEntryEvent;
        private IData _dataString;
        private IData _dataInt;
        private string _testString;
        private int _testInt;

        [SetUp]
        public void Init()
        {
            _serializationService = new SerializationServiceBuilder().Build();
            _testString = "Test String";
            _dataString = _serializationService.ToData(_testString);
            _testInt = 666;
            _dataInt = _serializationService.ToData(_testInt);

            var member = new Member(new Address("localhost", 5701), "");

            _dataAwareEntryEvent = new DataAwareEntryEvent<string, int?>("source", member, EntryEventType.Added,
                _dataString, _dataInt, _dataInt, null, _serializationService);
        }

        [TearDown]
        public void Destroy()
        {
            _serializationService.Destroy();
        }

        [Test]
        public void GetLazy()
        {
            Assert.AreEqual(_dataAwareEntryEvent.GetKey(), _testString);
            Assert.AreEqual(_dataAwareEntryEvent.GetValue(), _testInt);
            Assert.AreEqual(_dataAwareEntryEvent.GetOldValue(), _testInt);
            Assert.Null(_dataAwareEntryEvent.GetMergingValue());
        }
    }
}
/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

﻿using System;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Test.Serialization
{
    class SerializationTest
    {

        static void Mainzz(string[] args)
        {
            var clientConfig = new ClientConfig();
            clientConfig.GetNetworkConfig().AddAddress("127.0.0.1");
            clientConfig.GetSerializationConfig().AddDataSerializableFactory(1, new MyDataSerializableFactory());


            IHazelcastInstance client = HazelcastClient.NewHazelcastClient(clientConfig);
            //All cluster operations that you can do with ordinary HazelcastInstance
            IMap<string, DataSerializableType> map = client.GetMap<string, DataSerializableType>("imap");

            ISerializationService service = ((HazelcastClientProxy)client).GetSerializationService();

            object obj=new DataSerializableType(1000,1000);
            long start = Clock.CurrentTimeMillis();
            var data = service.ToData(obj);

            var dataSerializableType = service.ToObject<DataSerializableType>(data);

            long diff = Clock.CurrentTimeMillis()- start;

            Console.WriteLine("Serialization time:"+diff);

            Console.ReadKey();
        }
    }
}

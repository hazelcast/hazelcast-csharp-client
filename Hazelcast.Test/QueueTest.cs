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

﻿
using System;

using Hazelcast.Core;
using Hazelcast.Client;
using Hazelcast.Config;

namespace HazelcastTest
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    internal class Program
    {
        private static void Main11(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.class", "console");

            Console.WriteLine("INIT APP");
            ClientConfig clientConfig = new ClientConfig();
            clientConfig.SetNetworkConfig(new ClientNetworkConfig().AddAddress(("127.0.0.1:5701")));
            IHazelcastInstance client = HazelcastClient.NewHazelcastClient(clientConfig);
            var queue = client.GetQueue<byte[]>("a queue");

            var watch = Stopwatch.StartNew();
            var task = Task.Factory.StartNew(
                () =>
                {
                    byte[] loop = queue.Poll(15, TimeUnit.SECONDS);
                    while (loop != null && loop.Length != 0)
                    {
                        loop = queue.Poll(15,TimeUnit.SECONDS);
                        if (loop != null)
                        {
                            var a=new [] {loop[0], loop[1]};
                            Console.WriteLine(BitConverter.ToInt16(a,0));
                        }
                    }
                });

            const int byteLen = 1024 * 500;
            byte[] buf = new byte[byteLen];

            for (int i = 0; i < 1000; ++i)
            {
                byte[] bytes = BitConverter.GetBytes((short)i);
                Array.Copy(bytes,buf,bytes.Length);
                queue.Put(buf);
            }

            queue.Put(new byte[0]);

            task.Wait();
            var elapsed = watch.Elapsed;
            Console.WriteLine("Time: {0}", elapsed.TotalMilliseconds);

            client.Shutdown();
        }

        static void Main111(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            ClientConfig clientConfig = new ClientConfig();
            clientConfig.SetNetworkConfig(new ClientNetworkConfig().AddAddress(("127.0.0.1:5701")));
            IHazelcastInstance instance = HazelcastClient.NewHazelcastClient(clientConfig);

            var locker = instance.GetLock("processLock");
            Console.WriteLine("Waiting for lock");
            locker.Lock();
            Console.WriteLine("Locked");
            Console.ReadLine();
            locker.Unlock();

            instance.Shutdown();
        }

    }
}

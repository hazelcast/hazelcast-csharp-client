// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Test
{
    public class ClientQueuePerformanceTest
    {
        internal const int ThreadCount = 400;
        internal static readonly AtomicLong totalOffer = new AtomicLong();

        internal static readonly AtomicLong totalPoll = new AtomicLong();

        internal static readonly AtomicLong totalPeek = new AtomicLong();

        internal static readonly byte[] Value = new byte[1000];

        internal static IQueue<object> q;

        //    static HazelcastInstance second;
        /// <exception cref="System.Exception"></exception>
        public static void Mainwq(string[] args)
        {
            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            q = client.GetQueue<object>("test");
            Test1();
        }

        public static void Run1()
        {
            var randomGen = new Random((int) DateTime.Now.Ticks);
            while (true)
            {
                var random = randomGen.Next(0, 100);
                if (random > 54)
                {
                    q.Poll();
                    totalPoll.IncrementAndGet();
                }
                else
                {
                    if (random > 4)
                    {
                        q.Offer(Value);
                        totalOffer.IncrementAndGet();
                    }
                    else
                    {
                        q.Peek();
                        totalPeek.IncrementAndGet();
                    }
                }
            }
        }


        public static void Run2()
        {
            while (true)
            {
                try
                {
                    var size = q.Count;
                    if (size > 50000)
                    {
                        Console.Error.WriteLine("cleaning a little");
                        for (var i = 0; i < 20000; i++)
                        {
                            q.Poll();
                            totalPoll.IncrementAndGet();
                        }
                        Thread.Sleep(2*1000);
                    }
                    else
                    {
                        Thread.Sleep(10*1000);
                    }
                }
                catch
                {
                }
            }
        }

        /// <exception cref="System.Exception"></exception>
        public static void Test1()
        {
            for (var i = 0; i < ThreadCount; i++)
            {
                var t = new Thread(Run1);
                t.Start();
            }

            new Thread(Run2).Start();

            while (true)
            {
                var sleepTime = 10;
                Thread.Sleep(sleepTime*1000);
                var totalOfferVal = totalOffer.GetAndSet(0);
                var totalPollVal = totalPoll.GetAndSet(0);
                var totalPeekVal = totalPeek.GetAndSet(0);
                Console.Error.WriteLine(
                    "_______________________________________________________________________________________");
                Console.Error.WriteLine(" offer: " + totalOfferVal + ",\t poll: " + totalPollVal + ",\t peek: " +
                                        totalPeekVal);
                Console.Error.WriteLine(" size: " + q.Count + " \t speed: " +
                                        ((totalOfferVal + totalPollVal + totalPeekVal)/sleepTime));
                Console.Error.WriteLine(
                    "---------------------------------------------------------------------------------------");
                Console.Error.WriteLine(string.Empty);
            }
        }
    }
}
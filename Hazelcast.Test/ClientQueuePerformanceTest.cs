using System;
using System.Threading;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Test
{
    public class ClientQueuePerformanceTest
    {
        internal static readonly AtomicLong totalOffer = new AtomicLong();

        internal static readonly AtomicLong totalPoll = new AtomicLong();

        internal static readonly AtomicLong totalPeek = new AtomicLong();

        internal const int ThreadCount = 400;

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

        /// <exception cref="System.Exception"></exception>
        public static void Test1()
        {
            for (int i = 0; i < ThreadCount; i++)
            {
                Thread t = new Thread(Run1);
                t.Start();
            }
            
            new Thread(Run2).Start();

            while (true)
            {
                int sleepTime = 10;
                Thread.Sleep(sleepTime*1000);
                long totalOfferVal = totalOffer.GetAndSet(0);
                long totalPollVal = totalPoll.GetAndSet(0);
                long totalPeekVal = totalPeek.GetAndSet(0);
                System.Console.Error.WriteLine(
                    "_______________________________________________________________________________________");
                System.Console.Error.WriteLine(" offer: " + totalOfferVal + ",\t poll: " + totalPollVal + ",\t peek: " +
                                               totalPeekVal);
                System.Console.Error.WriteLine(" size: " + q.Count + " \t speed: " +
                                               ((totalOfferVal + totalPollVal + totalPeekVal)/sleepTime));
                System.Console.Error.WriteLine(
                    "---------------------------------------------------------------------------------------");
                System.Console.Error.WriteLine(string.Empty);
            }
        }

        public static void Run1()
        {
            var randomGen = new Random((int) DateTime.Now.Ticks);
            while (true)
            {
                int random = randomGen.Next(0, 100);
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
                    int size = q.Count;
                    if (size > 50000)
                    {
                        System.Console.Error.WriteLine("cleaning a little");
                        for (int i = 0; i < 20000; i++)
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
                catch (Exception e)
                {
                }
            }
        }
    }
}

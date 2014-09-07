using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Test
{
    public class Stats
    {
        public long gets;
        public long puts;
        public long removes;
        public long exceptions;

        public Stats GetAndReset()
        {
            long putsNow = Interlocked.Exchange(ref puts, 0);
            long getsNow = Interlocked.Exchange(ref gets, 0);
            long removesNow = Interlocked.Exchange(ref removes, 0);
            long exceptionsNow = Interlocked.Exchange(ref exceptions, 0);
            var newOne = new Stats();
            Interlocked.Exchange(ref newOne.puts, putsNow);
            Interlocked.Exchange(ref newOne.gets, getsNow);
            Interlocked.Exchange(ref newOne.removes, removesNow);
            Interlocked.Exchange(ref newOne.exceptions, exceptionsNow);
            return newOne;
        }

        public long Total()
        {
            return //
                Interlocked.Read(ref gets) +
                Interlocked.Read(ref puts) +
                Interlocked.Read(ref removes);
        }

        public override string ToString()
        {
            return "total= " + Total() + ", gets:" + Interlocked.Read(ref gets) + ", puts: " +
                   Interlocked.Read(ref puts) + ", removes:" + Interlocked.Read(ref removes) + ", exceptions:" + Interlocked.Read(ref exceptions);
        }
    }

    internal class SimpleMapTestFromClient
    {
        public static int THREAD_COUNT = 100;
        public static int ENTRY_COUNT = 10*1000;
        public static int VALUE_SIZE = 1000;
        public static int STATS_SECONDS = 10;
        public static int GET_PERCENTAGE = 40;
        public static int PUT_PERCENTAGE = 40;

        private static IHazelcastInstance hazelcast;

        private static Stats stats;

        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var clientConfig = new ClientConfig();
            clientConfig.GetNetworkConfig().AddAddress("127.0.0.1");
            //clientConfig.GetNetworkConfig().SetConnectionAttemptLimit(1000);
            hazelcast = HazelcastClient.NewHazelcastClient(clientConfig);

            Console.WriteLine("Client Ready to go");

            stats = new Stats();
            //Thread.Sleep(100000);
            if (args != null && args.Length > 0)
            {
                foreach (string _arg in  args)
                {
                    string arg = _arg.Trim();
                    //if (arg.startsWith("t")) {
                    //    THREAD_COUNT = Integer.parseInt(arg.substring(1));
                    //} else if (arg.startsWith("c")) {
                    //    ENTRY_COUNT = Integer.parseInt(arg.substring(1));
                    //} else if (arg.startsWith("v")) {
                    //    VALUE_SIZE = Integer.parseInt(arg.substring(1));
                    //} else if (arg.startsWith("g")) {
                    //    GET_PERCENTAGE = Integer.parseInt(arg.substring(1));
                    //} else if (arg.startsWith("p")) {
                    //    PUT_PERCENTAGE = Integer.parseInt(arg.substring(1));
                    //}
                }
            }
            else
            {
                Console.WriteLine("Help: sh test.sh t200 v130 p10 g85 ");
                Console.WriteLine("    // means 200 threads, value-size 130 bytes, 10% put, 85% get");
                Console.WriteLine("");
            }
            Console.WriteLine("Starting Test with ");
            Console.WriteLine("      Thread Count: " + THREAD_COUNT);
            Console.WriteLine("       Entry Count: " + ENTRY_COUNT);
            Console.WriteLine("        Value Size: " + VALUE_SIZE);
            Console.WriteLine("    Get Percentage: " + GET_PERCENTAGE);
            Console.WriteLine("    Put Percentage: " + PUT_PERCENTAGE);
            Console.WriteLine(" Remove Percentage: " + (100 - (PUT_PERCENTAGE + GET_PERCENTAGE)));


            var tasks = new List<Task>();
            for (int i = 0; i < THREAD_COUNT; i++)
            {
                var t = new Task(()=> HzTask(hazelcast),TaskCreationOptions.LongRunning);
                
                t.Start();

                tasks.Add(t);
            }

            var tm = new Thread(StatDisplayTask);
            tm.Start();

            Task.WaitAll(tasks.ToArray());
            tm.Abort();
            Console.WriteLine("--THE END--");
            //Task.Factory.StartNew(StatDisplayTask);

            //startNew.Wait();

            //StatDisplayTask();

            //Task.Factory.Scheduler.MaximumConcurrencyLevel = THREAD_COUNT;

            Console.ReadKey();
        }

        public static void HzTask(IHazelcastInstance hz)
        {
            try
            {
                var random = new Random();
                IMap<string, byte[]> map = hz.GetMap<String, byte[]>("default");
                while (true)
                {
                    try
                    {
                        int key = random.Next(0, ENTRY_COUNT);
                        int operation = random.Next(0, 100);
                        if (operation < GET_PERCENTAGE)
                        {
                            //                            long start = Clock.currentTimeMillis();
                            map.Get(key.ToString());
                            //                            System.out.println("Get takes " + (Clock.currentTimeMillis() - start) + " ms" );
                            Interlocked.Increment(ref stats.gets);
                        }
                        else if (operation < GET_PERCENTAGE + PUT_PERCENTAGE)
                        {
                            map.Put(key.ToString(), new byte[VALUE_SIZE]);
                            Interlocked.Increment(ref stats.puts);
                        }
                        else
                        {
                            map.Remove(key.ToString());
                            Interlocked.Increment(ref stats.removes);
                        }
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref stats.exceptions);
                        //Console.WriteLine(ex);
                        //Console.ReadKey();
                        //break;
                    }

                }

            }
            catch (Exception e)
            {
                //Console.WriteLine(e.StackTrace);
            }

        }

        public static void StatDisplayTask()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(STATS_SECONDS*1000);
                    Console.WriteLine("cluster size:" + hazelcast.GetCluster().GetMembers().Count);
                    Stats currentStats = stats.GetAndReset();
                    Console.WriteLine(currentStats.ToString());
                    Console.WriteLine("Operations per Second : " + currentStats.Total()/STATS_SECONDS);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
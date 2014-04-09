
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
        private static void Main(string[] args)
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
    }
}

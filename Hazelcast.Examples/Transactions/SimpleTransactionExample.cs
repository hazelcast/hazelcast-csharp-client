using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Transaction;

namespace Hazelcast.Examples.Transactions
{
    class SimpleTransactionExample
    {
        static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var options = new TransactionOptions();
            options.SetTransactionType(TransactionOptions.TransactionType.OnePhase);
            var ctx = client.NewTransactionContext(options);
            ctx.BeginTransaction();
            try
            {
                var txMap = ctx.GetMap<string, string>("txn-map");
                txMap.Put("foo", "bar");
                ctx.CommitTransaction();
            }
            catch
            {
                ctx.RollbackTransaction();
            }

            var map = client.GetMap<string, string>("txn-map");
            Console.WriteLine("Value of foo is " + map.Get("key"));
            map.Destroy();
            client.Shutdown();
        }
    }
}

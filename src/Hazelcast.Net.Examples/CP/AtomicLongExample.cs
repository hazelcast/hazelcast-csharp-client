using System;
using System.Threading.Tasks;

// ReSharper disable LocalizableElement

namespace Hazelcast.Examples.CP
{
    public class AtomicLongExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get the CP AtomicLong from the cluster
            await using var along = await client.CPSubsystem.GetAtomicLongAsync($"atomiclong-example-{Guid.NewGuid()}");
            Console.WriteLine($"Initial value: {await along.GetAsync()}");

            await along.SetAsync(10);
            Console.WriteLine($"Value after set: {await along.GetAsync()}");

            var previous = await along.IncrementAndGetAsync();
            Console.WriteLine($"Value after increment: {await along.GetAsync()}, previous: {previous}");

            previous = await along.DecrementAndGetAsync();
            Console.WriteLine($"Value after decrement: {await along.GetAsync()}, previous: {previous}");

            previous = await along.AddAndGetAsync(5);
            Console.WriteLine($"Value after add: {await along.GetAsync()}, previous: {previous}");

            previous = await along.GetAndSetAsync(100);
            Console.WriteLine($"Value after get&set: {await along.GetAsync()}, previous: {previous}");

            // destroy the AtomicLong
            await along.DestroyAsync();
        }
    }
}

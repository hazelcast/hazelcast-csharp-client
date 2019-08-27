using BenchmarkDotNet.Running;

namespace Hazelcast.Benchmarks
{
    class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}

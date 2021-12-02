using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Benchmarks
{
    public class BasicMapOperation
    {
        private IHazelcastClient _client;
        const string _mapName = "highWay";
        private string _key = "dearMama";
        private string _value = "Ain't a woman alive that could take my mama's place";

        //They does not work inline process. To overcome that
        //seperate the class into a different project.
        //details : https://benchmarkdotnet.org/articles/features/etwprofiler.html
        //[NativeMemoryProfiler]
        //[MemoryDiagnoser]
        //[HardwareCounters(HardwareCounter.TotalCycles, HardwareCounter.BranchInstructions)]
        public BasicMapOperation()
        {

        }

        private string[] GenerateWords(int count, int minLength = 5, int maxLength = 10)
        {
            string[] words = new string[count];

            for (int i = 0; i < count; i++)
            {
                string word = GenerateWord(minLength, maxLength);
                words[i] = word;
            }

            return words;
        }


        [GlobalSetup]
        public void Setup()
        {
            var options = new HazelcastOptionsBuilder();

            var buildedOpt = options
                  .With(opt => opt.Networking.Addresses.Add("127.0.0.1:5701"))
                  .With("Logging:LogLevel:System", "None")
                  .With("Logging:LogLevel:Microsoft", "None")
                  .With("Logging:LogLevel:Hazelcast", "Debug")
                  .With((conf, opt) => { opt.LoggerFactory.Creator = () => LoggerFactory.Create(loggerConfig => loggerConfig.SetMinimumLevel(LogLevel.Debug)); })
                  .Build();

            _client = HazelcastClientFactory.StartNewClientAsync(buildedOpt).Result;
        }

        /// <summary>
        /// Generates random letter gorups based on ascii code
        /// </summary>
        /// <param name="minLength"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        private string GenerateWord(int minLength, int maxLength)
        {
            int letterStart = 97;
            int letterEnd = 122;
            var rnd = new Random();

            int length = rnd.Next(minLength, maxLength);

            char[] letters = new char[length];

            for (int i = 0; i < length; i++)
            {
                int ch = rnd.Next(letterStart, letterEnd);
                letters[i] = (char)ch;
            }

            return new string(letters);
        }

        [Benchmark]
        public async Task DoPut()
        {
            var map = await _client.GetMapAsync<string, string>(_mapName);
            await map.PutAsync(_key, _value);
        }

        [Benchmark]
        public async Task DoGet()
        {
            var map = await _client.GetMapAsync<string, string>(_mapName);
            var readValue = await map.GetAsync(_key);
        }

        [GlobalCleanup]
        public void TearDown()
        {
            _client.DisposeAsync();
        }
    }
}

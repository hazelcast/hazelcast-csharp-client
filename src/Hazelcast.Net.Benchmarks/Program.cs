// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.IO;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

#pragma warning disable

namespace Hazelcast.Benchmarks
{
    /// <summary>
    /// Represents the benchmarks launcher.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main.
        /// </summary>
        /// <param name="args">Arguments.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Hazelcast.Net Benchmarks");
            if (args.Length < 1)
            {
                Console.WriteLine("usage: hb.exe <benchmark> [<args>]");
                return;
            }

            var typeName = typeof (Program).FullName?.Replace(".Program", "." + args[0]);
            var type = typeName == null ? null: Type.GetType(typeName);
            if (type == null)
            {
                Console.WriteLine($"error: type '{typeName}' not found.");
                return;
            }

            var exePath = typeof (Program).Assembly.Location;
            var slnPath = Path.GetFullPath(exePath);
            while (!File.Exists(Path.Combine(slnPath, "Hazelcast.Net.sln")))
                slnPath = Path.GetFullPath(Path.Combine(slnPath, ".."));
            var artPath = Path.GetFullPath(Path.Combine(slnPath, "temp/benchmarkDotNet"));

            if (!Directory.Exists(artPath)) Directory.CreateDirectory(artPath);

            Console.WriteLine($"Writing to {artPath}");

            var config = DefaultConfig.Instance // start with default configuration
                .WithArtifactsPath(artPath) // relocate output
                .AddDiagnoser(MemoryDiagnoser.Default) // with memory diagnostics
                .AddJob(Job.InProcess); // run in-process (exe name 'hb' is different from csproj name)

            BenchmarkRunner.Run(type, config);

            Console.WriteLine("Press a key to exit...");
            Console.ReadKey(true);
        }
    }
}

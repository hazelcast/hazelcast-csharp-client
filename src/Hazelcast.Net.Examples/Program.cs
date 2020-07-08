﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hazelcast.Protocol;

namespace Hazelcast.Examples
{
    public class Program
    {
        private static List<(string, Exception)> _exceptions = new List<(string, Exception)>();

        // NOTE
        //
        // run examples with 'hx' for Framework 4.6.2 and Core 3.x
        // run examples with 'dotnet hx.dll' for Core 2.1 (does not produce platform-specific exes)

        public static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(@"Usage: hx <example> <args>
  executes the static 'Hazelcast.Examples.<example>Example.Run' method.
  example: hx Client.Lifecycle
  <args> are passed as arguments to the method, and to configuration, so it is for
  instance possible to configure the server with the following arg:
    hazelcast.networking.addresses.0=192.168.0.42");
                return;
            }

            var filter = args[0];

#if NETCOREAPP
            if (filter.EndsWith('*'))
#else
            if (filter.EndsWith("*"))
#endif
            {
                filter = "Hazelcast.Examples." + filter.TrimEnd('*');
                var types = typeof (Program).Assembly
                    .GetTypes()
                    .Where(x => x.Name.EndsWith("Example") && x.FullName != null && x.FullName.StartsWith(filter))
                    .ToList();
                if (types.Count == 0)
                {
                    Console.WriteLine($"Error: no example matching '{filter}*'.");
                    return;
                }

                foreach (var type in types)
                {
                    await TryRunExampleAsync(type, args);
                }
            }
            else
            {
                var typeName = "Hazelcast.Examples." + filter + "Example";
                var type = Type.GetType(typeName);
                if (type == null)
                {
                    Console.WriteLine($"Error: could not find type {typeName}.");
                    return;
                }

                await TryRunExampleAsync(type, args);
            }

            if (_exceptions.Count > 0)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine();
                Console.WriteLine("The following examples have failed:");
                foreach (var (name, exception) in _exceptions)
                {
                    Console.WriteLine($"-{name} ({exception.GetType()}{ClientExceptionCode(exception)})");
                }
                Console.ForegroundColor = color;
            }
        }

        private static string ClientExceptionCode(Exception e)
        {
            if (!(e is ClientProtocolException cpe)) return null;

            return " - " + cpe.Error;
        }

        private static async Task TryRunExampleAsync(Type type, string[] args)
        {
            Console.WriteLine("###");
            Console.WriteLine($"### {type.Name}");
            Console.WriteLine("###");
            Console.WriteLine();
            try
            {
                await RunExampleAsync(type, args);
            }
            catch (Exception e)
            {
                _exceptions.Add((type.Name, e));

                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Example has thrown!");
                Console.WriteLine(e);
                Console.ForegroundColor = color;
            }
            Console.WriteLine();
        }

        private static async Task RunExampleAsync(Type type, string[] args)
        {
            var method = type.GetMethod("Run", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                Console.WriteLine($"Error: could not find static method {type.Name}.Run.");
                return;
            }

            var parameters = Array.Empty<object>();
            if (method.GetParameters().Length > 0)
            {
                args = args.Skip(1).ToArray();
                parameters = new object[] { args };
            }


            if (method.ReturnType == typeof(Task))
            {
                var task = (Task)method.Invoke(null, parameters);
                if (task == null)
                {
                    Console.WriteLine($"Error: static method {type.Name}.Run returned a null task.");
                    return;
                }
                await task;
            }
            else
            {
                method.Invoke(null, parameters);
            }
        }
    }
}

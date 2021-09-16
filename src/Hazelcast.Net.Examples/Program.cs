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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hazelcast.Protocol;

namespace Hazelcast.Examples
{
    public class Program
    {
        private static readonly List<(string, Exception)> Exceptions
            = new List<(string, Exception)>();
        private static readonly ConcurrentQueue<UnobservedTaskExceptionEventArgs> UnobservedExceptions
            = new ConcurrentQueue<UnobservedTaskExceptionEventArgs>();
        private static bool _collectUnobservedExceptions;

        // NOTE
        //
        // run examples with 'hx' for Framework 4.6.2 and Core 3.x
        // run examples with 'dotnet hx.dll' for Core 2.1 (does not produce platform-specific exes)

        public static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(@"Usage: hx [~]<example> <args>
  finds examples matching <example> and runs their Run method.
  if '~' is missing, looks for Hazelcast.Examples.<example>[Example] types; otherwise
  look for types with a full name matching the <example> regex.
  example: hx Client.Lifecycle ... hx ~MapSimple
  <args> are passed as arguments to the method, and to configuration, so it is for
  instance possible to configure the server with the following arg:
    --hazelcast.networking.addresses.0=192.168.0.42");
                return;
            }

            var exampleName = args[0];

#if NETFRAMEWORK
            if (exampleName.StartsWith("~"))
#else
            if (exampleName.StartsWith('~'))
#endif
            {
                // approx type name is a regex because, why not?

                var regex = new Regex(exampleName.TrimStart('~'), RegexOptions.Compiled);

                var types = typeof(Program).Assembly
                    .GetTypes()
                    .Where(x => x.IsClass && !x.IsNested &&
                            x.FullName != null &&
                            !x.FullName.Contains('<') &&
                            regex.IsMatch(x.FullName))
                    .ToList();

                if (types.Count == 0)
                {
                    Console.WriteLine($"Error: no example matching pattern '{exampleName}'.");
                    return;
                }

                foreach (var type in types)
                {
                    await TryRunExampleAsync(type, args);
                }
            }
            else
            {
                // exact type name can match Foo or FooExample.

                var typeName1 = "Hazelcast.Examples." + exampleName;
                var typeName2 = typeName1 + "Example";
                var type = Type.GetType(typeName1) ?? Type.GetType(typeName2);
                if (type == null)
                {
                    Console.WriteLine(!typeName1.EndsWith("Example")
                        ? $"Error: could not find type {typeName1} nor {typeName2}."
                        : $"Error: could not find type {typeName1}.");
                    return;
                }

                await TryRunExampleAsync(type, args);
            }

            if (Exceptions.Count > 0)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine();
                Console.WriteLine("The following examples have failed:");
                foreach (var (name, exception) in Exceptions)
                {
                    Console.WriteLine($"-{name} ({exception.GetType()}{ClientExceptionCode(exception)})");
                }
                Console.ForegroundColor = color;
            }
        }

        private static string ClientExceptionCode(Exception e)
            => e is RemoteException cpe ? " - " + cpe.Error : null;

        private static void InitializeUnobservedExceptions()
        {
            // make sure the queue is empty
            while (UnobservedExceptions.TryDequeue(out _))
            { }

            // handle unobserved exceptions
            TaskScheduler.UnobservedTaskException += UnobservedTaskException;

            _collectUnobservedExceptions = false;

            // GC should finalize everything, thus trigger unobserved exceptions
            // this should deal with leftovers from previous tests
            GC.Collect();
            GC.WaitForPendingFinalizers();

            _collectUnobservedExceptions = true;
        }

        private static void CollectUnobservedExceptions()
        {
            // GC should finalize everything, thus trigger unobserved exceptions
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // clear unobserved exceptions
#if !NETFRAMEWORK
            UnobservedExceptions.Clear();
#else
            while (UnobservedExceptions.TryDequeue(out _)) { }
#endif

            // remove handler
            TaskScheduler.UnobservedTaskException -= UnobservedTaskException;
        }

        private static void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            string message;
            if (_collectUnobservedExceptions)
            {
                message = "Unobserved";
                UnobservedExceptions.Enqueue(args);
            }
            else
            {
                message = "Leftover unobserved";
            }
            Console.WriteLine($"{message} Task Exception from {sender}\n{args.Exception}");
            args.SetObserved();
        }

        private static async Task TryRunExampleAsync(Type type, string[] args)
        {
            Console.WriteLine("###");
            Console.WriteLine($"### {type.Name}");
            Console.WriteLine("###");
            Console.WriteLine();

            InitializeUnobservedExceptions();

            try
            {
                await RunExampleAsync(type, args);
            }
            catch (Exception e)
            {
                Exceptions.Add((type.Name, e));

                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Example has thrown!");
                Console.WriteLine(e);
                Console.ForegroundColor = color;
            }

            CollectUnobservedExceptions();

            Console.WriteLine();
        }

        private static async Task RunExampleAsync(Type type, string[] args)
        {
            var method = type.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                Console.WriteLine($"Error: could not find static method {type.Name}.Main.");
                return;
            }

            var methodParameters = method.GetParameters();
            if (methodParameters.Length > 1)
            {
                Console.WriteLine($"Error: method {type.Name}.Main has an invalid number of parameters.");
                return;
            }

            var parameters = Array.Empty<object>();
            if (methodParameters.Length > 0)
            {
                if (methodParameters[0].ParameterType != typeof (string[]))
                {
                    Console.WriteLine($"Error: method {type.Name}.Main has an invalid parameter type.");
                    return;
                }

                args = args.Skip(1).ToArray();
                parameters = new object[] { args };
            }

            if (method.ReturnType == typeof(Task))
            {
                var task = (Task) method.Invoke(null, parameters);
                if (task == null)
                {
                    Console.WriteLine($"Error: method {type.Name}.Main returned a null task.");
                    return;
                }
                await task;
            }
            else if (method.ReturnType != typeof(void))
            {
                Console.WriteLine($"Error: method {type.Name}.Main has an invalid return type.");
            }
            else
            {
                method.Invoke(null, parameters);
            }
        }
    }
}

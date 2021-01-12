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
        private static readonly List<(string, Exception)> _exceptions
            = new List<(string, Exception)>();
        private static readonly ConcurrentQueue<UnobservedTaskExceptionEventArgs> _unobservedExceptions
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
                Console.WriteLine(@"Usage: hx <example> <args>
  executes the static 'Hazelcast.Examples.<example>Example.Run' method.
  example: hx Client.Lifecycle
  <args> are passed as arguments to the method, and to configuration, so it is for
  instance possible to configure the server with the following arg:
    hazelcast.networking.addresses.0=192.168.0.42");
                return;
            }

            var exampleName = args[0];

#if NETCOREAPP
            if (exampleName.StartsWith('~'))
#else
            if (exampleName.StartsWith("~"))
#endif
            {
                // approx type name is a regex because, why not?

                var regex = new Regex(exampleName.TrimStart('~'), RegexOptions.Compiled);

                var types = typeof(Program).Assembly
                    .GetTypes()
                    .Where(x => x.FullName != null && regex.IsMatch(x.FullName))
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
                    Console.WriteLine($"Error: could not find type {typeName1} nor {typeName2}.");
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
            => e is RemoteException cpe ? " - " + cpe.Error : null;

        private static void InitializeUnobservedExceptions()
        {
            // make sure the queue is empty
            while (_unobservedExceptions.TryDequeue(out _))
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

            // check for unobserved exceptions and report
            var failed = false;
            while (_unobservedExceptions.TryDequeue(out var args))
            {
                //var innerException = args.Exception.Flatten().InnerException;
                // log?
                failed = true;
            }

            // remove handler
            TaskScheduler.UnobservedTaskException -= UnobservedTaskException;
        }

        private static void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            string message;
            if (_collectUnobservedExceptions)
            {
                message = "Unobserved";
                _unobservedExceptions.Enqueue(args);
            }
            else
            {
                message = $"Leftover unobserved";
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
                _exceptions.Add((type.Name, e));

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
            object example;
            try
            {
                example = Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: could not instantiate example {type.Name}.");
                Console.WriteLine($"Exception: {e}");
                return;
            }

            var method = type.GetMethod("Run", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                Console.WriteLine($"Error: could not find instance method {type.Name}.Run.");
                return;
            }

            var methodParameters = method.GetParameters();
            if (methodParameters.Length > 1)
            {
                Console.WriteLine($"Error: method {type.Name}.Run has an invalid number of parameters.");
                return;
            }

            var parameters = Array.Empty<object>();
            if (methodParameters.Length > 0)
            {
                if (methodParameters[0].ParameterType != typeof (string[]))
                {
                    Console.WriteLine($"Error: method {type.Name}.Run has an invalid parameter type.");
                    return;
                }

                args = args.Skip(1).ToArray();
                parameters = new object[] { args };
            }

            if (method.ReturnType == typeof(Task))
            {
                var task = (Task) method.Invoke(example, parameters);
                if (task == null)
                {
                    Console.WriteLine($"Error: method {type.Name}.Run returned a null task.");
                    return;
                }
                await task;
            }
            else if (method.ReturnType != typeof(void))
            {
                Console.WriteLine($"Error: method {type.Name}.Run has an invalid return type.");
            }
            else
            {
                method.Invoke(example, parameters);
            }
        }
    }
}

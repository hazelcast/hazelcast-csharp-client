// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Reflection;

namespace Hazelcast.Examples
{
    internal class App
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: Hazelcast.Examples <example> <args>");
                Console.WriteLine("  executes the static, Hazelcast.Examples.<example>Example.Run method.");
                Console.WriteLine("  example: Hazelcast.Examples Client.Lifecycle");
                Console.WriteLine("  <args> are passed as arguments to the method");
                return;
            }

            var typeName = "Hazelcast.Examples." + args[0] + "Example";
            var type = Type.GetType(typeName);
            if (type == null)
            {
                Console.WriteLine($"Error: could not find type type {typeName}.");
                return;
            }

            var method = type.GetMethod("Run", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                Console.WriteLine($"Error: could not find static method {typeName}.Run.");
                return;
            }

            args = args.Skip(1).ToArray();
            method.Invoke(null, new object[] { args });
        }
    }
}
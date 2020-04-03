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
using System.Threading;

namespace AsyncTests1.Networking
{
    public class Log
    {
        public Log()
        { }

        public Log(string prefix)
        {
            Prefix = prefix;
        }

        public string Prefix { get; set; }

        private string FullPrefix => $"[{Thread.CurrentThread.ManagedThreadId:00}] {Prefix}: ";

        public void WriteLine(string text) => Console.WriteLine(FullPrefix + text);

        public void WriteLine(string format, params object[] args) => Console.WriteLine(FullPrefix + format, args);

        public void WriteLine(object o) => Console.WriteLine(FullPrefix + o);
    }
}
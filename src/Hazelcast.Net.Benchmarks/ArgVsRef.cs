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
using BenchmarkDotNet.Attributes;

namespace Hazelcast.Benchmarks
{
    public class ArgVsRef
    {
        [Benchmark]
        public void ArgBool()
        {
            var value = Condition();
            value = ArgBool(value);
            Consume(value);
        }

        [Benchmark]
        public void RefBool()
        {
            var value = Condition();
            RefBool(ref value);
            Consume(value);
        }

        [Benchmark]
        public void ArgS()
        {
            var value = new S { Value = Condition() };
            value = ArgS(value);
            Consume(value.Value);
        }

        [Benchmark]
        public void RefS()
        {
            var value = new S { Value = Condition() };
            RefS(ref value);
            Consume(value.Value);
        }

        //public void Meh()
        //{
        //    RefBool(ref var duh);
        //}

        private static bool ArgBool(bool value)
        {
            return Condition() && value; // respect order
        }

        private static void RefBool(ref bool value)
        {
            // &= does not produce optimized IL code
            // so, &= gives very bad benchmark results, whereas && gives
            // very close benchmark results (maybe a tad slower?)
            //value = value && Condition();
            value = Condition() && value;
        }

        private static S ArgS(S value)
        {
            return new S { Value = Condition() && value.Value };
        }

        private static void RefS(ref S value)
        {
            value = new S { Value = Condition() && value.Value };
        }

        private struct S
        {
            public bool Value;
        }

        private static bool Condition() => DateTime.Now.Second >= 30;

        private static void Consume(bool value) { }
    }
}

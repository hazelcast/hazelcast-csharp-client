// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Test
{
    internal class MyClass : IDisposable
    {
        private int _data = 10;
        private bool disposed;

        public int Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            Console.WriteLine("Dispose:" + Data);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!disposed)
            {
                // Note disposing has been done.
                disposed = true;
            }
        }

        ~MyClass()
        {
            Dispose(false);
            Console.WriteLine("finalized:" + Data);
        }
    }

    internal class Program
    {
        private static readonly ConcurrentDictionary<string, MyClass> dict = new ConcurrentDictionary<string, MyClass>();

        private static void Main1(string[] args)
        {
            using (var myClass = new MyClass())
            {
                myClass.Data = 17;
                //dict.TryAdd("den", myClass);
            }

            MyClass o;
            dict.TryGetValue("den", out o);

            //Console.WriteLine(o.Data);
            Console.WriteLine(dict.Count);
            Console.ReadKey();
        }

        private static void Main11(string[] args)
        {
            var clientConfig = new ClientConfig();

            clientConfig.GetNetworkConfig().AddAddress("127.0.0.1");

            var hazelcast = HazelcastClient.NewHazelcastClient(clientConfig);

            var list = hazelcast.GetList<object>("mylist");

            list.Add("Item 1");
            list.Add("Item 2");
            list.Add("Item 3");

            Console.WriteLine("count:" + list.Size());
            Console.ReadKey();
        }

        private static void Main2(string[] args)
        {
            var clientConfig = new ClientConfig();

            clientConfig.GetNetworkConfig().AddAddress("192.168.1.162");

            var hazelcast = HazelcastClient.NewHazelcastClient(clientConfig);

            var hzMap = hazelcast.GetMap<string, string>("mylist");

            //hzMap.Put("key1","Value1");
            //hzMap.Put("key2","Value2");
            //hzMap.Put("key3","Value3");
            //hzMap.Put("key4","Value4");

            Console.WriteLine(hzMap.Get("key1"));
            Console.WriteLine(hzMap.Get("key2"));
            Console.WriteLine(hzMap.Get("key3"));
            Console.WriteLine(hzMap.Get("key4"));

            Console.ReadKey();
        }

        //{
        //    var bs = new ByteSerializer();

        //    Console.WriteLine(bs is ByteSerializer);

        //    Console.WriteLine(bs is SingletonSerializer<byte>);
        //    Console.WriteLine(bs is IStreamSerializer<byte>);

        //    Console.ReadKey();
        //}

        //private bool test<T>(T t)
        //{

        //}
    }

    //public sealed class ByteSerializer : SingletonSerializer<byte>
    //{Class1.cs
    //    public override int GetTypeId()
    //    {
    //        return 1;
    //    }

    //    /// <exception cref="System.IO.IOException"></exception>
    //    public override byte Read(object input)
    //    {
    //        return 1;
    //    }

    //    /// <exception cref="System.IO.IOException"></exception>
    //    public override void Write(object output, byte obj)
    //    {
    //        //
    //    }
    //}

    //public abstract class SingletonSerializer<T> : IStreamSerializer<T>
    //{
    //    public virtual void Destroy()
    //    {
    //    }

    //    public abstract int GetTypeId();

    //    public abstract T Read(object arg1);

    //    public abstract void Write(object arg1, T arg2);
    //}
    //public interface IStreamSerializer<T> : ISerializer
    //{
    //    /// <exception cref="System.IO.IOException"></exception>
    //    void Write(object output, T t);

    //    /// <exception cref="System.IO.IOException"></exception>
    //    T Read(object input);
    //}

    //public interface ISerializer
    //{
    //    int GetTypeId();

    //    void Destroy();
    //}
}
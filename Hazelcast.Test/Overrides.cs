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
using System.Net;
using System.Reflection;
using Hazelcast.Util;

namespace Hazelcast.Test
{
    class Overrides
    {
        public static class Dns
        {
            static readonly StaticField GetHostNameFunc = new StaticField(typeof(DnsUtil), "_getHostNameFunc");
            static readonly StaticField GetHostEntryFunc = new StaticField(typeof(DnsUtil), "_getHostEntryFunc");
            static readonly StaticField GetHostAddressesFund = new StaticField(typeof(DnsUtil), "_getHostAddressesFunc");

            public static IDisposable GetHostName(Func<string> getHostName)
            {
                var f = GetHostNameFunc.Value;
                GetHostNameFunc.Value = getHostName;
                return new Disposable(() => { GetHostNameFunc.Value = f; });
            }

            public static IDisposable GetHostEntry(Func<string, IPHostEntry> getHostEntry)
            {
                var f = GetHostEntryFunc.Value;
                GetHostEntryFunc.Value = getHostEntry;
                return new Disposable(() => { GetHostEntryFunc.Value = f; });
            }

            public static IDisposable GetHostAddresses(Func<string, IPAddress[]> getHostAddresses)
            {
                var f = GetHostAddressesFund.Value;
                GetHostAddressesFund.Value = getHostAddresses;
                return new Disposable(() => { GetHostAddressesFund.Value = f; });
            }
        }

        class StaticField
        {
            readonly FieldInfo _field;

            public StaticField(Type type, string name)
            {
                _field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }

            public object Value
            {
                get { return _field.GetValue(null); }
                set { _field.SetValue(null, value); }
            }
        }

        class Disposable : IDisposable
        {
            readonly Action _action;

            public Disposable(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }
    }
}
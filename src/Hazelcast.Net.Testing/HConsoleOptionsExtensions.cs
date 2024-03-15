// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Testing.Logging;

namespace Hazelcast.Testing;

#if HZ_CONSOLE_PUBLIC
public
#else
internal
#endif

static class HConsoleOptionsExtensions
{
    public static HConsoleOptions ConfigureDefaults(this HConsoleOptions options, object source)
    {
#if HZ_CONSOLE
        options
            .WithHConsoleWriter(new HConsoleTextContextWriter())
            .Configure().SetLevel(0).EnableTimeStamp(origin: DateTime.Now) // default level
            .Configure<HConsoleLoggerProvider>().SetPrefix("LOG").SetMaxLevel() // always write the log output
            .Configure(source).SetMaxLevel().SetPrefix("TEST") // always write the test output
            .Configure<AsyncContext>().SetMinLevel() // do *not* write the AsyncContext verbose output
            .Configure<SocketConnectionBase>().SetIndent(1).SetLevel(0).SetPrefix("SOCKET");
#endif
        return options;
    }
}

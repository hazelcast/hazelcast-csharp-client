// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices.ComTypes;

namespace Hazelcast.Core;

#if HZ_CONSOLE

/// <summary>
/// Represents an <see cref="IHConsoleWriter"/> which writes directly to the system console.
/// </summary>
#if HZ_CONSOLE_PUBLIC
public
#else
internal
#endif
class HConsoleConsoleWriter : IHConsoleWriter
{
    private readonly object _mutex = new();

    /// <inheritdoc />
    public void AppendLine(string text)
    {
        lock (_mutex) Console.WriteLine(text);
    }
}

#endif
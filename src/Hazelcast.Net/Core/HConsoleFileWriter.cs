// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using System.IO;

namespace Hazelcast.Core;

#if HZ_CONSOLE

/// <summary>
/// Represents an <see cref="IHConsoleWriter"/> which writes to a file.
/// </summary>
#if HZ_CONSOLE_PUBLIC
public
#else
internal
#endif
class HConsoleFileWriter : IHConsoleWriter
{
    private readonly object _mutex = new();
    private readonly string _filename;

    public HConsoleFileWriter(string filename)
    {
        _filename = filename;
    }

    public void AppendLine(string text)
    {
        // todo: implement a more efficient buffered mechanism
        lock (_mutex) File.AppendAllLines(_filename, GetLines(text));
    }

    private static IEnumerable<string> GetLines(string text)
    {
        yield return text;
    }
}

#endif
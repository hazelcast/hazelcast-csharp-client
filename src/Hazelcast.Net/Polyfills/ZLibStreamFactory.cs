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
using System.IO;

#if NET6_0_OR_GREATER
using System.IO.Compression;
#endif

namespace Hazelcast.Polyfills;

// System.IO.Compression.ZLibStream is available starting with .NET 6, and
// before that there is no way to zip files in stock .NET that can be un-zipped
// by Java on Hazelcast members (for metrics). This attempts at providing
// a solution for .NET pre-6.
//
// We used to use DotNetZip but that library has a medium security issue that
// bothers some of our users (silly thing about using random number generator).
//
// back-porting the .NET 6 ZLibStream class proves problematic as deep down
// it P/Invokes ZLIB and who knows what' available on .NET Framework?
//
// so... we're bringing a small subset of DotNetZip (which is not impacted by
// the security issue) into our codebase. Code is available on GitHub under the
// MS-PL license.
//
// this factory ensures we create the proper zipping stream depending on framework.

internal static class ZLibStreamFactory
{
#if NET6_0_OR_GREATER

    public static Stream Compress(Stream stream, bool leaveOpen)
        => new ZLibStream(stream, CompressionLevel.Fastest, leaveOpen);

    public static Stream Decompress(Stream stream, bool leaveOpen)
        => new ZLibStream(stream, CompressionMode.Decompress, leaveOpen);

#else

    public static Stream Compress(Stream stream, bool leaveOpen)
        => new Ionic.Zlib.ZlibStream(stream, Ionic.Zlib.CompressionMode.Compress, Ionic.Zlib.CompressionLevel.BestSpeed, leaveOpen);

    public static Stream Decompress(Stream stream, bool leaveOpen)
        => new Ionic.Zlib.ZlibStream(stream, Ionic.Zlib.CompressionMode.Decompress, leaveOpen);

#endif
}
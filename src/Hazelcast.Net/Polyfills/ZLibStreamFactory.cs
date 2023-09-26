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
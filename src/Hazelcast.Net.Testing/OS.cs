using System;
using System.Runtime.InteropServices;

namespace Hazelcast.Testing
{
    // ReSharper disable once InconsistentNaming
    public static class OS
    {
        // https://stackoverflow.com/questions/38790802/determine-operating-system-in-net-core

        static OS()
        {
            Platform =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSPlatform.Windows :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSPlatform.OSX :
                throw new NotSupportedException("Unsupported platform.");

        }

        public static bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        // ReSharper disable once InconsistentNaming
        public static bool IsOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static OSPlatform Platform { get; }
    }
}

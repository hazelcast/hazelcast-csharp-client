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

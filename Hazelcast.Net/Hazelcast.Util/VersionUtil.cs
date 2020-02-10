// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Util
{
    internal class VersionUtil
    {
        public const int UnknownVersion = -1;
        private const int MajorVersionMultiplier = 10000;
        private const int MinorVersionMultiplier = 100;

        public static string GetDllVersion()
        {
            var version = typeof(VersionUtil).Assembly.GetName().Version;
            var versionStr = version.Build == 0
                ? $"{version.Major}.{version.Minor}"
                : $"{version.Major}.{version.Minor}.{version.Build}";
            return versionStr;
        }

        public static int ParseServerVersion(string serverVersion)
        {
            if (serverVersion == null)
            {
                return UnknownVersion;
            }

            var mainParts = serverVersion.Split('-');
            var tokens = mainParts[0].Split('.');

            if (tokens.Length < 2)
            {
                return UnknownVersion;
            }
            var calculatedVersion = int.Parse(tokens[0]) * MajorVersionMultiplier;
            calculatedVersion += int.Parse(tokens[1]) * MinorVersionMultiplier;
            if (tokens.Length > 2)
            {
                calculatedVersion += int.Parse(tokens[2]);
            }
            return calculatedVersion;
        }

        public const int Version38 = MajorVersionMultiplier * 3 + MinorVersionMultiplier * 8;
    }
}
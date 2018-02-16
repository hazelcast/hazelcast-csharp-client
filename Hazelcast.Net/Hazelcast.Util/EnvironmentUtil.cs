// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Logging;

namespace Hazelcast.Util
{
    internal class EnvironmentUtil
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (EnvironmentUtil));

        public static int? ReadEnvironmentVar(string var)
        {
            var param = Environment.GetEnvironmentVariable(var);
            try
            {
                if (param != null)
                {
                    return Convert.ToInt32(param, 10);
                }
            }
            catch (Exception)
            {
                Logger.Warning("Provided value is not a valid value : " + param);
                return null;
            }
            return null;
        }

        public static string GetDllVersion()
        {
            var version = typeof(EnvironmentUtil).Assembly.GetName().Version;
            var versionStr = version.Build == 0 ? string.Format("{0}.{1}", version.Major, version.Minor)
                : string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            return versionStr;
        }
    }
}
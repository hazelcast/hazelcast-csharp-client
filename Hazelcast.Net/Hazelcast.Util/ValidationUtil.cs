// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Util
{
    /// <summary>A utility class for validating arguments and state.</summary>
    /// <remarks>A utility class for validating arguments and state.</remarks>
    internal class ValidationUtil
    {
        private ValidationUtil()
        {
        }

        public static string HasText(string argument, string argName)
        {
            IsNotNull(argument, argName);
            if (argument.Length == 0)
            {
                throw new ArgumentException(string.Format("argument {0} can't be an empty string", argName));
            }
            return argument;
        }

        public static T IsNotNull<T>(T argument, string argName)
        {
            if (argument == null)
            {
                throw new ArgumentException(string.Format("argument {0} can't be null", argName));
            }
            return argument;
        }
    }
}
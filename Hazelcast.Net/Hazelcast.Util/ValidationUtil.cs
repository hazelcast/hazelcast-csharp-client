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

namespace Hazelcast.Util
{
    /// <summary>A utility class for validating arguments and state.</summary>
    internal static class ValidationUtil
    {
        public const string NULL_LISTENER_IS_NOT_ALLOWED = "Null listener is not allowed!";
        public const string NULL_KEY_IS_NOT_ALLOWED = "Null key is not allowed!";
        public const string NULL_VALUE_IS_NOT_ALLOWED = "Null value is not allowed!";
        public const string NULL_PREDICATE_IS_NOT_ALLOWED = "Predicate should not be null!";
        public const string NULL_AGGREGATOR_IS_NOT_ALLOWED = "Aggregator should not be null!";
        public const string NULL_PROJECTION_IS_NOT_ALLOWED = "Projection should not be null!";
        public const string NULL_JSON_STRING_IS_NOT_ALLOWED = "JSON string cannot be null!";

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

        public static T CheckNotNull<T>(T argument, string errorMessage)
        {
            if (argument == null)
            {
                throw new NullReferenceException(errorMessage);
            }
            return argument;
        }
        
        public static int CheckNotNegative(int value, string errorMessage) {
            if (value < 0) {
                throw new ArgumentException(errorMessage);
            }
            return value;
        }
        
        public static void ThrowExceptionIfNull(object o, string message = null)
        {
            if (o == null)
            {
                throw new ArgumentNullException(message);
            }
        }

        public static void ThrowExceptionIfTrue(bool expression, string message)
        {
            if (expression)
            {
                throw new ArgumentException(message);
            }
        }

    }
}
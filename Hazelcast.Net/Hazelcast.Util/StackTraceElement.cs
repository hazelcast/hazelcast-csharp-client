/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

﻿namespace Hazelcast.Util
{
    internal class StackTraceElement
    {
        public StackTraceElement(string className, string methodName, string fileName, int lineNumber)
        {
            ClassName = className;
            MethodName = methodName;
            FileName = fileName;
            LineNumber = lineNumber;
        }

        public string ClassName { get; private set; }
        public string MethodName { get; private set; }
        public string FileName { get; private set; }
        public int LineNumber { get; private set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((StackTraceElement) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ClassName != null ? ClassName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (MethodName != null ? MethodName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (FileName != null ? FileName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ LineNumber;
                return hashCode;
            }
        }

        protected bool Equals(StackTraceElement other)
        {
            return string.Equals(ClassName, other.ClassName) && string.Equals(MethodName, other.MethodName) &&
                   string.Equals(FileName, other.FileName) && LineNumber == other.LineNumber;
        }
    }
}
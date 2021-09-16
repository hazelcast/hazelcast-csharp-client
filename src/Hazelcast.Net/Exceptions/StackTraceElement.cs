// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

#if NETFRAMEWORK || NETSTANDARD2_0
#else
using System;
#endif

namespace Hazelcast.Exceptions
{
    /// <summary>
    /// Represents a stack trace element that can be exchanged between the client and the server.
    /// </summary>
    internal class StackTraceElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StackTraceElement"/> class.
        /// </summary>
        /// <param name="className">The name of the class.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="fileName">The name of the source file.</param>
        /// <param name="lineNumber">The line number.</param>
        public StackTraceElement(string className, string methodName, string fileName, int lineNumber)
        {
            ClassName = className;
            MethodName = methodName;
            FileName = fileName;
            LineNumber = lineNumber;
        }

        /// <summary>
        /// Gets the name of the class.
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Gets the name of the source file.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int LineNumber { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is StackTraceElement other && EqualsN(this, other);
        }

        /// <summary>
        /// Determines whether two objects are equal.
        /// </summary>
        /// <param name="left">The first object.</param>
        /// <param name="right">The second object.</param>
        /// <returns>A value indicating whether the two objects are equal.</returns>
        private static bool Equals(StackTraceElement left, StackTraceElement right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return EqualsN(left, right);
        }

        private static bool EqualsN(StackTraceElement left, StackTraceElement right)
            => left.ClassName == right.ClassName &&
               left.MethodName == right.MethodName &&
               left.FileName == right.FileName &&
               left.LineNumber == right.LineNumber;

        public static bool operator ==(StackTraceElement left, StackTraceElement right) => Equals(left, right);

        public static bool operator !=(StackTraceElement left, StackTraceElement right) => !Equals(left, right);

        /// <inheritdoc />
        public override int GetHashCode()
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            unchecked
            {
                var hashCode = (ClassName != null ? ClassName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MethodName != null ? MethodName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FileName != null ? FileName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ LineNumber;
                return hashCode;
            }
#else
            return HashCode.Combine(ClassName, MethodName, FileName, LineNumber);
#endif
        }

        /// <inheritdoc />
        public override string ToString() => $"at {ClassName}.{MethodName}(...) in {FileName}:{LineNumber}";
    }
}

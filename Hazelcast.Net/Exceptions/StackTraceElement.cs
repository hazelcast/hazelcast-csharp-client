using System;

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
            return obj is StackTraceElement other && Equals(this, other);
        }

        /// <summary>
        /// Determines whether two objects are equal.
        /// </summary>
        /// <param name="x1">The first object.</param>
        /// <param name="x2">The second object.</param>
        /// <returns>A value indicating whether the two objects are equal.</returns>
        private static bool Equals(StackTraceElement x1, StackTraceElement x2)
        {
            if (ReferenceEquals(x1, x2)) return true;
            if (x1 is null) return false;
            return x1.ClassName == x2.ClassName &&
                   x1.MethodName == x2.MethodName &&
                   x1.FileName == x2.FileName &&
                   x1.LineNumber == x2.LineNumber;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
#if NETSTANDARD2_0
            unchecked
            {
                var hashCode = (ClassName != null ? ClassName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MethodName != null ? MethodName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FileName != null ? FileName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ LineNumber;
                return hashCode;
            }
#endif
#if NETSTANDARD2_1
            return HashCode.Combine(ClassName, MethodName, FileName, LineNumber);
#endif
        }

        /// <inheritdoc />
        public override string ToString() => $"at {ClassName}.{MethodName}(...) in {FileName}:{LineNumber}";
    }
}

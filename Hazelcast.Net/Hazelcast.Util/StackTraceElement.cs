namespace Hazelcast.Util
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
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

        public static E IsNotNull<E>(E argument, string argName)
        {
            if (argument == null)
            {
                throw new ArgumentException(string.Format("argument {0} can't be null", argName));
            }
            return argument;
        }
    }
}
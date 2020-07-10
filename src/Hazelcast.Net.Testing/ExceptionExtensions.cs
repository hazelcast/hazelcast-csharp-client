using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Hazelcast.Testing
{
    public static class ExceptionExtensions
    {
        public static TException SerializeAndDeSerialize<TException>(this TException e)
            where TException : Exception
        {
            // read https://stackoverflow.com/questions/94488

            var fmt = new BinaryFormatter();
            using var ms = new MemoryStream();

            fmt.Serialize(ms, e);
            ms.Seek(0, 0);
            return (TException)fmt.Deserialize(ms);
        }
    }
}

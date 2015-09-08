using System;
using System.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;

namespace Hazelcast.IO
{
    internal sealed class IOUtil
    {
        /// <summary>Closes the Closable quietly.</summary>
        /// <remarks>Closes the Closable quietly. So no exception will be thrown. Can also safely be called with a null value.</remarks>
        /// <param name="closeable">the Closeable to close.</param>
        public static void CloseResource(IDisposable closeable)
        {
            if (closeable != null)
            {
                try
                {
                    closeable.Dispose();
                }
                catch (IOException e)
                {
                    Logger.GetLogger(typeof (IOUtil)).Finest("closeResource failed", e);
                }
            }
        }
    }
}
using System;
using System.IO;
using System.Text;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Represents a capture of the console output.
    /// </summary>
    public class ConsoleCapture
    {
        private readonly MemoryStream _memory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleCapture"/> class.
        /// </summary>
        public ConsoleCapture()
        {
            _memory = new MemoryStream();
        }

        /// <summary>
        /// Starts capturing the console output.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> object which must be disposed to stop the capture.</returns>
        public IDisposable Output()
        {
            return new Capturing(_memory);
        }

        private class Capturing : IDisposable
        {
            private readonly TextWriter _console;
            private readonly StreamWriter _writer;

            public Capturing(Stream stream)
            {
                _console = Console.Out;
                _writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);

                Console.SetOut(_writer);
            }

            public void Dispose()
            {
                _writer.Flush();
                _writer.Close();
                _writer.Dispose();

                Console.SetOut(_console);
            }
        }

        /// <summary>
        /// Read all characters.
        /// </summary>
        /// <returns>A string containing all characters that were captured.</returns>
        public string ReadToEnd()
        {
            _memory.Seek(0, SeekOrigin.Begin);
            return new StreamReader(_memory).ReadToEnd();
        }
    }
}

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
            _memory.SetLength(0);
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

// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using System.IO;
using System.IO.Pipelines;

namespace Hazelcast.Testing.Networking
{
    /// <summary>
    /// Represents a memory pipe.
    /// </summary>
    public class MemoryPipe
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryPipe"/> class.
        /// </summary>
        public MemoryPipe()
        {
            var pipe1 = new Pipe();
            var pipe2 = new Pipe();

            Stream1 = new DuplexStream(pipe1, pipe2, () => Stream2.Dispose());
            Stream2 = new DuplexStream(pipe2, pipe1, () => Stream1.Dispose());
        }

        /// <summary>
        /// Gets the first end of the pipe.
        /// </summary>
        public Stream Stream1 { get; }

        /// <summary>
        /// Gets the second end of the pipe.
        /// </summary>
        public Stream Stream2 { get; }
    }
}

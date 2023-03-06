// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Buffers;
using System.Globalization;
using System.Text;
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for dumping objects.
    /// </summary>
    internal static class DumpCoreExtensions
    {
        /// <summary>
        /// Dumps an array of bytes into a readable format.
        /// </summary>
        /// <param name="bytes">The array of bytes.</param>
        /// <param name="length">The number of bytes to dump, or zero to dump all bytes in the array.</param>
        /// <param name="formatted">Whether to format the output.</param>
        /// <returns>A readable string representation of the array of bytes.</returns>
        public static string Dump(this byte[] bytes, long length = 0, bool formatted = true)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (length > bytes.Length)
                throw new InvalidOperationException(ExceptionMessages.NotEnoughBytes);

            if (length == 0) length = bytes.Length;
            if (length == 0) return string.Empty;

            var text = new StringBuilder();

            if (formatted)
            {
                var i = 0;
                while (i < length)
                {
                    for (var j = 0; j < 8 && i < length; j++, i++)
                    {
                        if (j > 0) text.Append(' ');
                        text.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", bytes[i]);
                    }

                    if (i < length)
                        text.AppendLine();
                }
            }
            else
            {
                for (var i = 0; i < length; i++)
                    text.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", bytes[i]);
            }

            return text.ToString();
        }

        /// <summary>
        /// Dumps an sequence of bytes into a readable format.
        /// </summary>
        /// <param name="bytes">The sequence of bytes.</param>
        /// <param name="length">The number of bytes to dump, or zero to dump all bytes in the sequence.</param>
        /// <returns>A readable string representation of the sequence of bytes.</returns>
        public static string Dump(this ReadOnlySequence<byte> bytes, long length = 0)
        {
            if (length == 0) length = bytes.Length;
            if (length == 0) return string.Empty;
            var a = new byte[length];
            bytes.CopyTo(a);
            return a.Dump();
        }
    }
}

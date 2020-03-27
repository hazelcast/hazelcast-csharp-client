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

using System.Text;

namespace AsyncTests1.Networking
{
    public class Message
    {
        public Message(string text) 
            => Text = text;

        public static Message Parse(byte[] bytes)
        {
            var s = Encoding.UTF8.GetString(bytes);
            return Parse(s);
        }

        public static Message Parse(string s)
            => new Message(s.Substring(5)) { Id = int.Parse(s.Substring(0, 4)) };

        public int Id { get; set; }

        public string Text { get; }

        public byte[] ToBytes() => Encoding.UTF8.GetBytes(ToString());

        public byte[] ToPrefixedBytes()
        {
            var bytes1 = Encoding.UTF8.GetBytes(ToString());
            var bytes2 = new byte[bytes1.Length + 4];

            var length = bytes1.Length;

            for (var i = 3; i >= 0; i--)
            {
                bytes2[i] = (byte) length;
                length >>= 8;
            }

            bytes1.CopyTo(bytes2, 4);
            return bytes2;
        }

        private string ToPrefixedString() => $"{33}:{Id:0000}:{Text}";

        public override string ToString() => $"{Id:0000}:{Text}";
    }
}
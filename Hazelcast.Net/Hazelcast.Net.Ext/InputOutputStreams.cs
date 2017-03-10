// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Net.Ext
{
    internal interface IInputStream
    {
        int Available();

        void Close();

        void Mark(int readlimit);

        bool MarkSupported();
        int Read();

        int Read(byte[] b);

        int Read(byte[] b, int off, int len);

        void Reset();

        long Skip(long n);
    }

    internal interface IOutputStream
    {
        void Close();

        void Flush();
        void Write(int b);

        void Write(byte[] b);

        void Write(byte[] b, int off, int len);
    }
}
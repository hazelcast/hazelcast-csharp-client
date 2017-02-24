// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Hazelcast.Client.Test
{
    public static class ProcessUtil
    {
        /// <summary>
        /// Resumes all threads of the given process
        /// </summary>
        /// <param name="process"></param>
        public static void Resume(Process process)
        {
            Action<ProcessThread> resume = pt =>
            {
                var threadHandle = NativeMethods.OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint) pt.Id);

                if (threadHandle != IntPtr.Zero)
                {
                    try
                    {
                        NativeMethods.ResumeThread(threadHandle);
                    }
                    finally
                    {
                        NativeMethods.CloseHandle(threadHandle);
                    }
                }
            };

            var threads = GetThreads(process);
            Parallel.ForEach(threads, new ParallelOptions {MaxDegreeOfParallelism = threads.Length},
                pt => { resume(pt); });
        }

        /// <summary>
        /// Suspends all threads of the given process
        /// </summary>
        /// <param name="process"></param>
        public static void Suspend(Process process)
        {
            Action<ProcessThread> suspend = pt =>
            {
                var threadHandle = NativeMethods.OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint) pt.Id);

                if (threadHandle != IntPtr.Zero)
                {
                    try
                    {
                        NativeMethods.SuspendThread(threadHandle);
                    }
                    finally
                    {
                        NativeMethods.CloseHandle(threadHandle);
                    }
                }
                ;
            };

            var threads = GetThreads(process);
            Parallel.ForEach(threads, new ParallelOptions {MaxDegreeOfParallelism = threads.Length},
                pt => { suspend(pt); });
        }

        private static ProcessThread[] GetThreads(Process process)
        {
            var threads = new ProcessThread[process.Threads.Count];
            for (var i = 0; i < process.Threads.Count; i++)
            {
                threads[i] = process.Threads[i];
            }
            return threads;
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr hObject);

            [DllImport("kernel32.dll")]
            public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

            [DllImport("kernel32.dll")]
            public static extern uint ResumeThread(IntPtr hThread);

            [DllImport("kernel32.dll")]
            public static extern uint SuspendThread(IntPtr hThread);
        }

        [Flags]
        private enum ThreadAccess
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }
    }
}
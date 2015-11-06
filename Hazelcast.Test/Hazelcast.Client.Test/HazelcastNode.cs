/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

﻿using System;
using System.Diagnostics;
using System.IO;
﻿using Hazelcast.Logging;

namespace Hazelcast.Client.Test
{
    public class HazelcastNode
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(HazelcastNode));
        private readonly int _id;
        private Process _process;
        private StreamWriter _stderr;
        private const string HazelcastJar = "hazelcast.jar";

        public HazelcastNode(int id)
        {
            _id = id;
        }

        public int Id
        {
            get { return _id; }
        }

        public void Start()
        {
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (javaHome == null) throw new Exception("JAVA_HOME must be defined in order to run the unit tests.");
            
            var hzhome = Environment.GetEnvironmentVariable("HAZELCAST_HOME");
            if (hzhome == null) throw new Exception("HAZELCAST_HOME must be defined in order to run the unit tests.");

            var redirectOutput = Environment.GetEnvironmentVariable("HAZELCAST_REDIRECT_OUTPUT") != null;
            var jarPath = Path.Combine(hzhome, HazelcastJar);

            if (!File.Exists(jarPath))
            {
                throw new FileNotFoundException("Could not find hazelcast.jar at " + jarPath);
            }

            string[] arguments =
            {
                "-cp", jarPath,
                "-Dhazelcast.event.queue.capacity=1100000",
                "-Dhazelcast.config=" + Path.Combine(hzhome,"hazelcast.xml"),
                "com.hazelcast.core.server.StartServer"
            };

            _process = new Process();
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.Arguments = String.Join(" ", arguments);
            _process.StartInfo.FileName = Path.Combine(javaHome, "bin", "java");

            if (redirectOutput)
            {
                _process.StartInfo.RedirectStandardOutput = true;
                _process.StartInfo.RedirectStandardError = true;
                _stderr = new StreamWriter("hz-" + _id + "-err.txt", false);
                _process.ErrorDataReceived += (sender, args) => _stderr.WriteLine(args.Data);
            }

            Logger.Info("Starting Hazelcast instance with id " + Id);
            if (!_process.Start())
            {
                throw new Exception("Could not start hazelcast insance.");
            }

            if (redirectOutput)
            {
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }
        }

        public void Stop()
        {
            if (_stderr != null)
            {
                _stderr.WriteLine(DateTime.Now + ": Stopping instance.");
            }

            Logger.Info("Stopping Hazelcast instance with id " + Id);
            _process.Kill();
            _process.WaitForExit();
            if (_stderr != null)
            {
                _stderr.Flush();
            }
        }

        public void Suspend()
        {
            Logger.Info("Suspending Hazelcast instance with id " + Id);
            ProcessUtil.Suspend(_process);
        }

        public void Resume()
        {
            Logger.Info("Resuming Hazelcast instance with id " + Id);
            ProcessUtil.Resume(_process);
        }
    }
}

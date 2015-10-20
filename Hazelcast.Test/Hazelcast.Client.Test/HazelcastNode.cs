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
using Hazelcast.Logging;

namespace Hazelcast.Client.Test
{
    public class HazelcastNode
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(HazelcastNode));
        private readonly int _id;
        private Process _process;
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
            
            string[] arguments =
            {
                "-cp", Path.Combine(hzhome, HazelcastJar),
                "-Dhazelcast.event.queue.capacity=1100000",
                "-Dhazelcast.config=" + Path.Combine(hzhome,"hazelcast.xml"),
                "com.hazelcast.core.server.StartServer"
            };

            _process = new Process();
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.Arguments = String.Join(" ", arguments);
            _process.StartInfo.FileName = Path.Combine(javaHome, "bin", "java");

            Logger.Info("Starting Hazelcast instance with id " + Id);
            if (!_process.Start())
            {
                throw new Exception("Could not start hazelcast insance.");
            }
        }

        public void Stop()
        {
            Logger.Info("Stopping Hazelcast instance with id " + Id);
            _process.Kill();
        }
    }
}

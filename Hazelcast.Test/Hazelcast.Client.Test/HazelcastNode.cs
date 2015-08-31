using System;
using System.Diagnostics;
using System.IO;

namespace Hazelcast.Client.Test
{
    public class HazelcastNode
    {
        private Process _process;
        private const string HazelcastJar = "hazelcast.jar";

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

            if (!_process.Start())
            {
                throw new Exception("Could not start hazelcast insance.");
            }
        }

        public void Stop()
        {
            _process.Kill();
        }
    }
}

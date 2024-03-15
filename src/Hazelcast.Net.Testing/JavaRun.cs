// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    public class JavaRun : IDisposable
    {
        private static readonly string JdkPath;
        private static readonly string JavaExe;
        private static readonly string JavacExe;

        private readonly string _path = Path.Combine(Path.GetTempPath(), $"hz-tests-{Guid.NewGuid():N}");
        private readonly List<string> _sources = new List<string>();
        private readonly Dictionary<string, string> _sourceTexts = new Dictionary<string, string>();
        private readonly List<string> _libs = new List<string>();
        private readonly string _libpath;

        private static void IfExists(string path, string exe, ref string javaPath, ref string javaExe)
        {
            if (javaPath == null && File.Exists(Path.Combine(path, exe)))
            {
                javaPath = path;
                javaExe = exe;
            }
        }

        static JavaRun()
        {
            var envpath = Environment.GetEnvironmentVariable("PATH");
            string javaPath = null, javacPath = null;
            if (envpath != null)
            {
                foreach (var path in envpath.Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries))
                {
                    IfExists(path, "java.exe", ref javaPath, ref JavaExe);
                    IfExists(path, "java", ref javaPath, ref JavaExe);
                    IfExists(path, "javac.exe", ref javacPath, ref JavacExe);
                    IfExists(path, "javac", ref javacPath, ref JavacExe);
                }
            }

            if (javaPath == null || javacPath == null || javaPath != javacPath)
                throw new InvalidOperationException($"Could not locate a JDK in PATH ({envpath}) " +
                                                    $"(java: {javaPath ?? "n/a"}, javac: {javacPath ?? "n/a"}).");

            JdkPath = javaPath;
        }

        public JavaRun()
        {
            if (JdkPath == null)
                throw new InvalidOperationException($"Could not locate a JDK in PATH ({Environment.GetEnvironmentVariable("PATH")}).");

            if (!File.Exists(Path.Combine(JdkPath, JavaExe)))
                throw new InvalidOperationException($"Not found: {Path.Combine(JdkPath, JavaExe)}.");

            if (!File.Exists(Path.Combine(JdkPath, JavacExe)))
                throw new InvalidOperationException($"Not found: {Path.Combine(JdkPath, JavacExe)}.");

            Console.WriteLine($"JavaRun with JDK: {JdkPath}");

            var assemblyLocation = GetType().Assembly.Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            if (assemblyDirectory == null)
                throw new InvalidOperationException("Could not locate assembly.");
            var solutionPath = Path.GetFullPath(Path.Combine(assemblyDirectory, "../../../../.."));
            _libpath = Path.Combine(solutionPath, "temp/lib");
        }

        public void Dispose()
        {
            // get rid of the temp directory
            Directory.Delete(_path, true);

        }

        public JavaRun WithSource(string source)
        {
            _sources.Add(source);
            return this;
        }

        public JavaRun WithSourceText(string name, string text)
        {
            _sourceTexts.Add(name, text);
            return this;
        }

        public JavaRun WithLib(string lib)
        {
            _libs.Add(lib);
            return this;
        }

        private string GetClassPath(string path = null)
        {
            const string prefix = "-cp ";

            var separator = Path.PathSeparator;

            var classpath = new StringBuilder();
            classpath.Append(prefix);

            foreach (var lib in _libs)
            {
                if (classpath.Length > prefix.Length) classpath.Append(separator);
                var libpath = PathCombineFull(_libpath, lib);
                Assert.That(File.Exists(libpath), Is.True, $"Lib {lib} not found.");
                classpath.Append(libpath);
            }

            if (path != null)
            {
                if (classpath.Length > prefix.Length) classpath.Append(separator);
                classpath.Append(path);
            }

            return classpath.Length > prefix.Length ? classpath.ToString() : "";
        }

        private static string PathCombineFull(params string[] paths)
        {
            // meh - .NET Framework (even 4.8) does not like having a '*' char in the path, throws
            var path = Path.Combine(paths);
            path = path.Replace("*", "THIS_IS_A_STAR_CHAR");
            path = Path.GetFullPath(path);
            path = path.Replace("THIS_IS_A_STAR_CHAR", "*");
            return path;
        }

        private static (int RC, string Output, string OutputAndError) Run(string exe, string args, byte[] input = null)
        {
            var withInput = input != null && input.Length > 0;

            var info = new ProcessStartInfo(exe, args)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = withInput
            };

#if !NETCOREAPP
            info.UseShellExecute = false;
#endif

            var process = new Process { StartInfo = info };

            var output = new StringBuilder();
            var outputAndError = new StringBuilder();

            // perform async reads, sync reads can hang if output is too big
            process.OutputDataReceived += (_, a) =>
            {
                output.Append(a.Data);
                output.Append(Environment.NewLine);
                outputAndError.Append(a.Data);
                outputAndError.Append(Environment.NewLine);
            };
            process.ErrorDataReceived += (_, a) =>
            {
                outputAndError.Append("ERR: ");
                outputAndError.Append(a.Data);
                outputAndError.Append(Environment.NewLine);
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (withInput)
            {
                process.StandardInput.BaseStream.Write(input, 0, input.Length);
                process.StandardInput.Close();
            }

            process.WaitForExit();

            return (process.ExitCode, output.ToString(), outputAndError.ToString());
        }

        public void Compile()
        {
            Console.WriteLine("Create temp directory...");
            Directory.CreateDirectory(_path);

            var args = new StringBuilder();
            args.Append(GetClassPath());

            Console.WriteLine("Copy source files...");
            foreach (var source in _sources)
            {
                var path = PathCombineFull(_path, Path.GetFileName(source));
                args.Append(" ");
                args.Append(path);
                File.WriteAllText(path, TestFiles.ReadAllText(this, source));
            }
            Console.WriteLine("Write source texts...");
            foreach (var (name, text) in _sourceTexts)
            {
                var path = PathCombineFull(_path, $"{name}.java");
                args.Append(" ");
                args.Append(path);
                File.WriteAllText(path, text);
            }
            if (!Directory.GetFiles(_path, "*.java").Any())
                throw new InvalidOperationException("No source files.");

            Console.WriteLine("Compile...");
            var exe = PathCombineFull(JdkPath, JavacExe);
            Console.WriteLine($"> javac {args}");

            var (exitCode, _, outputAndError) = Run(exe, args.ToString());
            Console.WriteLine(outputAndError);

            if (exitCode != 0) throw new InvalidOperationException($"Compilation failed (rc={exitCode}).");
        }

        public string Execute(string classname, byte[] pipeBytes = null)
        {
            pipeBytes ??= Array.Empty<byte>();

            Console.WriteLine($"Execute (piping {pipeBytes.Length} bytes)...");
            var exe = PathCombineFull(JdkPath, JavaExe);
            var args = $"{GetClassPath(_path)} {classname}";
            Console.WriteLine($"> java {args}");

            var (exitCode, output, outputAndError) = Run(exe, args, pipeBytes);
            Console.WriteLine(outputAndError);

            if (exitCode != 0) throw new InvalidOperationException($"Execution failed (rc={exitCode}).");
            return output;
        }
    }
}

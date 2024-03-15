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
using System.IO;
using System.Reflection;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides access to files in an assembly project.
    /// </summary>
    /// <remarks>
    /// <para>This is similar to using resources, except that the files are not embedded in the assembly
    /// but directly accessed via the filesystem. This means that the test assembly does not need to be
    /// recompiled when the files change, which can simplify and accelerate iterative development.</para>
    /// </remarks>
    public static class TestFiles
    {
        /// <summary>
        /// Opens an assembly project text file, reads all the text in the file, and then closes the file.
        /// </summary>
        /// <typeparam name="T">A type contained by the assembly.</typeparam>
        /// <param name="path">The path of the file relative to the project.</param>
        /// <returns>The text content of the file.</returns>
        public static string ReadAllText<T>(string path)
            => ReadAllText(typeof (T).Assembly, path);

        /// <summary>
        /// Opens an assembly project text file, reads all the text in the file, and then closes the file.
        /// </summary>
        /// <param name="o">An object of a type contained by the assembly.</param>
        /// <param name="path">The path of the file relative to the project.</param>
        /// <returns>The text content of the file.</returns>
        public static string ReadAllText(object o, string path)
            => ReadAllText(o.GetType().Assembly, path);

        /// <summary>
        /// Opens an assembly project text file, reads all the text in the file, and then closes the file.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="path">The path of the file relative to the project.</param>
        /// <returns>The text content of the file.</returns>
        public static string ReadAllText(Assembly assembly, string path)
        {
            var filepath = GetFullPath(assembly, path);
            try
            {
                return File.ReadAllText(filepath);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"There is no resource corresponding to \"{path}\" at \"{filepath}\".", nameof(path), e);
            }
        }

        /// <summary>
        /// Copies an assembly project text file to a destination.
        /// </summary>
        /// <typeparam name="T">A type contained by the assembly.</typeparam>
        /// <param name="path">The path of the file relative to the project.</param>
        /// <param name="destinationPath">The destination path.</param>
        public static void Copy<T>(string path, string destinationPath)
            => Copy(typeof(T).Assembly, path, destinationPath);

        /// <summary>
        /// Copies an assembly project text file to a destination.
        /// </summary>
        /// <param name="o">An object of a type contained by the assembly.</param>
        /// <param name="path">The path of the file relative to the project.</param>
        /// <param name="destinationPath">The destination path.</param>
        public static void Copy(object o, string path, string destinationPath)
            => Copy(o.GetType().Assembly, path, destinationPath);

        /// <summary>
        /// Copies an assembly project text file to a destination.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="path">The path of the file relative to the project.</param>
        /// <param name="destinationPath">The destination path.</param>
        public static void Copy(Assembly assembly, string path, string destinationPath)
        {
            var sourcePath = GetFullPath(assembly, path);
            if (!File.Exists(sourcePath))
                throw new ArgumentException($"There is no resource corresponding to \"{path}\" at \"{sourcePath}\".", nameof(path));

            try
            {
                File.Copy(sourcePath, destinationPath, true);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to copy resource corresponding to \"{path}\" at \"{sourcePath}\" to \"{destinationPath}\".", nameof(path), e);
            }
        }

        /// <summary>
        /// Gets the full path of an assembly project file.
        /// </summary>
        /// <typeparam name="T">A type contained by the assembly.</typeparam>
        /// <param name="paths">The parts of path of the file relative to the project.</param>
        /// <returns>The full path to the file.</returns>
        public static string GetFullPath<T>(params string[] paths)
            => GetFullPath(typeof(T).Assembly, paths);
        
        /// <summary>
        /// Gets the full path of an assembly project file.
        /// </summary>
        /// <param name="o">An object of a type contained by the assembly.</param>
        /// <param name="paths">The parts of path of the file relative to the project.</param>
        /// <returns>The full path to the file.</returns>
        public static string GetFullPath(object o, params string[] paths)
            => GetFullPath(o.GetType().Assembly, paths);
        
        /// <summary>
        /// Gets the full path of an assembly project file.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="paths">The parts of the path of the file relative to the project.</param>
        /// <returns>The full path to the file.</returns>
        public static string GetFullPath(Assembly assembly, params string[] paths)
        {
            var assemblyLocation = Path.GetDirectoryName(Path.GetFullPath(assembly.Location));
            if (assemblyLocation == null) throw new ArgumentException($"Could not locate assembly \"{assembly.FullName}\".");

            // assembly location is src/<project>/bin/<configuration>/<target>
            var projectLocation = Path.Combine(assemblyLocation, "../../..");
            var solutionLocation = Path.Combine(projectLocation, "../..");

            var path = Path.Combine(paths);

            string fileLocation;
            if (path.StartsWith("temp:"))
            {
                // path is relative to the temp directory
                fileLocation = Path.Combine(solutionLocation, "temp", path["temp:".Length..]);
            }
            else if (path.StartsWith("res:"))
            {
                // path is relative to the src/<project>/Resources directory
                fileLocation = Path.Combine(projectLocation, "Resources", path["res:".Length..]);
            }
            else
            {
                // path is relative to the src/<project>/Resources directory
                fileLocation = Path.Combine(projectLocation, "Resources", path);
            }

            return Path.GetFullPath(fileLocation);
        }
    }
}

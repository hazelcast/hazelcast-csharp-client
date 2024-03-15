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

using System.Text;

namespace Hazelcast.CodeGenerator;

/// <summary>
/// Edits files.
/// </summary>
public class FileEditor
{
    private readonly string _slnPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileEditor"/> class.
    /// </summary>
    /// <param name="slnPath">The path to the solution root.</param>
    public FileEditor(string slnPath)
    {
        _slnPath = slnPath;
    }

    /// <summary>
    /// Copies lines to next 'generated' tag, then skips generated content.
    /// </summary>
    /// <param name="text">The destination <see cref="StringBuilder"/>.</param>
    /// <param name="source">The source lines.</param>
    /// <param name="i">The current line number.</param>
    private static void CopiesToNext(StringBuilder text, string[] source, ref int i)
    {
        while (i < source.Length && source[i].Trim() != "// <generated>")
        {
            text.AppendLine(source[i]);
            i++;
        }

        if (i == source.Length) throw new Exception("Failed to find next <generated> tag.");

        text.AppendLine(source[i]);
        text.AppendLine();

        while (i < source.Length && source[i].Trim() != "// </generated>")
        {
            i++;
        }

        if (i == source.Length) throw new Exception("Failed to find </generated> tag.");
    }

    /// <summary>
    /// Copies the rest of the lines.
    /// </summary>
    /// <param name="text">The destination <see cref="StringBuilder"/>.</param>
    /// <param name="source">The source lines.</param>
    /// <param name="i">The current line number.</param>
    private static void CopyRemains(StringBuilder text, string[] source, ref int i)
    {
        while (i < source.Length)
        {
            text.AppendLine(source[i++]);
        }
    }

    /// <summary>
    /// Edit a file.
    /// </summary>
    /// <param name="filename">The name of the file.</param>
    /// <param name="actions">Actions for generating each 'generated' block.</param>
    public void EditFile(string filename, params Action<StringBuilder>[] actions)
    {
        Console.WriteLine($"Edit {Path.GetFileName(filename)}.");
        var filepath = Path.Combine(_slnPath, filename);
        if (!File.Exists(filepath))
        {
            Console.WriteLine($"ERR: File not found: {filepath}");
            return;
        }

        var source = File.ReadAllLines(filepath);
        var text = new StringBuilder();
        var i = 0;
        foreach (var action in actions)
        {
            CopiesToNext(text, source, ref i);
            action(text);
            var j = text.Length - 1;
            if (text[j] != '\n') // ensure it ends with a nl
            {
                text.AppendLine();
                j = text.Length - 1;
            }
            if (text[j] == '\n') j--; // now it should end with a nl
            while (text[j] == '\r' || text[j] == ' ') j--; // maybe also this
            if (text[j] != '\n') text.AppendLine(); // we *want* an empty line
        }
        CopyRemains(text, source, ref i);

        File.WriteAllText(filepath, text.ToString());
        Console.WriteLine("Edited.");
    }
}
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.DocAsCode.Build.Common;
using Microsoft.DocAsCode.Build.ConceptualDocuments;
using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.DataContracts.ManagedReference;
using Microsoft.DocAsCode.Plugins;

namespace Hazelcast.Net.DocAsCode
{
    [Export(nameof(ConceptualDocumentProcessor), typeof(IDocumentBuildStep))]
    // ReSharper disable once UnusedMember.Global -- injected
    public class BuildHazelcastOptions : BaseDocumentBuildStep
    {
        public override string Name => nameof(BuildHazelcastOptions);
        public override int BuildOrder => Constants.BuildOrder.BuildOptions;

        [Import]
        public HazelcastOptionsState State { get; set; }

        public override IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host)
        {
            State.Gathered.Wait();

            var built = false;

            foreach (var model in models)
            {
                if (model.Type != DocumentType.Article) continue;

                if (!(model.Content is Dictionary<string, object> dict) ||
                    !dict.TryGetValue<string>(Constants.ConceptualKey, out var text) ||
                    !dict.TryGetValue<bool>(Constants.BuildOptionsKey, out var build) ||
                    !build) continue;

                Logger.LogInfo($" Built options page: {model.Key}.");
                built = true;

                // have to update conceptual during pre-build: during build it is too late, Markdown has been processed already

                dict[Constants.ConceptualKey] = UpdateMarkdown(text);
            }

            if (!built)
                Logger.LogWarning(" Found no options page to build.");

            return models;
        }

        private string UpdateMarkdown(string text)
        {
            static string TrimNoise(string s)
            {
                s = s.Substring("obj/dev/api/".Length);
                s = s.Substring(0, s.Length - ".yml".Length);
                return s;
            }

            var options = State.OptionFiles.ToDictionary(
                x => TrimNoise(x.File),
                x => x.Content as PageViewModel);

            var textBuilder = new StringBuilder(text);

            // wrap everything into a div that will get rid of <thead> elements,
            // because we cannot prevent markdown from generating them (else it
            // does not recognize the |...|...| as a table)
            textBuilder.AppendLine("> [!div class=\"xoptions\"]");
            // and then everything must be '> ' prefixed

            textBuilder.AppendLine(">");
            textBuilder.AppendLine("> See @Hazelcast.HazelcastOptions for full documentation of the class.");
            textBuilder.AppendLine(">  ");
            textBuilder.AppendLine(">  <br><br><br>");
            textBuilder.AppendLine(">  ");

            if (options.TryGetValue("Hazelcast.HazelcastOptions", out var root))
            {
                AppendClass(textBuilder, root, options);
            }
            else
            {
                Logger.LogWarning("Could not find class Hazelcast.HazelcastOptions.");

                textBuilder.AppendLine("> **error**: could not find class Hazelcast.HazelcastOptions.");
                textBuilder.AppendLine("> ");
                textBuilder.AppendLine("> Classes:");
                textBuilder.AppendLine("> ");
                foreach (var o in options)
                {
                    textBuilder.Append("> *");
                    textBuilder.AppendLine(o.Key);
                }

                if (options.Count == 0)
                {
                    textBuilder.AppendLine("> * **none**");
                }
            }

            return textBuilder.ToString();
        }

        private static void AppendClass(StringBuilder textBuilder, PageViewModel model, Dictionary<string, PageViewModel> models, int depth = 0, string path = "")
        {
            var header = false;

            var classItem = model.Items.FirstOrDefault(x => x.Type == MemberType.Class);
            if (classItem != null && !string.IsNullOrWhiteSpace(classItem.Remarks))
            {
                textBuilder.Append("> ");
                textBuilder.AppendLine(FormatRemarks(classItem.Remarks));
                textBuilder.AppendLine("> ");
            }

            foreach (var item in model.Items.Where(x => x.Type == MemberType.Property))
            {
                var returnType = item.Syntax.Return.Type;
                if (returnType.EndsWith("Options")) continue; // nested option class

                if (!header)
                {
                    textBuilder.AppendLine("> |Property|Summary|");
                    textBuilder.AppendLine("> |-|-|");
                    header = true;
                }

                // there should not be line breaks in summary
                var summary = item.Summary ?? "";
                summary = summary.Trim()
                    .Replace("\r", "")
                    .Replace("\n", " ");


                if (string.IsNullOrWhiteSpace(item.Remarks))
                    summary = $"<p>{summary}</p>";
                else
                    summary = $"<p style=\"margin-bottom: 4px;\">{summary}</p>" + FormatRemarks(item.Remarks);

                //if (item.Examples != null)
                //{
                //    foreach (var example in item.Examples)
                //    {
                //        summary += "<br>" + example.Trim().Replace("\r", "").Replace("\n", " ");
                //    }
                //}

                textBuilder.Append("> |@");
                textBuilder.Append(item.FullName);
                textBuilder.Append("|");
                textBuilder.Append(summary);
                textBuilder.AppendLine("|");
            }

            textBuilder.AppendLine("> ");

            foreach (var item in model.Items.Where(x => x.Type == MemberType.Property))
            {
                var returnType = item.Syntax.Return.Type;
                if (!returnType.EndsWith("Options")) continue; // nested option class

                textBuilder.Append("> ");
                for (var i = 0; i < 2 + depth; i++) textBuilder.Append("#");
                textBuilder.Append(" <a name=\"");
                textBuilder.Append(item.Name.ToLowerInvariant());
                textBuilder.Append("\" />");
                textBuilder.AppendLine(item.Name);
                textBuilder.AppendLine("> ");
                textBuilder.Append("> Access @");
                textBuilder.Append(returnType);
                textBuilder.Append(" via options.");
                textBuilder.Append(path);
                textBuilder.Append("@");
                textBuilder.Append(item.FullName);
                textBuilder.AppendLine(".");
                textBuilder.AppendLine("> ");

                if (models.TryGetValue(returnType, out var nested))
                {
                    AppendClass(textBuilder, nested, models, depth + 1, path + item.Name + ".");
                }
                else
                {
                    textBuilder.Append("> **error**: could not find class ");
                    textBuilder.Append(returnType);
                    textBuilder.AppendLine(".");
                }
            }

            textBuilder.AppendLine(">");
            textBuilder.AppendLine(">");
        }

        private static string FormatRemarks(string remarks)
        {
            var builder = new StringBuilder();

            // there should not be line breaks in remarks, except for paragraphs
            // at that point, we get html where <para> and <code> have already been
            // turned into the corresponding <p> and <pre> html tags

            var blocks = Regex.Split(remarks, "<(?:|/)pre>");
            for (var i = 0; i < blocks.Length; i++)
            {
                var remark = blocks[i].Replace("\r", "");

                if (i % 2 == 0)
                {
                    builder.Append(remark.Replace("\n", " "));
                }
                else
                {
                    builder.Append("<pre>");
                    builder.Append(remark.Replace("\n", "<br>"));
                    builder.Append("</pre>");
                }
            }

            return builder.ToString();
        }
    }
}

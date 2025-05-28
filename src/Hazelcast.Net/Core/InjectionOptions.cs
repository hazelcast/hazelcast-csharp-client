// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents an option for an injected instance.
    /// </summary>
    internal class InjectionOptions
    {
        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        public string TypeName { get; set;}

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        public Dictionary<string, string> Args { get; } = new Dictionary<string, string>();

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.Append(GetType().Name);
            text.Append(" typeName: '");
            text.Append(TypeName ?? "<null>");
            text.Append('\'');

            ToString(text);

            if (Args != null)
            {
                foreach (var (argKey, argValue) in Args)
                {
                    text.Append(", ");
                    text.Append(argKey);
                    text.Append(": '");
                    text.Append(argValue ?? "<null>");
                    text.Append('\'');
                }
            }

            return text.ToString();
        }

        protected virtual void ToString(StringBuilder text)
        { }
    }
}

// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;
using System.Text.RegularExpressions;
using Hazelcast.Data;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Provides extension methods to the <see cref="IndexConfig"/> class.
    /// </summary>
    public static class IndexConfigExtensions
    {
        // TODO: document, explain, refactor

        /** Maximum number of attributes allowed in the index. */
        private const int MaxAttributes = 255;

        /** Regex to stripe away "this." prefix. */
        private static readonly Regex ThisPattern = new Regex("^this\\.");

        public static IndexConfig ValidateAndNormalize(this IndexConfig config, string mapName)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            // Validate attributes.
            var originalAttributeNames = config.Attributes;
            if (originalAttributeNames.Count == 0)
            {
                throw new ArgumentException($"Index must have at least one attribute: {config}");
            }
            if (originalAttributeNames.Count > MaxAttributes)
            {
                throw new ArgumentException($"Index cannot have more than {MaxAttributes}  attributes: {config}");
            }
            var normalizedAttributeNames = new List<string>(originalAttributeNames.Count);
            foreach (var originalAttributeName in originalAttributeNames)
            {
                var attributeName = originalAttributeName.Trim();
                ValidateAttribute(config, attributeName);
                var normalizedAttributeName = CanonicalizeAttribute(attributeName);
                var existingIdx = normalizedAttributeNames.IndexOf(normalizedAttributeName);
                if (existingIdx != -1)
                {
                    var duplicateOriginalAttributeName = originalAttributeNames[existingIdx];

                    if (duplicateOriginalAttributeName == attributeName)
                    {
                        throw new ArgumentException($"Duplicate attribute name [attributeName={attributeName}, indexConfig={config} ]");
                    }
                    throw new ArgumentException(
                        $"Duplicate attribute names [attributeName1={duplicateOriginalAttributeName}, attributeName2={attributeName}, indexConfig={config}]");
                }
                normalizedAttributeNames.Add(normalizedAttributeName);
            }
            // Construct final index.
            var name = config.Name;
            if (name != null && name.Trim().Length == 0)
            {
                name = null;
            }
            return BuildNormalizedConfig(mapName, config.Type, name, normalizedAttributeNames);
        }

        private static IndexConfig BuildNormalizedConfig(string mapName, IndexType indexType, string indexName,
            List<string> normalizedAttributeNames)
        {
            var newConfig = new IndexConfig { Type = indexType };

            var nameBuilder = indexName == null ? new StringBuilder(mapName + "_" + GetIndexTypeName(indexType)) : null;

            foreach (var normalizedAttributeName in normalizedAttributeNames)
            {
                newConfig.AddAttribute(normalizedAttributeName);
                nameBuilder?.Append("_").Append(normalizedAttributeName);
            }
            if (nameBuilder != null)
                indexName = nameBuilder.ToString();
            {
            }
            newConfig.Name = indexName;
            return newConfig;
        }

        private static void ValidateAttribute(IndexConfig config, string attributeName)
        {
            if (string.IsNullOrEmpty(attributeName) || attributeName.Trim().EndsWith(".", StringComparison.Ordinal))
                throw new ArgumentException($"Invalid attribute '{attributeName}' in index configuration '{config.Name}'. " +
                                            "Attributes must not be null nor empty, nor end with a dot.");
        }

        private static string CanonicalizeAttribute(string attribute)
        {
            return ThisPattern.Replace(attribute, "");
        }

        private static string GetIndexTypeName(IndexType indexType)
        {
            return indexType switch
            {
                IndexType.Sorted => "sorted",
                IndexType.Hashed => "hash",
                _ => throw new ArgumentException($"Unsupported index type: {indexType}")
            };
        }
    }
}

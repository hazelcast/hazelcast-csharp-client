using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Hazelcast.Configuration;

namespace Hazelcast.DistributedObjects.Implementation
{
    /// <summary>
    /// Provides extension methods to the <see cref="IndexConfig"/> class.
    /// </summary>
    public static class IndexConfigExtensions
    {
        // TODO: document, explain, refactor

        /** Maximum number of attributes allowed in the index. */
        private static readonly int MaxAttributes = 255;

        /** Regex to stripe away "this." prefix. */
        private static readonly Regex ThisPattern = new Regex("^this\\.");

        public static IndexConfig ValidateAndNormalize(this IndexConfig config, string mapName)
        {
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
            if (attributeName == null)
            {
                throw new NullReferenceException($"Attribute name cannot be null: {config}");
            }

            var attributeName0 = attributeName.Trim();

            if (attributeName0.Length == 0)
            {
                throw new ArgumentException($"Attribute name cannot be empty: {config}");
            }

            if (attributeName0.EndsWith("."))
            {
                throw new ArgumentException($"Attribute name cannot end with dot: {attributeName}");
            }
        }

        private static string CanonicalizeAttribute(string attribute)
        {
            return ThisPattern.Replace(attribute, "");
        }

        private static string GetIndexTypeName(IndexType indexType)
        {
            switch (indexType)
            {
                case IndexType.Sorted:
                    return "sorted";

                case IndexType.Hashed:
                    return "hash";

                default:
                    throw new ArgumentException($"Unsupported index type: {indexType}");
            }
        }
    }
}

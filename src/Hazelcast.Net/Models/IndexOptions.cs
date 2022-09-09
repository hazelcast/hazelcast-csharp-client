﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;

namespace Hazelcast.Models
{
    /// <summary>
    /// Configuration of an index.
    /// </summary>
    /// <remarks>
    /// <para>Hazelcast support three types of indexes: sorted, hash and bitmap indexes. They can be
    /// created on one or more attributes, specified by their name.</para>
    /// <para>Sorted indexes can be used with equality and range predicates and have logarithmic
    /// search time.</para>
    /// <para>Hash indexes can be used with equality predicates and have constant search time assuming
    /// the hash function of the indexed field disperses the elements properly.</para>
    /// <para>Bitmap indexes (to be completed).</para>
    /// </remarks>
    public class IndexOptions
    {
        public static readonly IndexType DefaultType = IndexType.Sorted;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexOptions"/> class.
        /// </summary>
        public IndexOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexOptions"/> class.
        /// </summary>
        public IndexOptions(IEnumerable<string> attributes)
        {
            Attributes = attributes.ToList();
        }

        /// <summary>
        /// Gets or sets the name of the index.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the index.
        /// </summary>
        public IndexType Type { get; set; } = DefaultType;

        /// <summary>
        /// Gets the indexed attributes.
        /// </summary>
        public IList<string> Attributes { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the bitmap index options.
        /// </summary>
        public BitmapIndexOptions BitmapIndexOptions { get; set; } = new BitmapIndexOptions();

        /// <summary>
        /// Adds an indexed attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <returns>This instance for chaining.</returns>
        public IndexOptions AddAttribute(string attribute)
        {
            ValidateAttribute(this, attribute);
            Attributes.Add(attribute);
            return this;
        }

        /// <summary>
        /// Adds indexed attributes.
        /// </summary>
        /// <param name="attributes">The names of the attributes.</param>
        /// <returns>This instance for chaining.</returns>
        public IndexOptions AddAttributes(params string[] attributes)
        {
            if (attributes is null) throw new ArgumentNullException(nameof(attributes));

            foreach (var attribute in attributes)
            {
                ValidateAttribute(this, attribute);
            }

            foreach (var attribute in attributes)
            {
                Attributes.Add(attribute);
            }
            return this;
        }

        internal static void ValidateAttribute(IndexOptions options, string attributeName)
        {
            if (attributeName == null)
                throw new ArgumentNullException(nameof(attributeName), $"Attribute name cannot be null: {options}");

            attributeName = attributeName.Trim();

            if (attributeName.Length == 0)
                throw new ArgumentException($"Attribute name cannot be empty: {options}", nameof(attributeName));

            if (attributeName.EndsWith(".", StringComparison.Ordinal))
                throw new ArgumentException($"Attribute name cannot end with dot: {attributeName}", nameof(attributeName));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"IndexConfig[Name={Name}, IndexType= {Type}, Attributes={string.Join(",", Attributes)}]";
        }
    }
}

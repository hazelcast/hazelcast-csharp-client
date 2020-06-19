﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using JetBrains.Annotations;

// ReSharper disable UnusedMember.Global - normal
// ReSharper disable InconsistentNaming - required

#pragma warning disable IDE1006 // Naming Styles - required
#pragma warning disable IDE0060 // Remove unused parameter - required

namespace Hazelcast.Net.JetBrainsAnnotations
{
    // https://www.jetbrains.com/help/resharper/Postfix_Templates.html#
    // https://www.jetbrains.com/help/resharper/Source_Templates.html#
    // https://www.jetbrains.com/help/resharper/Code_Analysis__Annotations_in_Source_Code.html#

    /// <summary>
    /// Defines source templates for ReSharper.
    /// </summary>
    public static class SourceTemplates
    {
        [SourceTemplate]
        public static void argNullOrEmpty(this string s)
        {
            //$if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(s))
        }

        [SourceTemplate]
        public static void argNull(this object o)
        {
            //$if (o == null) throw new ArgumentNullException(nameof(o))
        }
    }
}

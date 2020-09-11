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

using System.Diagnostics.CodeAnalysis;

#if CODE_ANALYSIS

// this file contains assembly-level SuppressMessage attributes for NDepend,
// they will be compiled into the assembly only if CODE_ANALYSIS is defined
// and should not be compiled into the final Release version that ships.

// scope can be: deep module namespace type method field
// and also: namespaceAndDescendants
//
// If no scope is specified, only issues related to code element tagged will
// be suppressed. For example when you suppress issues on a class, it suppresses
// the issues against the class itself. It does not suppress the issues
// against methods or fields within the class.
//
// The scope value "deep" cannot be used with another scope value. It means:
//  for an assembly: "module namespace type method field
//  for a namespace: "namespace type method field"
//  for a type: "type method field"
//  for a method: "method"
//  for a field: "field"
//
// MS now has an analyzer for the SuppressMessage attribute which quite does
// not like 'deep' scope
// MS also uses 'member' which NDepend ignores

// The Target property of the SuppressMessage attribute follows Roslyn's conventions
// see: https://github.com/dotnet/csharplang/blob/master/spec/documentation-comments.md#id-string-format
//
// E event
// F field
// M method, ctor, operator...
// N namespace
// P property, indexer...
// T type
// ! error
//
// so: "~X:Name"

// Unfortunately, NDepend chokes on them :(
#pragma warning disable IDE0077 // Avoid legacy format target in 'SuppressMessageAttribute'

// Unfortunately, NDepend uses e.g. 'method' scope which is not OK
#pragma warning disable IDE0076 // Invalid global 'SuppressMessageAttribute'

// NDepend complains about 'public' methods in an 'internal' class
// but even NDepend documentation mentions that not everyone agrees
// see http://ericlippert.com/2014/09/15/internal-or-public/
// we *do* use 'public' methods in 'internal' classes, so, suppress
[assembly: SuppressMessage("NDepend", "ND1807:AvoidPublicMethodsNotPubliclyVisible",
    Justification = "Accepted."
)]

// We use System.Random in our RandomProvider and don't expect it
// to be used for anything security-related, so, suppress
[assembly: SuppressMessage("NDepend", "ND3101:DontUseSystemRandomForSecurityPurposes",
    Target = /*~M:*/ "Hazelcast.Core.RandomProvider.NewRandom()",
    Scope = "method",
    Justification = "Not used for security purposes.")]
[assembly: SuppressMessage("NDepend", "ND3101:DontUseSystemRandomForSecurityPurposes",
    Target = /*~M:*/ "Hazelcast.Core.RandomProvider..cctor()",
    Scope = "method",
    Justification = "Not used for security purposes.")]

// collides with System.Reflection.MemberInfo but really, it's
// two different worlds and we can use MemberInfo too
[assembly: SuppressMessage("NDepend", "ND2012:AvoidHavingDifferentTypesWithSameName",
    Target = /*~T:*/ "Hazelcast.Data.MemberInfo",
    Scope = "type",
    Justification = "Accepted.")]

// codecs are what they are, not much we want to do about it for now
[assembly: SuppressMessage("NDepend", "",
    Target = /*~N:*/ "Hazelcast.Protocol.Codecs",
    Scope = "namespaceAndDescendants",
    Justification = "Exclude codecs from code analysis.")]
[assembly: SuppressMessage("NDepend", "",
    Target = /*~N:*/ "Hazelcast.Protocol.BuiltInCodecs",
    Scope = "namespaceAndDescendants",
    Justification = "Exclude codecs from code analysis.")]
[assembly: SuppressMessage("NDepend", "",
    Target = /*~N:*/ "Hazelcast.Protocol.CustomCodecs",
    Scope = "namespaceAndDescendants",
    Justification = "Exclude codecs from code analysis.")]

// JavaUuidOrder is a special struct that is used for (de) serializing Guid
// and therefore used in twisted ways, and this is intentional
[assembly: SuppressMessage("NDepend", "ND1903:StructuresShouldBeImmutable",
    Target = /*~T:*/ "Hazelcast.Core.JavaUuidOrder",
    Scope = "type",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND1905:AFieldMustNotBeAssignedFromOutsideItsParentHierarchyTypes",
    Target = "Hazelcast.Core.JavaUuidOrder.Value",
    Scope = "method",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND2000:InstanceFieldsNamingConvention",
    Target = "Hazelcast.Core.JavaUuidOrder.Value",
    Scope = "method",
    Justification = "Accepted.")]


// do *not* suppress that one but use a customized rule
/*
[assembly: SuppressMessage("NDepend",
    "ND2016:MethodsPrefixedWithTryShouldReturnABoolean",
    Scope = "method",
    Justification="We have many TryXxx methods that return Attempt or are async.")]
*/

// suppress issues with our System.* extensions for dealing with netstandard versions
// each namespace needs its rule

[assembly: SuppressMessage("NDepend", "",
    Target = /*~N:*/ "System",
    Scope = "namespaceAndDescendants",
    Justification = "Exclude MS code from analysis.")]
[assembly: SuppressMessage("NDepend", "",
    Target = /*~N:*/ "System.IO",
    Scope = "namespaceAndDescendants",
    Justification = "Exclude MS code from analysis.")]
[assembly: SuppressMessage("NDepend", "",
    Target = /*~N:*/ "System.Buffers",
    Scope = "namespaceAndDescendants",
    Justification = "Exclude MS code from analysis.")]
[assembly: SuppressMessage("NDepend", "",
    Target = /*~N:*/ "System.Threading.Tasks",
    Scope = "namespaceAndDescendants",
    Justification = "Exclude MS code from analysis.")]
[assembly: SuppressMessage("NDepend", "",
    Target = /*~N:*/ "System.Collections.Generic",
    Scope = "namespaceAndDescendants",
    Justification = "Exclude MS code from analysis.")]
[assembly: SuppressMessage("NDepend", "",
    Target = /*~N:*/ "System.Diagnostics.CodeAnalysis",
    Scope = "namespaceAndDescendants",
    Justification = "Accepted.")]
[assembly:SuppressMessage("NDepend", "",
    Target = /*~N:*/ "System.Runtime.CompilerServices",
    Scope = "namespaceAndDescendants",
    Justification = "Imported code.")]

#if NETSTANDARD2_0
[assembly: SuppressMessage("NDepend", "",
    Target = /*~T:*/ "System.Runtime.CompilerServices.RuntimeHelpersEx",
    Scope = "type",
    Justification = "Imported code.")]
[assembly: SuppressMessage("NDepend", "",
    Target = /*~N:*/ "System.Collections.Concurrent",
    Scope = "namespaceAndDescendants",
    Justification = "Imported code.")]
#endif

#endif

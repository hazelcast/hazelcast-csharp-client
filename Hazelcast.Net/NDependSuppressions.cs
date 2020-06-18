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

// NDepend complains about 'public' methods in an 'internal' class
// but even NDepend documentation mentions that not everyone agrees
// see http://ericlippert.com/2014/09/15/internal-or-public/
// we *do* use 'public' methods in 'internal' classes, so, suppress
[assembly: SuppressMessage("NDepend", "ND1807:AvoidPublicMethodsNotPubliclyVisible",
    Scope = "deep",
    Justification = "Accepted."
)]

// We use System.Random in our RandomProvider and don't expect it
// to be used for anything security-related, so, suppress
[assembly: SuppressMessage("NDepend", "ND3101:DontUseSystemRandomForSecurityPurposes",
    Target = "Hazelcast.Core.RandomProvider",
    Scope = "deep",
    Justification = "Not used for security purposes.")]

// collides with System.Reflection.MemberInfo but really, it's
// two different worlds and we can use MemberInfo too
[assembly: SuppressMessage("NDepend", "ND2012:AvoidHavingDifferentTypesWithSameName",
    Target = "Hazelcast.Data.MemberInfo",
    Justification = "Accepted.")]

// codecs are what they are, not much we want to do about it
[assembly: SuppressMessage("NDepend", "ND2011:AvoidFieldsWithNameTooLong",
    Target = "Hazelcast.Protocol.Codecs",
    Scope = "deep",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND2300:CollectionPropertiesShouldBeReadOnly",
    Target = "Hazelcast.Protocol.Codecs",
    Scope = "deep",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND1801:TypesThatCouldHaveALowerVisibility",
    Target = "Hazelcast.Protocol.Codecs",
    Scope = "deep",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND1206:AStatelessClassOrStructureMightBeTurnedIntoAStaticType",
    Target = "Hazelcast.Protocol.Codecs",
    Scope = "deep",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND1306:NestedTypesShouldNotBeVisible",
    Target = "Hazelcast.Protocol.Codecs",
    Scope = "deep",
    Justification = "Accepted.")]

// JavaUuidOrder is a special struct that is used for (de) serializing Guid
// and therefore used in twisted ways, and this is intentional
[assembly: SuppressMessage("NDepend", "ND1903:StructuresShouldBeImmutable",
    Target = "Hazelcast.Core.JavaUuidOrder",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND1905:AFieldMustNotBeAssignedFromOutsideItsParentHierarchyTypes",
    Target = "Hazelcast.Core.JavaUuidOrder.Value",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND2000:InstanceFieldsNamingConvention",
    Target = "Hazelcast.Core.JavaUuidOrder.Value",
    Justification = "Accepted.")]

// some collections *need* to be set
[assembly: SuppressMessage("NDepend", "ND2300:CollectionPropertiesShouldBeReadOnly",
    Target = "Hazelcast.Configuration.HazelcastCommandLineConfigurationSource.set_SwitchMappings(IDictionary<String,String>)",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND2300:CollectionPropertiesShouldBeReadOnly",
    Target = "Hazelcast.Core.InjectionOptions.set_Args(Dictionary<String,String>)",
    Justification = "Accepted.")]

// do *not* suppress that one but use a customized rule
/*
[assembly: SuppressMessage("NDepend",
    "ND2016:MethodsPrefixedWithTryShouldReturnABoolean",
    Scope = "method",
    Justification="We have many TryXxx methods that return Attempt or are async.")]
*/

// suppress issues with our System.* extensions for dealing with netstandard versions
// note that 'System + deep' is not supported yet, each namespace needs its rule
[assembly: SuppressMessage("NDepend", "ND1400:AvoidNamespacesMutuallyDependent",
    Target = "System.Runtime.CompilerServices.RuntimeHelpersEx",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND2103:NamespaceNameShouldCorrespondToFileLocation",
    Target = "System",
    Scope = "deep",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND2103:NamespaceNameShouldCorrespondToFileLocation",
    Target = "System.IO",
    Scope = "deep",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND2103:NamespaceNameShouldCorrespondToFileLocation",
    Target = "System.Buffers",
    Scope = "deep",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND2103:NamespaceNameShouldCorrespondToFileLocation",
    Target = "System.Threading.Tasks",
    Scope = "deep",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND2103:NamespaceNameShouldCorrespondToFileLocation",
    Target = "System.Collections.Generic",
    Scope = "deep",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND2103:NamespaceNameShouldCorrespondToFileLocation",
    Target = "System.Diagnostics.CodeAnalysis",
    Scope = "deep",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND2104:TypesWithSourceFilesStoredInTheSameDirectoryShouldBeDeclaredInTheSameNamespace",
    Target = "System",
    Scope = "deep",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND2104:TypesWithSourceFilesStoredInTheSameDirectoryShouldBeDeclaredInTheSameNamespace",
    Target = "System.Diagnostics.CodeAnalysis",
    Scope = "deep",
    Justification = "Accepted.")]
[assembly: SuppressMessage("NDepend", "ND2102:AvoidDefiningMultipleTypesInASourceFile",
    Target = "System.Diagnostics.CodeAnalysis",
    Scope = "deep",
    Justification = "Accepted.")]

#endif
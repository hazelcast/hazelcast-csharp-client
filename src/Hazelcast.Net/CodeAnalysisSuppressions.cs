// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

// this file contains assembly-level suppression directives for NDepend,
// they will be compiled into the assembly only if CODE_ANALYSIS is defined
// and should not be compiled into the final Release version that ships.
//
// this file also contains assembly-level suppression directives for the
// compiler (to deal with some warnings) that are always compiled.

// TODO: remove these suppressions (https://github.com/hazelcast/hazelcast-csharp-client/issues/540)
[assembly:SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>")]
[assembly:SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>")]

#if CODE_ANALYSIS

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

#region NDepend / General

// NDepend complains about 'public' methods in an 'internal' class
// but even NDepend documentation mentions that not everyone agrees
// see http://ericlippert.com/2014/09/15/internal-or-public/
// we *do* use 'public' methods in 'internal' classes, so, suppress
[assembly: SuppressMessage(NDepend.Category, NDepend.Check.AvoidPublicMethodsNotPubliclyVisible,
    Justification = NDepend.Justification.Accepted
)]

#endregion

#region NDepend / Accepted Checks

// We use System.Random in our RandomProvider and don't expect it
// to be used for anything security-related, so, suppress
[assembly: SuppressMessage(NDepend.Category, "ND3101:DontUseSystemRandomForSecurityPurposes",
    Target = /*~M:*/ "Hazelcast.Core.RandomProvider.NewRandom()",
    Scope = NDepend.Scope.Method,
    Justification = "Not used for security purposes.")]
[assembly: SuppressMessage(NDepend.Category, "ND3101:DontUseSystemRandomForSecurityPurposes",
    Target = /*~M:*/ "Hazelcast.Core.RandomProvider..cctor()",
    Scope = NDepend.Scope.Method,
    Justification = "Not used for security purposes.")]

// collides with System.Reflection.MemberInfo but really, it's
// two different worlds and we can use MemberInfo too
[assembly: SuppressMessage(NDepend.Category, "ND2012:AvoidHavingDifferentTypesWithSameName",
    Target = /*~T:*/ "Hazelcast.Data.MemberInfo",
    Scope = NDepend.Scope.Type,
    Justification = NDepend.Justification.Accepted)]

// JavaUuidOrder is a special struct that is used for (de) serializing Guid
// and therefore used in twisted ways, and this is intentional
[assembly: SuppressMessage(NDepend.Category, "ND1903:StructuresShouldBeImmutable",
    Target = /*~T:*/ "Hazelcast.Core.JavaUuidOrder",
    Scope = NDepend.Scope.Type,
    Justification = NDepend.Justification.Accepted)]
[assembly: SuppressMessage(NDepend.Category, "ND1905:AFieldMustNotBeAssignedFromOutsideItsParentHierarchyTypes",
    Target = "Hazelcast.Core.JavaUuidOrder.Value",
    Scope = NDepend.Scope.Method,
    Justification = NDepend.Justification.Accepted)]
[assembly: SuppressMessage(NDepend.Category, "ND2000:InstanceFieldsNamingConvention",
    Target = "Hazelcast.Core.JavaUuidOrder.Value",
    Scope = NDepend.Scope.Method,
    Justification = NDepend.Justification.Accepted)]

// do *not* suppress that one but use a customized rule
/*
[assembly: SuppressMessage(NDepend.Category,
    "ND2016:MethodsPrefixedWithTryShouldReturnABoolean",
    Scope = NDepend.Scope.Method,
    Justification="We have many TryXxx methods that return Attempt or are async.")]
*/

#endregion

#region NDepend / Codecs

// TODO: we should use all codecs, or mark them individually
// and so, we should not mark them all as "not dead code"

// codecs are what they are, not much we want to do about it for now
[assembly: SuppressMessage(NDepend.Category, NDepend.Check.All,
    Target = /*~N:*/ "Hazelcast.Protocol.Codecs",
    Scope = NDepend.Scope.NamespaceAndDescendants,
    Justification = "Exclude codecs from code analysis.")]
[assembly: SuppressMessage(NDepend.Category, NDepend.Check.All,
    Target = /*~N:*/ "Hazelcast.Protocol.BuiltInCodecs",
    Scope = NDepend.Scope.NamespaceAndDescendants,
    Justification = "Exclude codecs from code analysis.")]
[assembly: SuppressMessage(NDepend.Category, NDepend.Check.All,
    Target = /*~N:*/ "Hazelcast.Protocol.CustomCodecs",
    Scope = NDepend.Scope.NamespaceAndDescendants,
    Justification = NDepend.Justification.ExcludeCodecs)]

#endregion

#region NDepend / Types Not Dead

[assembly: SuppressMessage(NDepend.Category, NDepend.Check.PotentiallyDeadTypes,
    Target = "Hazelcast.AssemblySigning",
    Justification = NDepend.Justification.TypeNotDead)]

#endregion

#region NDepend / System

// suppress issues with our System.* extensions for dealing with netstandard versions
// each namespace needs its rule

[assembly: SuppressMessage(NDepend.Category, "",
    Target = /*~N:*/ "System",
    Scope = "namespaceAndDescendants",
    Justification = "Exclude MS code from analysis.")]
[assembly: SuppressMessage(NDepend.Category, "",
    Target = /*~N:*/ "System.IO",
    Scope = "namespaceAndDescendants",
    Justification = "Exclude MS code from analysis.")]
[assembly: SuppressMessage(NDepend.Category, "",
    Target = /*~N:*/ "System.Buffers",
    Scope = "namespaceAndDescendants",
    Justification = "Exclude MS code from analysis.")]
[assembly: SuppressMessage(NDepend.Category, "",
    Target = /*~N:*/ "System.Threading.Tasks",
    Scope = "namespaceAndDescendants",
    Justification = "Exclude MS code from analysis.")]
[assembly: SuppressMessage(NDepend.Category, "",
    Target = /*~N:*/ "System.Collections.Generic",
    Scope = "namespaceAndDescendants",
    Justification = "Exclude MS code from analysis.")]
[assembly: SuppressMessage(NDepend.Category, "",
    Target = /*~N:*/ "System.Diagnostics.CodeAnalysis",
    Scope = "namespaceAndDescendants",
    Justification = NDepend.Justification.Accepted)]
[assembly: SuppressMessage(NDepend.Category, "",
    Target = /*~N:*/ "System.Runtime.CompilerServices",
    Scope = "namespaceAndDescendants",
    Justification = "Imported code.")]

#if NETSTANDARD2_0
[assembly: SuppressMessage(NDepend.Category, "",
    Target = /*~T:*/ "System.Runtime.CompilerServices.RuntimeHelpersEx",
    Scope = NDepend.Scope.Type,
    Justification = "Imported code.")]
[assembly: SuppressMessage(NDepend.Category, "",
    Target = /*~N:*/ "System.Collections.Concurrent",
    Scope = "namespaceAndDescendants",
    Justification = "Imported code.")]
#endif

#endregion

#endif

#region Accepted Checks
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage",
    "CA2254:Template should be a static expression",
    Justification = "String interpolations are removed.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
    "CA1848:Use the LoggerMessage delegates",
    Justification = "LoggerMessage is not backward compitable.")]

[assembly: SuppressMessage("Security", "CA5394:Do not use insecure randomness",
    //Scope = "all",
    Justification = "Not used for security purposes.")]

// TODO: all our event args should inherit from EventArgs (breaking)
// (and, Target cannot contain wildcards)
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.Events.DistributedObjectCreatedEventArgs",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.Events.DistributedObjectDestroyedEventArgs",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.Events.MembersUpdatedEventArgs",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.Events.PartitionLostEventArgs",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.Events.DistributedObjectLifecycleEventArgs",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.CollectionItemEventArgs`1",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.MapClearedEventArgs",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.MapEvictedEventArgs",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.MapEntryEvictedEventArgs`2",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.MapEntryAddedEventArgs`2",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.MapEntryExpiredEventArgs`2",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.MapEntryInvalidatedEventArgs`2",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.MapEntryLoadedEventArgs`2",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.MapEntryMergedEventArgs`2",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.MapEntryRemovedEventArgs`2",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.MapEntryUpdatedEventArgs`2",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.TopicMessageEventArgs`1",
    Justification = Analysis.Justification.CA1711_EventArgs)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.StateChangedEventArgs",
    Justification = Analysis.Justification.CA1711_EventArgs)]

// nothing we can do about these
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.ITopicEventHandler`1",
    Justification = Analysis.Justification.CA1711_EventHandler)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.IMapEventHandler`3",
    Justification = Analysis.Justification.CA1711_EventHandler)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.IMapEntryEventHandler`3",
    Justification = Analysis.Justification.CA1711_EventHandler)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.ICollectionItemEventHandler`1",
    Justification = Analysis.Justification.CA1711_EventHandler)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.IHazelcastClientEventHandler",
    Justification = Analysis.Justification.CA1711_EventHandler)]

// nothing we can do about these
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.IHCollection`1",
    Justification = Analysis.Justification.CA1711_Reserved)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.IHQueue`1",
    Justification = Analysis.Justification.CA1711_Reserved)]
[assembly: SuppressMessage(Analysis.Naming, Analysis.CA1711, Scope = Analysis.Scope.Type,
    Target = "~T:Hazelcast.DistributedObjects.IHTxQueue`1",
    Justification = Analysis.Justification.CA1711_Reserved)]

#endregion

// ReSharper disable once CheckNamespace
internal static class Analysis
{
    public const string Naming = "Naming";
    public const string Style = "Style";

    // ReSharper disable InconsistentNaming
    public const string CA1711 = "CA1711:Identifiers should not have incorrect suffix";
    public const string IDE0066 = "IDE0066:Convert switch statement to expression";
    // ReSharper restore InconsistentNaming

    public static class Scope
    {
        public const string Type = "type";
    }

    public static class Justification
    {
        // ReSharper disable InconsistentNaming
        public const string CA1711_EventArgs = "Our event args should inherit from System.EventArgs but this is a breaking change";
        public const string CA1711_EventHandler = "Our event handlers cannot be true event-handler delegates.";
        public const string CA1711_Reserved = "We have to use some names that are reserved.";
        // ReSharper restore InconsistentNaming
    }
}

// ReSharper disable once CheckNamespace
internal static class NDepend
{
    public const string Category = "NDepend";

    public static class Check
    {
        public const string All = "";
        public const string PotentiallyDeadTypes = "ND1700:PotentiallyDeadTypes";
        public const string AvoidPublicMethodsNotPubliclyVisible = "ND1807:AvoidPublicMethodsNotPubliclyVisible";
    }

    public static class Scope
    {
        public const string Deep = "deep";
        public const string Module = "module";
        public const string Namespace = "namespace";
        public const string Type = "type";
        public const string Method = "method";
        public const string Field = "field";
        public const string NamespaceAndDescendants = "namespaceAndDescendants";
    }

    public static class Justification
    {
        public const string Accepted = "Accepted.";
        public const string ExcludeCodecs = "Exclude codecs from analyzis.";
        public const string TypeNotDead = "Not dead.";
    }
}

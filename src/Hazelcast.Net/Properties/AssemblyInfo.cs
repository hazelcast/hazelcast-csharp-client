﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Runtime.CompilerServices;

// NOTE: do NOT make this assembly visible to Hazelcast.Net.Examples, as
// the whole point of examples is to show how *users* can use the library!

#if ASSEMBLY_SIGNING

using Hazelcast;

[assembly: InternalsVisibleTo("Hazelcast.Net.Tests, PublicKey=" + AssemblySigning.PublicKey)]
[assembly: InternalsVisibleTo("Hazelcast.Net.Testing, PublicKey=" + AssemblySigning.PublicKey)]
[assembly: InternalsVisibleTo("hb, PublicKey=" + AssemblySigning.PublicKey)]
[assembly: InternalsVisibleTo("Hazelcast.Net.DependencyInjection, PublicKey=" + AssemblySigning.PublicKey)]
[assembly: InternalsVisibleTo("Hazelcast.Net.Caching, PublicKey=" + AssemblySigning.PublicKey)]
[assembly: InternalsVisibleTo("Hazelcast.Net.Linq.Async, PublicKey=" + AssemblySigning.PublicKey)]

// see https://github.com/Moq/moq4/wiki/Quickstart and https://stackoverflow.com/questions/30089042
// we need the full public key here in order to be able to use Moq when our assemblies are signed
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2,PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

#else

[assembly: InternalsVisibleTo("Hazelcast.Net.Tests")]
[assembly: InternalsVisibleTo("Hazelcast.Net.Testing")]
[assembly: InternalsVisibleTo("hb")]
[assembly: InternalsVisibleTo("Hazelcast.Net.DependencyInjection")]
[assembly: InternalsVisibleTo("Hazelcast.Net.Caching")]
[assembly: InternalsVisibleTo("Hazelcast.Net.Linq.Async")]

// moq
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

#endif

// We propose to accept that the code is not CLS Compliant anymore (remove
// the CLSCompliant attribute), i.e. to stop actively indicating that the
// code complies with the Common Language Specification (CLS) and can be
// considered language-independent. There are great debates in the .NET
// community about whether this is important or not, but the main argument
// is that Microsoft has dropped explicit compliance from most of its
// Microsoft.Extensions.* library, and .NET Core.
//
// This does not mean that the Hazelcast library is not language-independent.
// Only, we don't indicate that we are, because in order to do so we would
// need to drop all dependencies on common Microsoft libraries. However,
// we should aim at being language-independent.
//
// TODO: write for instance VB+F# examples just to be sure
//[assembly: CLSCompliant(true)]

[assembly: SuppressMessage("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant", Justification="Intentional")]

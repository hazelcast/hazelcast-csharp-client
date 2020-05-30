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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Hazelcast.Net.Tests")]
[assembly: InternalsVisibleTo("Hazelcast.Net.Testing")]

[assembly: CLSCompliant(true)]

// NDepend scope can be: deep module namespace type method field

// NDepend complains about 'public' methods in an 'internal' class
// but even NDepend documentation mentions that not everyone agrees
// see http://ericlippert.com/2014/09/15/internal-or-public/
// we *do* use 'public' methods in 'internal' classes, so, suppress
[assembly: SuppressMessage("NDepend",
    "ND1807:AvoidPublicMethodsNotPubliclyVisible",
    Scope = "deep",
    Justification = "Accepted."
)]

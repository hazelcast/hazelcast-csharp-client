

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Hazelcast.Net.Tests")]
[assembly: InternalsVisibleTo("Hazelcast.Net.Testing")]

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

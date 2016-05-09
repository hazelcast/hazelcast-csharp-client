
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.0.30319.33440
//  Copyright (c) Microsoft Corporation.  All rights reserved.



// Metadata version: v2.0.50727
.assembly extern System.Core
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 3:5:0:0
}
.assembly extern System
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 2:0:0:0
}
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 2:0:0:0
}
.assembly extern System.Web
{
  .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A )                         // .?_....:
  .ver 2:0:0:0
}
.assembly Thrift
{
  .custom instance void [mscorlib]System.Reflection.AssemblyTitleAttribute::.ctor(string) = ( 01 00 06 54 68 72 69 66 74 00 00 )                // ...Thrift..
  .custom instance void [mscorlib]System.Reflection.AssemblyDescriptionAttribute::.ctor(string) = ( 01 00 2C 43 23 20 62 69 6E 64 69 6E 67 73 20 66   // ..,C# bindings f
                                                                                                    6F 72 20 74 68 65 20 41 70 61 63 68 65 20 54 68   // or the Apache Th
                                                                                                    72 69 66 74 20 52 50 43 20 73 79 73 74 65 6D 00   // rift RPC system.
                                                                                                    00 ) 
  .custom instance void [mscorlib]System.Reflection.AssemblyConfigurationAttribute::.ctor(string) = ( 01 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Reflection.AssemblyCompanyAttribute::.ctor(string) = ( 01 00 1E 54 68 65 20 41 70 61 63 68 65 20 53 6F   // ...The Apache So
                                                                                                66 74 77 61 72 65 20 46 6F 75 6E 64 61 74 69 6F   // ftware Foundatio
                                                                                                6E 00 00 )                                        // n..
  .custom instance void [mscorlib]System.Reflection.AssemblyProductAttribute::.ctor(string) = ( 01 00 06 54 68 72 69 66 74 00 00 )                // ...Thrift..
  .custom instance void [mscorlib]System.Reflection.AssemblyCopyrightAttribute::.ctor(string) = ( 01 00 1E 54 68 65 20 41 70 61 63 68 65 20 53 6F   // ...The Apache So
                                                                                                  66 74 77 61 72 65 20 46 6F 75 6E 64 61 74 69 6F   // ftware Foundatio
                                                                                                  6E 00 00 )                                        // n..
  .custom instance void [mscorlib]System.Reflection.AssemblyTrademarkAttribute::.ctor(string) = ( 01 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.InteropServices.ComVisibleAttribute::.ctor(bool) = ( 01 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.InteropServices.GuidAttribute::.ctor(string) = ( 01 00 24 64 66 33 66 38 65 66 30 2D 65 30 61 33   // ..$df3f8ef0-e0a3
                                                                                                  2D 34 63 38 36 2D 61 36 35 62 2D 38 65 63 38 34   // -4c86-a65b-8ec84
                                                                                                  65 30 31 36 62 31 64 00 00 )                      // e016b1d..
  .custom instance void [mscorlib]System.Reflection.AssemblyFileVersionAttribute::.ctor(string) = ( 01 00 07 30 2E 39 2E 33 2E 30 00 00 )             // ...0.9.3.0..

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 02 00 00 00 00 00 ) 

  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .hash algorithm 0x00008004
  .ver 0:9:3:0
}
.module Thrift.dll
// MVID: {30E236EE-F36E-40A5-890E-CA4230B25402}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x0000025BD3140000


// =============== CLASS MEMBERS DECLARATION ===================

.class public auto ansi beforefieldinit Thrift.Collections.TCollections
       extends [mscorlib]System.Object
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  } // end of method TCollections::.ctor

  .method public hidebysig static bool  Equals(class [mscorlib]System.Collections.IEnumerable first,
                                               class [mscorlib]System.Collections.IEnumerable second) cil managed
  {
    // Code size       195 (0xc3)
    .maxstack  3
    .locals init (class [mscorlib]System.Collections.IEnumerator V_0,
             class [mscorlib]System.Collections.IEnumerator V_1,
             bool V_2,
             bool V_3,
             class [mscorlib]System.Collections.IEnumerable V_4,
             class [mscorlib]System.Collections.IEnumerable V_5)
    IL_0000:  ldarg.0
    IL_0001:  brtrue     IL_000e

    IL_0006:  ldarg.1
    IL_0007:  brtrue     IL_000e

    IL_000c:  ldc.i4.1
    IL_000d:  ret

    IL_000e:  ldarg.0
    IL_000f:  brfalse    IL_001a

    IL_0014:  ldarg.1
    IL_0015:  brtrue     IL_001c

    IL_001a:  ldc.i4.0
    IL_001b:  ret

    IL_001c:  ldarg.0
    IL_001d:  callvirt   instance class [mscorlib]System.Collections.IEnumerator [mscorlib]System.Collections.IEnumerable::GetEnumerator()
    IL_0022:  stloc.0
    IL_0023:  ldarg.1
    IL_0024:  callvirt   instance class [mscorlib]System.Collections.IEnumerator [mscorlib]System.Collections.IEnumerable::GetEnumerator()
    IL_0029:  stloc.1
    IL_002a:  ldloc.0
    IL_002b:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
    IL_0030:  stloc.2
    IL_0031:  ldloc.1
    IL_0032:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
    IL_0037:  stloc.3
    IL_0038:  br         IL_00b2

    IL_003d:  ldloc.0
    IL_003e:  callvirt   instance object [mscorlib]System.Collections.IEnumerator::get_Current()
    IL_0043:  isinst     [mscorlib]System.Collections.IEnumerable
    IL_0048:  stloc.s    V_4
    IL_004a:  ldloc.1
    IL_004b:  callvirt   instance object [mscorlib]System.Collections.IEnumerator::get_Current()
    IL_0050:  isinst     [mscorlib]System.Collections.IEnumerable
    IL_0055:  stloc.s    V_5
    IL_0057:  ldloc.s    V_4
    IL_0059:  brfalse    IL_007a

    IL_005e:  ldloc.s    V_5
    IL_0060:  brfalse    IL_007a

    IL_0065:  ldloc.s    V_4
    IL_0067:  ldloc.s    V_5
    IL_0069:  call       bool Thrift.Collections.TCollections::Equals(class [mscorlib]System.Collections.IEnumerable,
                                                                      class [mscorlib]System.Collections.IEnumerable)
    IL_006e:  brtrue     IL_0075

    IL_0073:  ldc.i4.0
    IL_0074:  ret

    IL_0075:  br         IL_00a4

    IL_007a:  ldloc.s    V_4
    IL_007c:  ldnull
    IL_007d:  ceq
    IL_007f:  ldloc.s    V_5
    IL_0081:  ldnull
    IL_0082:  ceq
    IL_0084:  xor
    IL_0085:  brfalse    IL_008c

    IL_008a:  ldc.i4.0
    IL_008b:  ret

    IL_008c:  ldloc.0
    IL_008d:  callvirt   instance object [mscorlib]System.Collections.IEnumerator::get_Current()
    IL_0092:  ldloc.1
    IL_0093:  callvirt   instance object [mscorlib]System.Collections.IEnumerator::get_Current()
    IL_0098:  call       bool [mscorlib]System.Object::Equals(object,
                                                              object)
    IL_009d:  brtrue     IL_00a4

    IL_00a2:  ldc.i4.0
    IL_00a3:  ret

    IL_00a4:  ldloc.0
    IL_00a5:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
    IL_00aa:  stloc.2
    IL_00ab:  ldloc.1
    IL_00ac:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
    IL_00b1:  stloc.3
    IL_00b2:  ldloc.2
    IL_00b3:  brfalse    IL_00be

    IL_00b8:  ldloc.3
    IL_00b9:  brtrue     IL_003d

    IL_00be:  ldloc.2
    IL_00bf:  ldloc.3
    IL_00c0:  ceq
    IL_00c2:  ret
  } // end of method TCollections::Equals

  .method public hidebysig static int32  GetHashCode(class [mscorlib]System.Collections.IEnumerable enumerable) cil managed
  {
    // Code size       112 (0x70)
    .maxstack  2
    .locals init (int32 V_0,
             object V_1,
             class [mscorlib]System.Collections.IEnumerator V_2,
             class [mscorlib]System.Collections.IEnumerable V_3,
             int32 V_4,
             class [mscorlib]System.IDisposable V_5)
    IL_0000:  ldarg.0
    IL_0001:  brtrue     IL_0008

    IL_0006:  ldc.i4.0
    IL_0007:  ret

    IL_0008:  ldc.i4.0
    IL_0009:  stloc.0
    IL_000a:  ldarg.0
    IL_000b:  callvirt   instance class [mscorlib]System.Collections.IEnumerator [mscorlib]System.Collections.IEnumerable::GetEnumerator()
    IL_0010:  stloc.2
    .try
    {
      IL_0011:  br         IL_0048

      IL_0016:  ldloc.2
      IL_0017:  callvirt   instance object [mscorlib]System.Collections.IEnumerator::get_Current()
      IL_001c:  stloc.1
      IL_001d:  ldloc.1
      IL_001e:  isinst     [mscorlib]System.Collections.IEnumerable
      IL_0023:  stloc.3
      IL_0024:  ldloc.3
      IL_0025:  brtrue     IL_0035

      IL_002a:  ldloc.1
      IL_002b:  callvirt   instance int32 [mscorlib]System.Object::GetHashCode()
      IL_0030:  br         IL_003b

      IL_0035:  ldloc.3
      IL_0036:  call       int32 Thrift.Collections.TCollections::GetHashCode(class [mscorlib]System.Collections.IEnumerable)
      IL_003b:  stloc.s    V_4
      IL_003d:  ldloc.0
      IL_003e:  ldc.i4     0x18d
      IL_0043:  mul
      IL_0044:  ldloc.s    V_4
      IL_0046:  xor
      IL_0047:  stloc.0
      IL_0048:  ldloc.2
      IL_0049:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
      IL_004e:  brtrue     IL_0016

      IL_0053:  leave      IL_006e

    }  // end .try
    finally
    {
      IL_0058:  ldloc.2
      IL_0059:  isinst     [mscorlib]System.IDisposable
      IL_005e:  dup
      IL_005f:  stloc.s    V_5
      IL_0061:  brfalse    IL_006d

      IL_0066:  ldloc.s    V_5
      IL_0068:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_006d:  endfinally
    }  // end handler
    IL_006e:  ldloc.0
    IL_006f:  ret
  } // end of method TCollections::GetHashCode

} // end of class Thrift.Collections.TCollections

.class public auto ansi serializable beforefieldinit Thrift.Collections.THashSet`1<T>
       extends [mscorlib]System.Object
       implements class [mscorlib]System.Collections.Generic.ICollection`1<!T>,
                  class [mscorlib]System.Collections.Generic.IEnumerable`1<!T>,
                  [mscorlib]System.Collections.IEnumerable
{
  .field private class [System.Core]System.Collections.Generic.HashSet`1<!T> set
  .method public hidebysig newslot specialname virtual final 
          instance int32  get_Count() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.Collections.Generic.HashSet`1<!0> class Thrift.Collections.THashSet`1<!T>::set
    IL_0006:  callvirt   instance int32 class [System.Core]System.Collections.Generic.HashSet`1<!T>::get_Count()
    IL_000b:  ret
  } // end of method THashSet`1::get_Count

  .method public hidebysig newslot specialname virtual final 
          instance bool  get_IsReadOnly() cil managed
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldc.i4.0
    IL_0001:  ret
  } // end of method THashSet`1::get_IsReadOnly

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  newobj     instance void class [System.Core]System.Collections.Generic.HashSet`1<!T>::.ctor()
    IL_0006:  stfld      class [System.Core]System.Collections.Generic.HashSet`1<!0> class Thrift.Collections.THashSet`1<!T>::set
    IL_000b:  ldarg.0
    IL_000c:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0011:  ret
  } // end of method THashSet`1::.ctor

  .method public hidebysig newslot virtual final 
          instance void  Add(!T item) cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.Collections.Generic.HashSet`1<!0> class Thrift.Collections.THashSet`1<!T>::set
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance bool class [System.Core]System.Collections.Generic.HashSet`1<!T>::Add(!0)
    IL_000c:  pop
    IL_000d:  ret
  } // end of method THashSet`1::Add

  .method public hidebysig newslot virtual final 
          instance void  Clear() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.Collections.Generic.HashSet`1<!0> class Thrift.Collections.THashSet`1<!T>::set
    IL_0006:  callvirt   instance void class [System.Core]System.Collections.Generic.HashSet`1<!T>::Clear()
    IL_000b:  ret
  } // end of method THashSet`1::Clear

  .method public hidebysig newslot virtual final 
          instance bool  Contains(!T item) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.Collections.Generic.HashSet`1<!0> class Thrift.Collections.THashSet`1<!T>::set
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance bool class [System.Core]System.Collections.Generic.HashSet`1<!T>::Contains(!0)
    IL_000c:  ret
  } // end of method THashSet`1::Contains

  .method public hidebysig newslot virtual final 
          instance void  CopyTo(!T[] 'array',
                                int32 arrayIndex) cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.Collections.Generic.HashSet`1<!0> class Thrift.Collections.THashSet`1<!T>::set
    IL_0006:  ldarg.1
    IL_0007:  ldarg.2
    IL_0008:  callvirt   instance void class [System.Core]System.Collections.Generic.HashSet`1<!T>::CopyTo(!0[],
                                                                                                           int32)
    IL_000d:  ret
  } // end of method THashSet`1::CopyTo

  .method public hidebysig newslot virtual final 
          instance class [mscorlib]System.Collections.IEnumerator 
          GetEnumerator() cil managed
  {
    // Code size       17 (0x11)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.Collections.Generic.HashSet`1<!0> class Thrift.Collections.THashSet`1<!T>::set
    IL_0006:  callvirt   instance valuetype [System.Core]System.Collections.Generic.HashSet`1/Enumerator<!0> class [System.Core]System.Collections.Generic.HashSet`1<!T>::GetEnumerator()
    IL_000b:  box        valuetype [System.Core]System.Collections.Generic.HashSet`1/Enumerator<!T>
    IL_0010:  ret
  } // end of method THashSet`1::GetEnumerator

  .method private hidebysig newslot virtual final 
          instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!T> 
          'System.Collections.Generic.IEnumerable<T>.GetEnumerator'() cil managed
  {
    .override  method instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<!T>::GetEnumerator()
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.Collections.Generic.HashSet`1<!0> class Thrift.Collections.THashSet`1<!T>::set
    IL_0006:  callvirt   instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<!T>::GetEnumerator()
    IL_000b:  ret
  } // end of method THashSet`1::'System.Collections.Generic.IEnumerable<T>.GetEnumerator'

  .method public hidebysig newslot virtual final 
          instance bool  Remove(!T item) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.Collections.Generic.HashSet`1<!0> class Thrift.Collections.THashSet`1<!T>::set
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance bool class [System.Core]System.Collections.Generic.HashSet`1<!T>::Remove(!0)
    IL_000c:  ret
  } // end of method THashSet`1::Remove

  .property instance int32 Count()
  {
    .get instance int32 Thrift.Collections.THashSet`1::get_Count()
  } // end of property THashSet`1::Count
  .property instance bool IsReadOnly()
  {
    .get instance bool Thrift.Collections.THashSet`1::get_IsReadOnly()
  } // end of property THashSet`1::IsReadOnly
} // end of class Thrift.Collections.THashSet`1

.class interface public abstract auto ansi Thrift.Protocol.TAbstractBase
{
  .method public hidebysig newslot abstract virtual 
          instance void  Write(class Thrift.Protocol.TProtocol tProtocol) cil managed
  {
  } // end of method TAbstractBase::Write

} // end of class Thrift.Protocol.TAbstractBase

.class interface public abstract auto ansi Thrift.Protocol.TBase
       implements Thrift.Protocol.TAbstractBase
{
  .method public hidebysig newslot abstract virtual 
          instance void  Read(class Thrift.Protocol.TProtocol tProtocol) cil managed
  {
  } // end of method TBase::Read

} // end of class Thrift.Protocol.TBase

.class private abstract auto ansi sealed beforefieldinit Thrift.Protocol.TBase64Utils
       extends [mscorlib]System.Object
{
  .field static assembly literal string ENCODE_TABLE = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwx"
  + "yz0123456789+/"
  .field private static int32[] DECODE_TABLE
  .method assembly hidebysig static void 
          encode(uint8[] src,
                 int32 srcOff,
                 int32 len,
                 uint8[] dst,
                 int32 dstOff) cil managed
  {
    // Code size       234 (0xea)
    .maxstack  7
    IL_0000:  ldarg.3
    IL_0001:  ldarg.s    dstOff
    IL_0003:  ldstr      "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwx"
    + "yz0123456789+/"
    IL_0008:  ldarg.0
    IL_0009:  ldarg.1
    IL_000a:  ldelem.u1
    IL_000b:  ldc.i4.2
    IL_000c:  shr
    IL_000d:  ldc.i4.s   63
    IL_000f:  and
    IL_0010:  callvirt   instance char [mscorlib]System.String::get_Chars(int32)
    IL_0015:  conv.u1
    IL_0016:  stelem.i1
    IL_0017:  ldarg.2
    IL_0018:  ldc.i4.3
    IL_0019:  bne.un     IL_0085

    IL_001e:  ldarg.3
    IL_001f:  ldarg.s    dstOff
    IL_0021:  ldc.i4.1
    IL_0022:  add
    IL_0023:  ldstr      "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwx"
    + "yz0123456789+/"
    IL_0028:  ldarg.0
    IL_0029:  ldarg.1
    IL_002a:  ldelem.u1
    IL_002b:  ldc.i4.4
    IL_002c:  shl
    IL_002d:  ldc.i4.s   48
    IL_002f:  and
    IL_0030:  ldarg.0
    IL_0031:  ldarg.1
    IL_0032:  ldc.i4.1
    IL_0033:  add
    IL_0034:  ldelem.u1
    IL_0035:  ldc.i4.4
    IL_0036:  shr
    IL_0037:  ldc.i4.s   15
    IL_0039:  and
    IL_003a:  or
    IL_003b:  callvirt   instance char [mscorlib]System.String::get_Chars(int32)
    IL_0040:  conv.u1
    IL_0041:  stelem.i1
    IL_0042:  ldarg.3
    IL_0043:  ldarg.s    dstOff
    IL_0045:  ldc.i4.2
    IL_0046:  add
    IL_0047:  ldstr      "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwx"
    + "yz0123456789+/"
    IL_004c:  ldarg.0
    IL_004d:  ldarg.1
    IL_004e:  ldc.i4.1
    IL_004f:  add
    IL_0050:  ldelem.u1
    IL_0051:  ldc.i4.2
    IL_0052:  shl
    IL_0053:  ldc.i4.s   60
    IL_0055:  and
    IL_0056:  ldarg.0
    IL_0057:  ldarg.1
    IL_0058:  ldc.i4.2
    IL_0059:  add
    IL_005a:  ldelem.u1
    IL_005b:  ldc.i4.6
    IL_005c:  shr
    IL_005d:  ldc.i4.3
    IL_005e:  and
    IL_005f:  or
    IL_0060:  callvirt   instance char [mscorlib]System.String::get_Chars(int32)
    IL_0065:  conv.u1
    IL_0066:  stelem.i1
    IL_0067:  ldarg.3
    IL_0068:  ldarg.s    dstOff
    IL_006a:  ldc.i4.3
    IL_006b:  add
    IL_006c:  ldstr      "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwx"
    + "yz0123456789+/"
    IL_0071:  ldarg.0
    IL_0072:  ldarg.1
    IL_0073:  ldc.i4.2
    IL_0074:  add
    IL_0075:  ldelem.u1
    IL_0076:  ldc.i4.s   63
    IL_0078:  and
    IL_0079:  callvirt   instance char [mscorlib]System.String::get_Chars(int32)
    IL_007e:  conv.u1
    IL_007f:  stelem.i1
    IL_0080:  br         IL_00e9

    IL_0085:  ldarg.2
    IL_0086:  ldc.i4.2
    IL_0087:  bne.un     IL_00d0

    IL_008c:  ldarg.3
    IL_008d:  ldarg.s    dstOff
    IL_008f:  ldc.i4.1
    IL_0090:  add
    IL_0091:  ldstr      "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwx"
    + "yz0123456789+/"
    IL_0096:  ldarg.0
    IL_0097:  ldarg.1
    IL_0098:  ldelem.u1
    IL_0099:  ldc.i4.4
    IL_009a:  shl
    IL_009b:  ldc.i4.s   48
    IL_009d:  and
    IL_009e:  ldarg.0
    IL_009f:  ldarg.1
    IL_00a0:  ldc.i4.1
    IL_00a1:  add
    IL_00a2:  ldelem.u1
    IL_00a3:  ldc.i4.4
    IL_00a4:  shr
    IL_00a5:  ldc.i4.s   15
    IL_00a7:  and
    IL_00a8:  or
    IL_00a9:  callvirt   instance char [mscorlib]System.String::get_Chars(int32)
    IL_00ae:  conv.u1
    IL_00af:  stelem.i1
    IL_00b0:  ldarg.3
    IL_00b1:  ldarg.s    dstOff
    IL_00b3:  ldc.i4.2
    IL_00b4:  add
    IL_00b5:  ldstr      "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwx"
    + "yz0123456789+/"
    IL_00ba:  ldarg.0
    IL_00bb:  ldarg.1
    IL_00bc:  ldc.i4.1
    IL_00bd:  add
    IL_00be:  ldelem.u1
    IL_00bf:  ldc.i4.2
    IL_00c0:  shl
    IL_00c1:  ldc.i4.s   60
    IL_00c3:  and
    IL_00c4:  callvirt   instance char [mscorlib]System.String::get_Chars(int32)
    IL_00c9:  conv.u1
    IL_00ca:  stelem.i1
    IL_00cb:  br         IL_00e9

    IL_00d0:  ldarg.3
    IL_00d1:  ldarg.s    dstOff
    IL_00d3:  ldc.i4.1
    IL_00d4:  add
    IL_00d5:  ldstr      "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwx"
    + "yz0123456789+/"
    IL_00da:  ldarg.0
    IL_00db:  ldarg.1
    IL_00dc:  ldelem.u1
    IL_00dd:  ldc.i4.4
    IL_00de:  shl
    IL_00df:  ldc.i4.s   48
    IL_00e1:  and
    IL_00e2:  callvirt   instance char [mscorlib]System.String::get_Chars(int32)
    IL_00e7:  conv.u1
    IL_00e8:  stelem.i1
    IL_00e9:  ret
  } // end of method TBase64Utils::encode

  .method assembly hidebysig static void 
          decode(uint8[] src,
                 int32 srcOff,
                 int32 len,
                 uint8[] dst,
                 int32 dstOff) cil managed
  {
    // Code size       159 (0x9f)
    .maxstack  7
    IL_0000:  ldarg.3
    IL_0001:  ldarg.s    dstOff
    IL_0003:  ldsfld     int32[] Thrift.Protocol.TBase64Utils::DECODE_TABLE
    IL_0008:  ldarg.0
    IL_0009:  ldarg.1
    IL_000a:  ldelem.u1
    IL_000b:  ldc.i4     0xff
    IL_0010:  and
    IL_0011:  ldelem.i4
    IL_0012:  ldc.i4.2
    IL_0013:  shl
    IL_0014:  ldsfld     int32[] Thrift.Protocol.TBase64Utils::DECODE_TABLE
    IL_0019:  ldarg.0
    IL_001a:  ldarg.1
    IL_001b:  ldc.i4.1
    IL_001c:  add
    IL_001d:  ldelem.u1
    IL_001e:  ldc.i4     0xff
    IL_0023:  and
    IL_0024:  ldelem.i4
    IL_0025:  ldc.i4.4
    IL_0026:  shr
    IL_0027:  or
    IL_0028:  conv.u1
    IL_0029:  stelem.i1
    IL_002a:  ldarg.2
    IL_002b:  ldc.i4.2
    IL_002c:  ble        IL_009e

    IL_0031:  ldarg.3
    IL_0032:  ldarg.s    dstOff
    IL_0034:  ldc.i4.1
    IL_0035:  add
    IL_0036:  ldsfld     int32[] Thrift.Protocol.TBase64Utils::DECODE_TABLE
    IL_003b:  ldarg.0
    IL_003c:  ldarg.1
    IL_003d:  ldc.i4.1
    IL_003e:  add
    IL_003f:  ldelem.u1
    IL_0040:  ldc.i4     0xff
    IL_0045:  and
    IL_0046:  ldelem.i4
    IL_0047:  ldc.i4.4
    IL_0048:  shl
    IL_0049:  ldc.i4     0xf0
    IL_004e:  and
    IL_004f:  ldsfld     int32[] Thrift.Protocol.TBase64Utils::DECODE_TABLE
    IL_0054:  ldarg.0
    IL_0055:  ldarg.1
    IL_0056:  ldc.i4.2
    IL_0057:  add
    IL_0058:  ldelem.u1
    IL_0059:  ldc.i4     0xff
    IL_005e:  and
    IL_005f:  ldelem.i4
    IL_0060:  ldc.i4.2
    IL_0061:  shr
    IL_0062:  or
    IL_0063:  conv.u1
    IL_0064:  stelem.i1
    IL_0065:  ldarg.2
    IL_0066:  ldc.i4.3
    IL_0067:  ble        IL_009e

    IL_006c:  ldarg.3
    IL_006d:  ldarg.s    dstOff
    IL_006f:  ldc.i4.2
    IL_0070:  add
    IL_0071:  ldsfld     int32[] Thrift.Protocol.TBase64Utils::DECODE_TABLE
    IL_0076:  ldarg.0
    IL_0077:  ldarg.1
    IL_0078:  ldc.i4.2
    IL_0079:  add
    IL_007a:  ldelem.u1
    IL_007b:  ldc.i4     0xff
    IL_0080:  and
    IL_0081:  ldelem.i4
    IL_0082:  ldc.i4.6
    IL_0083:  shl
    IL_0084:  ldc.i4     0xc0
    IL_0089:  and
    IL_008a:  ldsfld     int32[] Thrift.Protocol.TBase64Utils::DECODE_TABLE
    IL_008f:  ldarg.0
    IL_0090:  ldarg.1
    IL_0091:  ldc.i4.3
    IL_0092:  add
    IL_0093:  ldelem.u1
    IL_0094:  ldc.i4     0xff
    IL_0099:  and
    IL_009a:  ldelem.i4
    IL_009b:  or
    IL_009c:  conv.u1
    IL_009d:  stelem.i1
    IL_009e:  ret
  } // end of method TBase64Utils::decode

  .method private hidebysig specialname rtspecialname static 
          void  .cctor() cil managed
  {
    // Code size       27 (0x1b)
    .maxstack  8
    IL_0000:  ldc.i4     0x100
    IL_0005:  newarr     [mscorlib]System.Int32
    IL_000a:  dup
    IL_000b:  ldtoken    field valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=1024' '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'::'$field-0'
    IL_0010:  call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array,
                                                                                                        valuetype [mscorlib]System.RuntimeFieldHandle)
    IL_0015:  stsfld     int32[] Thrift.Protocol.TBase64Utils::DECODE_TABLE
    IL_001a:  ret
  } // end of method TBase64Utils::.cctor

} // end of class Thrift.Protocol.TBase64Utils

.class public auto ansi beforefieldinit Thrift.Protocol.TBinaryProtocol
       extends Thrift.Protocol.TProtocol
{
  .class auto ansi nested public beforefieldinit Factory
         extends [mscorlib]System.Object
         implements Thrift.Protocol.TProtocolFactory
  {
    .field family bool strictRead_
    .field family bool strictWrite_
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       9 (0x9)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldc.i4.0
      IL_0002:  ldc.i4.1
      IL_0003:  call       instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor(bool,
                                                                                        bool)
      IL_0008:  ret
    } // end of method Factory::.ctor

    .method public hidebysig specialname rtspecialname 
            instance void  .ctor(bool strictRead,
                                 bool strictWrite) cil managed
    {
      // Code size       28 (0x1c)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldc.i4.1
      IL_0002:  stfld      bool Thrift.Protocol.TBinaryProtocol/Factory::strictWrite_
      IL_0007:  ldarg.0
      IL_0008:  call       instance void [mscorlib]System.Object::.ctor()
      IL_000d:  ldarg.0
      IL_000e:  ldarg.1
      IL_000f:  stfld      bool Thrift.Protocol.TBinaryProtocol/Factory::strictRead_
      IL_0014:  ldarg.0
      IL_0015:  ldarg.2
      IL_0016:  stfld      bool Thrift.Protocol.TBinaryProtocol/Factory::strictWrite_
      IL_001b:  ret
    } // end of method Factory::.ctor

    .method public hidebysig newslot virtual final 
            instance class Thrift.Protocol.TProtocol 
            GetProtocol(class Thrift.Transport.TTransport trans) cil managed
    {
      // Code size       19 (0x13)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldarg.0
      IL_0002:  ldfld      bool Thrift.Protocol.TBinaryProtocol/Factory::strictRead_
      IL_0007:  ldarg.0
      IL_0008:  ldfld      bool Thrift.Protocol.TBinaryProtocol/Factory::strictWrite_
      IL_000d:  newobj     instance void Thrift.Protocol.TBinaryProtocol::.ctor(class Thrift.Transport.TTransport,
                                                                                bool,
                                                                                bool)
      IL_0012:  ret
    } // end of method Factory::GetProtocol

  } // end of class Factory

  .field static family literal uint32 VERSION_MASK = uint32(0xFFFF0000)
  .field static family literal uint32 VERSION_1 = uint32(0x80010000)
  .field family bool strictRead_
  .field family bool strictWrite_
  .field private uint8[] bout
  .field private uint8[] i16out
  .field private uint8[] i32out
  .field private uint8[] i64out
  .field private uint8[] bin
  .field private uint8[] i16in
  .field private uint8[] i32in
  .field private uint8[] i64in
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.Transport.TTransport trans) cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldc.i4.0
    IL_0003:  ldc.i4.1
    IL_0004:  call       instance void Thrift.Protocol.TBinaryProtocol::.ctor(class Thrift.Transport.TTransport,
                                                                              bool,
                                                                              bool)
    IL_0009:  ret
  } // end of method TBinaryProtocol::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.Transport.TTransport trans,
                               bool strictRead,
                               bool strictWrite) cil managed
  {
    // Code size       125 (0x7d)
    .maxstack  2
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.1
    IL_0002:  stfld      bool Thrift.Protocol.TBinaryProtocol::strictWrite_
    IL_0007:  ldarg.0
    IL_0008:  ldc.i4.1
    IL_0009:  newarr     [mscorlib]System.Byte
    IL_000e:  stfld      uint8[] Thrift.Protocol.TBinaryProtocol::bout
    IL_0013:  ldarg.0
    IL_0014:  ldc.i4.2
    IL_0015:  newarr     [mscorlib]System.Byte
    IL_001a:  stfld      uint8[] Thrift.Protocol.TBinaryProtocol::i16out
    IL_001f:  ldarg.0
    IL_0020:  ldc.i4.4
    IL_0021:  newarr     [mscorlib]System.Byte
    IL_0026:  stfld      uint8[] Thrift.Protocol.TBinaryProtocol::i32out
    IL_002b:  ldarg.0
    IL_002c:  ldc.i4.8
    IL_002d:  newarr     [mscorlib]System.Byte
    IL_0032:  stfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64out
    IL_0037:  ldarg.0
    IL_0038:  ldc.i4.1
    IL_0039:  newarr     [mscorlib]System.Byte
    IL_003e:  stfld      uint8[] Thrift.Protocol.TBinaryProtocol::bin
    IL_0043:  ldarg.0
    IL_0044:  ldc.i4.2
    IL_0045:  newarr     [mscorlib]System.Byte
    IL_004a:  stfld      uint8[] Thrift.Protocol.TBinaryProtocol::i16in
    IL_004f:  ldarg.0
    IL_0050:  ldc.i4.4
    IL_0051:  newarr     [mscorlib]System.Byte
    IL_0056:  stfld      uint8[] Thrift.Protocol.TBinaryProtocol::i32in
    IL_005b:  ldarg.0
    IL_005c:  ldc.i4.8
    IL_005d:  newarr     [mscorlib]System.Byte
    IL_0062:  stfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64in
    IL_0067:  ldarg.0
    IL_0068:  ldarg.1
    IL_0069:  call       instance void Thrift.Protocol.TProtocol::.ctor(class Thrift.Transport.TTransport)
    IL_006e:  ldarg.0
    IL_006f:  ldarg.2
    IL_0070:  stfld      bool Thrift.Protocol.TBinaryProtocol::strictRead_
    IL_0075:  ldarg.0
    IL_0076:  ldarg.3
    IL_0077:  stfld      bool Thrift.Protocol.TBinaryProtocol::strictWrite_
    IL_007c:  ret
  } // end of method TBinaryProtocol::.ctor

  .method public hidebysig virtual instance void 
          WriteMessageBegin(valuetype Thrift.Protocol.TMessage message) cil managed
  {
    // Code size       104 (0x68)
    .maxstack  2
    .locals init (uint32 V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      bool Thrift.Protocol.TBinaryProtocol::strictWrite_
    IL_0006:  brfalse    IL_003f

    IL_000b:  ldc.i4     0x80010000
    IL_0010:  ldarga.s   message
    IL_0012:  call       instance valuetype Thrift.Protocol.TMessageType Thrift.Protocol.TMessage::get_Type()
    IL_0017:  or
    IL_0018:  stloc.0
    IL_0019:  ldarg.0
    IL_001a:  ldloc.0
    IL_001b:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI32(int32)
    IL_0020:  ldarg.0
    IL_0021:  ldarga.s   message
    IL_0023:  call       instance string Thrift.Protocol.TMessage::get_Name()
    IL_0028:  callvirt   instance void Thrift.Protocol.TProtocol::WriteString(string)
    IL_002d:  ldarg.0
    IL_002e:  ldarga.s   message
    IL_0030:  call       instance int32 Thrift.Protocol.TMessage::get_SeqID()
    IL_0035:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI32(int32)
    IL_003a:  br         IL_0067

    IL_003f:  ldarg.0
    IL_0040:  ldarga.s   message
    IL_0042:  call       instance string Thrift.Protocol.TMessage::get_Name()
    IL_0047:  callvirt   instance void Thrift.Protocol.TProtocol::WriteString(string)
    IL_004c:  ldarg.0
    IL_004d:  ldarga.s   message
    IL_004f:  call       instance valuetype Thrift.Protocol.TMessageType Thrift.Protocol.TMessage::get_Type()
    IL_0054:  conv.i1
    IL_0055:  callvirt   instance void Thrift.Protocol.TProtocol::WriteByte(int8)
    IL_005a:  ldarg.0
    IL_005b:  ldarga.s   message
    IL_005d:  call       instance int32 Thrift.Protocol.TMessage::get_SeqID()
    IL_0062:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI32(int32)
    IL_0067:  ret
  } // end of method TBinaryProtocol::WriteMessageBegin

  .method public hidebysig virtual instance void 
          WriteMessageEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TBinaryProtocol::WriteMessageEnd

  .method public hidebysig virtual instance void 
          WriteStructBegin(valuetype Thrift.Protocol.TStruct struc) cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TBinaryProtocol::WriteStructBegin

  .method public hidebysig virtual instance void 
          WriteStructEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TBinaryProtocol::WriteStructEnd

  .method public hidebysig virtual instance void 
          WriteFieldBegin(valuetype Thrift.Protocol.TField 'field') cil managed
  {
    // Code size       28 (0x1c)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarga.s   'field'
    IL_0003:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
    IL_0008:  conv.i1
    IL_0009:  callvirt   instance void Thrift.Protocol.TProtocol::WriteByte(int8)
    IL_000e:  ldarg.0
    IL_000f:  ldarga.s   'field'
    IL_0011:  call       instance int16 Thrift.Protocol.TField::get_ID()
    IL_0016:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI16(int16)
    IL_001b:  ret
  } // end of method TBinaryProtocol::WriteFieldBegin

  .method public hidebysig virtual instance void 
          WriteFieldEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TBinaryProtocol::WriteFieldEnd

  .method public hidebysig virtual instance void 
          WriteFieldStop() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.0
    IL_0002:  callvirt   instance void Thrift.Protocol.TProtocol::WriteByte(int8)
    IL_0007:  ret
  } // end of method TBinaryProtocol::WriteFieldStop

  .method public hidebysig virtual instance void 
          WriteMapBegin(valuetype Thrift.Protocol.TMap map) cil managed
  {
    // Code size       42 (0x2a)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarga.s   map
    IL_0003:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::get_KeyType()
    IL_0008:  conv.i1
    IL_0009:  callvirt   instance void Thrift.Protocol.TProtocol::WriteByte(int8)
    IL_000e:  ldarg.0
    IL_000f:  ldarga.s   map
    IL_0011:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::get_ValueType()
    IL_0016:  conv.i1
    IL_0017:  callvirt   instance void Thrift.Protocol.TProtocol::WriteByte(int8)
    IL_001c:  ldarg.0
    IL_001d:  ldarga.s   map
    IL_001f:  call       instance int32 Thrift.Protocol.TMap::get_Count()
    IL_0024:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI32(int32)
    IL_0029:  ret
  } // end of method TBinaryProtocol::WriteMapBegin

  .method public hidebysig virtual instance void 
          WriteMapEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TBinaryProtocol::WriteMapEnd

  .method public hidebysig virtual instance void 
          WriteListBegin(valuetype Thrift.Protocol.TList list) cil managed
  {
    // Code size       28 (0x1c)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarga.s   list
    IL_0003:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TList::get_ElementType()
    IL_0008:  conv.i1
    IL_0009:  callvirt   instance void Thrift.Protocol.TProtocol::WriteByte(int8)
    IL_000e:  ldarg.0
    IL_000f:  ldarga.s   list
    IL_0011:  call       instance int32 Thrift.Protocol.TList::get_Count()
    IL_0016:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI32(int32)
    IL_001b:  ret
  } // end of method TBinaryProtocol::WriteListBegin

  .method public hidebysig virtual instance void 
          WriteListEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TBinaryProtocol::WriteListEnd

  .method public hidebysig virtual instance void 
          WriteSetBegin(valuetype Thrift.Protocol.TSet set) cil managed
  {
    // Code size       28 (0x1c)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarga.s   set
    IL_0003:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TSet::get_ElementType()
    IL_0008:  conv.i1
    IL_0009:  callvirt   instance void Thrift.Protocol.TProtocol::WriteByte(int8)
    IL_000e:  ldarg.0
    IL_000f:  ldarga.s   set
    IL_0011:  call       instance int32 Thrift.Protocol.TSet::get_Count()
    IL_0016:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI32(int32)
    IL_001b:  ret
  } // end of method TBinaryProtocol::WriteSetBegin

  .method public hidebysig virtual instance void 
          WriteSetEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TBinaryProtocol::WriteSetEnd

  .method public hidebysig virtual instance void 
          WriteBool(bool b) cil managed
  {
    // Code size       20 (0x14)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  brfalse    IL_000d

    IL_0007:  ldc.i4.1
    IL_0008:  br         IL_000e

    IL_000d:  ldc.i4.0
    IL_000e:  callvirt   instance void Thrift.Protocol.TProtocol::WriteByte(int8)
    IL_0013:  ret
  } // end of method TBinaryProtocol::WriteBool

  .method public hidebysig virtual instance void 
          WriteByte(int8 b) cil managed
  {
    // Code size       30 (0x1e)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::bout
    IL_0006:  ldc.i4.0
    IL_0007:  ldarg.1
    IL_0008:  conv.u1
    IL_0009:  stelem.i1
    IL_000a:  ldarg.0
    IL_000b:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0010:  ldarg.0
    IL_0011:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::bout
    IL_0016:  ldc.i4.0
    IL_0017:  ldc.i4.1
    IL_0018:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_001d:  ret
  } // end of method TBinaryProtocol::WriteByte

  .method public hidebysig virtual instance void 
          WriteI16(int16 s) cil managed
  {
    // Code size       54 (0x36)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i16out
    IL_0006:  ldc.i4.0
    IL_0007:  ldc.i4     0xff
    IL_000c:  ldarg.1
    IL_000d:  ldc.i4.8
    IL_000e:  shr
    IL_000f:  and
    IL_0010:  conv.u1
    IL_0011:  stelem.i1
    IL_0012:  ldarg.0
    IL_0013:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i16out
    IL_0018:  ldc.i4.1
    IL_0019:  ldc.i4     0xff
    IL_001e:  ldarg.1
    IL_001f:  and
    IL_0020:  conv.u1
    IL_0021:  stelem.i1
    IL_0022:  ldarg.0
    IL_0023:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0028:  ldarg.0
    IL_0029:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i16out
    IL_002e:  ldc.i4.0
    IL_002f:  ldc.i4.2
    IL_0030:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_0035:  ret
  } // end of method TBinaryProtocol::WriteI16

  .method public hidebysig virtual instance void 
          WriteI32(int32 i32) cil managed
  {
    // Code size       92 (0x5c)
    .maxstack  5
    IL_0000:  ldarg.0
    IL_0001:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i32out
    IL_0006:  ldc.i4.0
    IL_0007:  ldc.i4     0xff
    IL_000c:  ldarg.1
    IL_000d:  ldc.i4.s   24
    IL_000f:  shr
    IL_0010:  and
    IL_0011:  conv.u1
    IL_0012:  stelem.i1
    IL_0013:  ldarg.0
    IL_0014:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i32out
    IL_0019:  ldc.i4.1
    IL_001a:  ldc.i4     0xff
    IL_001f:  ldarg.1
    IL_0020:  ldc.i4.s   16
    IL_0022:  shr
    IL_0023:  and
    IL_0024:  conv.u1
    IL_0025:  stelem.i1
    IL_0026:  ldarg.0
    IL_0027:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i32out
    IL_002c:  ldc.i4.2
    IL_002d:  ldc.i4     0xff
    IL_0032:  ldarg.1
    IL_0033:  ldc.i4.8
    IL_0034:  shr
    IL_0035:  and
    IL_0036:  conv.u1
    IL_0037:  stelem.i1
    IL_0038:  ldarg.0
    IL_0039:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i32out
    IL_003e:  ldc.i4.3
    IL_003f:  ldc.i4     0xff
    IL_0044:  ldarg.1
    IL_0045:  and
    IL_0046:  conv.u1
    IL_0047:  stelem.i1
    IL_0048:  ldarg.0
    IL_0049:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_004e:  ldarg.0
    IL_004f:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i32out
    IL_0054:  ldc.i4.0
    IL_0055:  ldc.i4.4
    IL_0056:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_005b:  ret
  } // end of method TBinaryProtocol::WriteI32

  .method public hidebysig virtual instance void 
          WriteI64(int64 i64) cil managed
  {
    // Code size       176 (0xb0)
    .maxstack  5
    IL_0000:  ldarg.0
    IL_0001:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64out
    IL_0006:  ldc.i4.0
    IL_0007:  ldc.i4     0xff
    IL_000c:  conv.i8
    IL_000d:  ldarg.1
    IL_000e:  ldc.i4.s   56
    IL_0010:  shr
    IL_0011:  and
    IL_0012:  conv.u1
    IL_0013:  stelem.i1
    IL_0014:  ldarg.0
    IL_0015:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64out
    IL_001a:  ldc.i4.1
    IL_001b:  ldc.i4     0xff
    IL_0020:  conv.i8
    IL_0021:  ldarg.1
    IL_0022:  ldc.i4.s   48
    IL_0024:  shr
    IL_0025:  and
    IL_0026:  conv.u1
    IL_0027:  stelem.i1
    IL_0028:  ldarg.0
    IL_0029:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64out
    IL_002e:  ldc.i4.2
    IL_002f:  ldc.i4     0xff
    IL_0034:  conv.i8
    IL_0035:  ldarg.1
    IL_0036:  ldc.i4.s   40
    IL_0038:  shr
    IL_0039:  and
    IL_003a:  conv.u1
    IL_003b:  stelem.i1
    IL_003c:  ldarg.0
    IL_003d:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64out
    IL_0042:  ldc.i4.3
    IL_0043:  ldc.i4     0xff
    IL_0048:  conv.i8
    IL_0049:  ldarg.1
    IL_004a:  ldc.i4.s   32
    IL_004c:  shr
    IL_004d:  and
    IL_004e:  conv.u1
    IL_004f:  stelem.i1
    IL_0050:  ldarg.0
    IL_0051:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64out
    IL_0056:  ldc.i4.4
    IL_0057:  ldc.i4     0xff
    IL_005c:  conv.i8
    IL_005d:  ldarg.1
    IL_005e:  ldc.i4.s   24
    IL_0060:  shr
    IL_0061:  and
    IL_0062:  conv.u1
    IL_0063:  stelem.i1
    IL_0064:  ldarg.0
    IL_0065:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64out
    IL_006a:  ldc.i4.5
    IL_006b:  ldc.i4     0xff
    IL_0070:  conv.i8
    IL_0071:  ldarg.1
    IL_0072:  ldc.i4.s   16
    IL_0074:  shr
    IL_0075:  and
    IL_0076:  conv.u1
    IL_0077:  stelem.i1
    IL_0078:  ldarg.0
    IL_0079:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64out
    IL_007e:  ldc.i4.6
    IL_007f:  ldc.i4     0xff
    IL_0084:  conv.i8
    IL_0085:  ldarg.1
    IL_0086:  ldc.i4.8
    IL_0087:  shr
    IL_0088:  and
    IL_0089:  conv.u1
    IL_008a:  stelem.i1
    IL_008b:  ldarg.0
    IL_008c:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64out
    IL_0091:  ldc.i4.7
    IL_0092:  ldc.i4     0xff
    IL_0097:  conv.i8
    IL_0098:  ldarg.1
    IL_0099:  and
    IL_009a:  conv.u1
    IL_009b:  stelem.i1
    IL_009c:  ldarg.0
    IL_009d:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_00a2:  ldarg.0
    IL_00a3:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64out
    IL_00a8:  ldc.i4.0
    IL_00a9:  ldc.i4.8
    IL_00aa:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_00af:  ret
  } // end of method TBinaryProtocol::WriteI64

  .method public hidebysig virtual instance void 
          WriteDouble(float64 d) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  call       int64 [mscorlib]System.BitConverter::DoubleToInt64Bits(float64)
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI64(int64)
    IL_000c:  ret
  } // end of method TBinaryProtocol::WriteDouble

  .method public hidebysig virtual instance void 
          WriteBinary(uint8[] b) cil managed
  {
    // Code size       26 (0x1a)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldlen
    IL_0003:  conv.i4
    IL_0004:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI32(int32)
    IL_0009:  ldarg.0
    IL_000a:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_000f:  ldarg.1
    IL_0010:  ldc.i4.0
    IL_0011:  ldarg.1
    IL_0012:  ldlen
    IL_0013:  conv.i4
    IL_0014:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_0019:  ret
  } // end of method TBinaryProtocol::WriteBinary

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TMessage 
          ReadMessageBegin() cil managed
  {
    // Code size       175 (0xaf)
    .maxstack  3
    .locals init (valuetype Thrift.Protocol.TMessage V_0,
             int32 V_1,
             uint32 V_2)
    IL_0000:  ldloca.s   V_0
    IL_0002:  initobj    Thrift.Protocol.TMessage
    IL_0008:  ldarg.0
    IL_0009:  callvirt   instance int32 Thrift.Protocol.TProtocol::ReadI32()
    IL_000e:  stloc.1
    IL_000f:  ldloc.1
    IL_0010:  ldc.i4.0
    IL_0011:  bge        IL_006d

    IL_0016:  ldloc.1
    IL_0017:  ldc.i4     0xffff0000
    IL_001c:  and
    IL_001d:  stloc.2
    IL_001e:  ldloc.2
    IL_001f:  ldc.i4     0x80010000
    IL_0024:  beq        IL_0040

    IL_0029:  ldc.i4.4
    IL_002a:  ldstr      "Bad version in ReadMessageBegin: "
    IL_002f:  ldloc.2
    IL_0030:  box        [mscorlib]System.UInt32
    IL_0035:  call       string [mscorlib]System.String::Concat(object,
                                                                object)
    IL_003a:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(int32,
                                                                                 string)
    IL_003f:  throw

    IL_0040:  ldloca.s   V_0
    IL_0042:  ldloc.1
    IL_0043:  ldc.i4     0xff
    IL_0048:  and
    IL_0049:  call       instance void Thrift.Protocol.TMessage::set_Type(valuetype Thrift.Protocol.TMessageType)
    IL_004e:  ldloca.s   V_0
    IL_0050:  ldarg.0
    IL_0051:  callvirt   instance string Thrift.Protocol.TProtocol::ReadString()
    IL_0056:  call       instance void Thrift.Protocol.TMessage::set_Name(string)
    IL_005b:  ldloca.s   V_0
    IL_005d:  ldarg.0
    IL_005e:  callvirt   instance int32 Thrift.Protocol.TProtocol::ReadI32()
    IL_0063:  call       instance void Thrift.Protocol.TMessage::set_SeqID(int32)
    IL_0068:  br         IL_00ad

    IL_006d:  ldarg.0
    IL_006e:  ldfld      bool Thrift.Protocol.TBinaryProtocol::strictRead_
    IL_0073:  brfalse    IL_0084

    IL_0078:  ldc.i4.4
    IL_0079:  ldstr      "Missing version in readMessageBegin, old client\?"
    IL_007e:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(int32,
                                                                                 string)
    IL_0083:  throw

    IL_0084:  ldloca.s   V_0
    IL_0086:  ldarg.0
    IL_0087:  ldloc.1
    IL_0088:  call       instance string Thrift.Protocol.TBinaryProtocol::ReadStringBody(int32)
    IL_008d:  call       instance void Thrift.Protocol.TMessage::set_Name(string)
    IL_0092:  ldloca.s   V_0
    IL_0094:  ldarg.0
    IL_0095:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_009a:  conv.i4
    IL_009b:  call       instance void Thrift.Protocol.TMessage::set_Type(valuetype Thrift.Protocol.TMessageType)
    IL_00a0:  ldloca.s   V_0
    IL_00a2:  ldarg.0
    IL_00a3:  callvirt   instance int32 Thrift.Protocol.TProtocol::ReadI32()
    IL_00a8:  call       instance void Thrift.Protocol.TMessage::set_SeqID(int32)
    IL_00ad:  ldloc.0
    IL_00ae:  ret
  } // end of method TBinaryProtocol::ReadMessageBegin

  .method public hidebysig virtual instance void 
          ReadMessageEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TBinaryProtocol::ReadMessageEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TStruct 
          ReadStructBegin() cil managed
  {
    // Code size       10 (0xa)
    .maxstack  1
    .locals init (valuetype Thrift.Protocol.TStruct V_0)
    IL_0000:  ldloca.s   V_0
    IL_0002:  initobj    Thrift.Protocol.TStruct
    IL_0008:  ldloc.0
    IL_0009:  ret
  } // end of method TBinaryProtocol::ReadStructBegin

  .method public hidebysig virtual instance void 
          ReadStructEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TBinaryProtocol::ReadStructEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TField 
          ReadFieldBegin() cil managed
  {
    // Code size       49 (0x31)
    .maxstack  2
    .locals init (valuetype Thrift.Protocol.TField V_0)
    IL_0000:  ldloca.s   V_0
    IL_0002:  initobj    Thrift.Protocol.TField
    IL_0008:  ldloca.s   V_0
    IL_000a:  ldarg.0
    IL_000b:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_0010:  conv.u1
    IL_0011:  call       instance void Thrift.Protocol.TField::set_Type(valuetype Thrift.Protocol.TType)
    IL_0016:  ldloca.s   V_0
    IL_0018:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
    IL_001d:  brfalse    IL_002f

    IL_0022:  ldloca.s   V_0
    IL_0024:  ldarg.0
    IL_0025:  callvirt   instance int16 Thrift.Protocol.TProtocol::ReadI16()
    IL_002a:  call       instance void Thrift.Protocol.TField::set_ID(int16)
    IL_002f:  ldloc.0
    IL_0030:  ret
  } // end of method TBinaryProtocol::ReadFieldBegin

  .method public hidebysig virtual instance void 
          ReadFieldEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TBinaryProtocol::ReadFieldEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TMap 
          ReadMapBegin() cil managed
  {
    // Code size       51 (0x33)
    .maxstack  2
    .locals init (valuetype Thrift.Protocol.TMap V_0)
    IL_0000:  ldloca.s   V_0
    IL_0002:  initobj    Thrift.Protocol.TMap
    IL_0008:  ldloca.s   V_0
    IL_000a:  ldarg.0
    IL_000b:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_0010:  conv.u1
    IL_0011:  call       instance void Thrift.Protocol.TMap::set_KeyType(valuetype Thrift.Protocol.TType)
    IL_0016:  ldloca.s   V_0
    IL_0018:  ldarg.0
    IL_0019:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_001e:  conv.u1
    IL_001f:  call       instance void Thrift.Protocol.TMap::set_ValueType(valuetype Thrift.Protocol.TType)
    IL_0024:  ldloca.s   V_0
    IL_0026:  ldarg.0
    IL_0027:  callvirt   instance int32 Thrift.Protocol.TProtocol::ReadI32()
    IL_002c:  call       instance void Thrift.Protocol.TMap::set_Count(int32)
    IL_0031:  ldloc.0
    IL_0032:  ret
  } // end of method TBinaryProtocol::ReadMapBegin

  .method public hidebysig virtual instance void 
          ReadMapEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TBinaryProtocol::ReadMapEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TList 
          ReadListBegin() cil managed
  {
    // Code size       37 (0x25)
    .maxstack  2
    .locals init (valuetype Thrift.Protocol.TList V_0)
    IL_0000:  ldloca.s   V_0
    IL_0002:  initobj    Thrift.Protocol.TList
    IL_0008:  ldloca.s   V_0
    IL_000a:  ldarg.0
    IL_000b:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_0010:  conv.u1
    IL_0011:  call       instance void Thrift.Protocol.TList::set_ElementType(valuetype Thrift.Protocol.TType)
    IL_0016:  ldloca.s   V_0
    IL_0018:  ldarg.0
    IL_0019:  callvirt   instance int32 Thrift.Protocol.TProtocol::ReadI32()
    IL_001e:  call       instance void Thrift.Protocol.TList::set_Count(int32)
    IL_0023:  ldloc.0
    IL_0024:  ret
  } // end of method TBinaryProtocol::ReadListBegin

  .method public hidebysig virtual instance void 
          ReadListEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TBinaryProtocol::ReadListEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TSet 
          ReadSetBegin() cil managed
  {
    // Code size       37 (0x25)
    .maxstack  2
    .locals init (valuetype Thrift.Protocol.TSet V_0)
    IL_0000:  ldloca.s   V_0
    IL_0002:  initobj    Thrift.Protocol.TSet
    IL_0008:  ldloca.s   V_0
    IL_000a:  ldarg.0
    IL_000b:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_0010:  conv.u1
    IL_0011:  call       instance void Thrift.Protocol.TSet::set_ElementType(valuetype Thrift.Protocol.TType)
    IL_0016:  ldloca.s   V_0
    IL_0018:  ldarg.0
    IL_0019:  callvirt   instance int32 Thrift.Protocol.TProtocol::ReadI32()
    IL_001e:  call       instance void Thrift.Protocol.TSet::set_Count(int32)
    IL_0023:  ldloc.0
    IL_0024:  ret
  } // end of method TBinaryProtocol::ReadSetBegin

  .method public hidebysig virtual instance void 
          ReadSetEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TBinaryProtocol::ReadSetEnd

  .method public hidebysig virtual instance bool 
          ReadBool() cil managed
  {
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_0006:  conv.i4
    IL_0007:  ldc.i4.1
    IL_0008:  ceq
    IL_000a:  ret
  } // end of method TBinaryProtocol::ReadBool

  .method public hidebysig virtual instance int8 
          ReadByte() cil managed
  {
    // Code size       25 (0x19)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::bin
    IL_0007:  ldc.i4.0
    IL_0008:  ldc.i4.1
    IL_0009:  call       instance int32 Thrift.Protocol.TBinaryProtocol::ReadAll(uint8[],
                                                                                 int32,
                                                                                 int32)
    IL_000e:  pop
    IL_000f:  ldarg.0
    IL_0010:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::bin
    IL_0015:  ldc.i4.0
    IL_0016:  ldelem.u1
    IL_0017:  conv.i1
    IL_0018:  ret
  } // end of method TBinaryProtocol::ReadByte

  .method public hidebysig virtual instance int16 
          ReadI16() cil managed
  {
    // Code size       48 (0x30)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i16in
    IL_0007:  ldc.i4.0
    IL_0008:  ldc.i4.2
    IL_0009:  call       instance int32 Thrift.Protocol.TBinaryProtocol::ReadAll(uint8[],
                                                                                 int32,
                                                                                 int32)
    IL_000e:  pop
    IL_000f:  ldarg.0
    IL_0010:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i16in
    IL_0015:  ldc.i4.0
    IL_0016:  ldelem.u1
    IL_0017:  ldc.i4     0xff
    IL_001c:  and
    IL_001d:  ldc.i4.8
    IL_001e:  shl
    IL_001f:  ldarg.0
    IL_0020:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i16in
    IL_0025:  ldc.i4.1
    IL_0026:  ldelem.u1
    IL_0027:  ldc.i4     0xff
    IL_002c:  and
    IL_002d:  or
    IL_002e:  conv.i2
    IL_002f:  ret
  } // end of method TBinaryProtocol::ReadI16

  .method public hidebysig virtual instance int32 
          ReadI32() cil managed
  {
    // Code size       83 (0x53)
    .maxstack  4
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i32in
    IL_0007:  ldc.i4.0
    IL_0008:  ldc.i4.4
    IL_0009:  call       instance int32 Thrift.Protocol.TBinaryProtocol::ReadAll(uint8[],
                                                                                 int32,
                                                                                 int32)
    IL_000e:  pop
    IL_000f:  ldarg.0
    IL_0010:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i32in
    IL_0015:  ldc.i4.0
    IL_0016:  ldelem.u1
    IL_0017:  ldc.i4     0xff
    IL_001c:  and
    IL_001d:  ldc.i4.s   24
    IL_001f:  shl
    IL_0020:  ldarg.0
    IL_0021:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i32in
    IL_0026:  ldc.i4.1
    IL_0027:  ldelem.u1
    IL_0028:  ldc.i4     0xff
    IL_002d:  and
    IL_002e:  ldc.i4.s   16
    IL_0030:  shl
    IL_0031:  or
    IL_0032:  ldarg.0
    IL_0033:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i32in
    IL_0038:  ldc.i4.2
    IL_0039:  ldelem.u1
    IL_003a:  ldc.i4     0xff
    IL_003f:  and
    IL_0040:  ldc.i4.8
    IL_0041:  shl
    IL_0042:  or
    IL_0043:  ldarg.0
    IL_0044:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i32in
    IL_0049:  ldc.i4.3
    IL_004a:  ldelem.u1
    IL_004b:  ldc.i4     0xff
    IL_0050:  and
    IL_0051:  or
    IL_0052:  ret
  } // end of method TBinaryProtocol::ReadI32

  .method public hidebysig virtual instance int64 
          ReadI64() cil managed
  {
    // Code size       163 (0xa3)
    .maxstack  4
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64in
    IL_0007:  ldc.i4.0
    IL_0008:  ldc.i4.8
    IL_0009:  call       instance int32 Thrift.Protocol.TBinaryProtocol::ReadAll(uint8[],
                                                                                 int32,
                                                                                 int32)
    IL_000e:  pop
    IL_000f:  ldarg.0
    IL_0010:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64in
    IL_0015:  ldc.i4.0
    IL_0016:  ldelem.u1
    IL_0017:  ldc.i4     0xff
    IL_001c:  and
    IL_001d:  conv.i8
    IL_001e:  ldc.i4.s   56
    IL_0020:  shl
    IL_0021:  ldarg.0
    IL_0022:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64in
    IL_0027:  ldc.i4.1
    IL_0028:  ldelem.u1
    IL_0029:  ldc.i4     0xff
    IL_002e:  and
    IL_002f:  conv.i8
    IL_0030:  ldc.i4.s   48
    IL_0032:  shl
    IL_0033:  or
    IL_0034:  ldarg.0
    IL_0035:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64in
    IL_003a:  ldc.i4.2
    IL_003b:  ldelem.u1
    IL_003c:  ldc.i4     0xff
    IL_0041:  and
    IL_0042:  conv.i8
    IL_0043:  ldc.i4.s   40
    IL_0045:  shl
    IL_0046:  or
    IL_0047:  ldarg.0
    IL_0048:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64in
    IL_004d:  ldc.i4.3
    IL_004e:  ldelem.u1
    IL_004f:  ldc.i4     0xff
    IL_0054:  and
    IL_0055:  conv.i8
    IL_0056:  ldc.i4.s   32
    IL_0058:  shl
    IL_0059:  or
    IL_005a:  ldarg.0
    IL_005b:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64in
    IL_0060:  ldc.i4.4
    IL_0061:  ldelem.u1
    IL_0062:  ldc.i4     0xff
    IL_0067:  and
    IL_0068:  conv.i8
    IL_0069:  ldc.i4.s   24
    IL_006b:  shl
    IL_006c:  or
    IL_006d:  ldarg.0
    IL_006e:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64in
    IL_0073:  ldc.i4.5
    IL_0074:  ldelem.u1
    IL_0075:  ldc.i4     0xff
    IL_007a:  and
    IL_007b:  conv.i8
    IL_007c:  ldc.i4.s   16
    IL_007e:  shl
    IL_007f:  or
    IL_0080:  ldarg.0
    IL_0081:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64in
    IL_0086:  ldc.i4.6
    IL_0087:  ldelem.u1
    IL_0088:  ldc.i4     0xff
    IL_008d:  and
    IL_008e:  conv.i8
    IL_008f:  ldc.i4.8
    IL_0090:  shl
    IL_0091:  or
    IL_0092:  ldarg.0
    IL_0093:  ldfld      uint8[] Thrift.Protocol.TBinaryProtocol::i64in
    IL_0098:  ldc.i4.7
    IL_0099:  ldelem.u1
    IL_009a:  ldc.i4     0xff
    IL_009f:  and
    IL_00a0:  conv.i8
    IL_00a1:  or
    IL_00a2:  ret
  } // end of method TBinaryProtocol::ReadI64

  .method public hidebysig virtual instance float64 
          ReadDouble() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance int64 Thrift.Protocol.TProtocol::ReadI64()
    IL_0006:  call       float64 [mscorlib]System.BitConverter::Int64BitsToDouble(int64)
    IL_000b:  ret
  } // end of method TBinaryProtocol::ReadDouble

  .method public hidebysig virtual instance uint8[] 
          ReadBinary() cil managed
  {
    // Code size       31 (0x1f)
    .maxstack  4
    .locals init (int32 V_0,
             uint8[] V_1)
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance int32 Thrift.Protocol.TProtocol::ReadI32()
    IL_0006:  stloc.0
    IL_0007:  ldloc.0
    IL_0008:  newarr     [mscorlib]System.Byte
    IL_000d:  stloc.1
    IL_000e:  ldarg.0
    IL_000f:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0014:  ldloc.1
    IL_0015:  ldc.i4.0
    IL_0016:  ldloc.0
    IL_0017:  callvirt   instance int32 Thrift.Transport.TTransport::ReadAll(uint8[],
                                                                             int32,
                                                                             int32)
    IL_001c:  pop
    IL_001d:  ldloc.1
    IL_001e:  ret
  } // end of method TBinaryProtocol::ReadBinary

  .method private hidebysig instance string 
          ReadStringBody(int32 size) cil managed
  {
    // Code size       38 (0x26)
    .maxstack  4
    .locals init (uint8[] V_0)
    IL_0000:  ldarg.1
    IL_0001:  newarr     [mscorlib]System.Byte
    IL_0006:  stloc.0
    IL_0007:  ldarg.0
    IL_0008:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_000d:  ldloc.0
    IL_000e:  ldc.i4.0
    IL_000f:  ldarg.1
    IL_0010:  callvirt   instance int32 Thrift.Transport.TTransport::ReadAll(uint8[],
                                                                             int32,
                                                                             int32)
    IL_0015:  pop
    IL_0016:  call       class [mscorlib]System.Text.Encoding [mscorlib]System.Text.Encoding::get_UTF8()
    IL_001b:  ldloc.0
    IL_001c:  ldc.i4.0
    IL_001d:  ldloc.0
    IL_001e:  ldlen
    IL_001f:  conv.i4
    IL_0020:  callvirt   instance string [mscorlib]System.Text.Encoding::GetString(uint8[],
                                                                                   int32,
                                                                                   int32)
    IL_0025:  ret
  } // end of method TBinaryProtocol::ReadStringBody

  .method private hidebysig instance int32 
          ReadAll(uint8[] buf,
                  int32 'off',
                  int32 len) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0006:  ldarg.1
    IL_0007:  ldarg.2
    IL_0008:  ldarg.3
    IL_0009:  callvirt   instance int32 Thrift.Transport.TTransport::ReadAll(uint8[],
                                                                             int32,
                                                                             int32)
    IL_000e:  ret
  } // end of method TBinaryProtocol::ReadAll

} // end of class Thrift.Protocol.TBinaryProtocol

.class public auto ansi beforefieldinit Thrift.Protocol.TCompactProtocol
       extends Thrift.Protocol.TProtocol
{
  .class abstract auto ansi sealed nested private beforefieldinit Types
         extends [mscorlib]System.Object
  {
    .field public static literal uint8 STOP = uint8(0x00)
    .field public static literal uint8 BOOLEAN_TRUE = uint8(0x01)
    .field public static literal uint8 BOOLEAN_FALSE = uint8(0x02)
    .field public static literal uint8 BYTE = uint8(0x03)
    .field public static literal uint8 I16 = uint8(0x04)
    .field public static literal uint8 I32 = uint8(0x05)
    .field public static literal uint8 I64 = uint8(0x06)
    .field public static literal uint8 DOUBLE = uint8(0x07)
    .field public static literal uint8 BINARY = uint8(0x08)
    .field public static literal uint8 LIST = uint8(0x09)
    .field public static literal uint8 SET = uint8(0x0A)
    .field public static literal uint8 MAP = uint8(0x0B)
    .field public static literal uint8 STRUCT = uint8(0x0C)
  } // end of class Types

  .class auto ansi nested public beforefieldinit Factory
         extends [mscorlib]System.Object
         implements Thrift.Protocol.TProtocolFactory
  {
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ret
    } // end of method Factory::.ctor

    .method public hidebysig newslot virtual final 
            instance class Thrift.Protocol.TProtocol 
            GetProtocol(class Thrift.Transport.TTransport trans) cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  newobj     instance void Thrift.Protocol.TCompactProtocol::.ctor(class Thrift.Transport.TTransport)
      IL_0006:  ret
    } // end of method Factory::GetProtocol

  } // end of class Factory

  .field private static valuetype Thrift.Protocol.TStruct ANONYMOUS_STRUCT
  .field private static valuetype Thrift.Protocol.TField TSTOP
  .field private static uint8[] ttypeToCompactType
  .field private static literal uint8 PROTOCOL_ID = uint8(0x82)
  .field private static literal uint8 VERSION = uint8(0x01)
  .field private static literal uint8 VERSION_MASK = uint8(0x1F)
  .field private static literal uint8 TYPE_MASK = uint8(0xE0)
  .field private static literal uint8 TYPE_BITS = uint8(0x07)
  .field private static literal int32 TYPE_SHIFT_AMOUNT = int32(0x00000005)
  .field private class [System]System.Collections.Generic.Stack`1<int16> lastField_
  .field private int16 lastFieldId_
  .field private valuetype [mscorlib]System.Nullable`1<valuetype Thrift.Protocol.TField> booleanField_
  .field private valuetype [mscorlib]System.Nullable`1<bool> boolValue_
  .field private uint8[] byteDirectBuffer
  .field private uint8[] i32buf
  .field private uint8[] varint64out
  .field private uint8[] byteRawBuf
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.Transport.TTransport trans) cil managed
  {
    // Code size       176 (0xb0)
    .maxstack  3
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.s   15
    IL_0003:  newobj     instance void class [System]System.Collections.Generic.Stack`1<int16>::.ctor(int32)
    IL_0008:  stfld      class [System]System.Collections.Generic.Stack`1<int16> Thrift.Protocol.TCompactProtocol::lastField_
    IL_000d:  ldarg.0
    IL_000e:  ldc.i4.1
    IL_000f:  newarr     [mscorlib]System.Byte
    IL_0014:  stfld      uint8[] Thrift.Protocol.TCompactProtocol::byteDirectBuffer
    IL_0019:  ldarg.0
    IL_001a:  ldc.i4.5
    IL_001b:  newarr     [mscorlib]System.Byte
    IL_0020:  stfld      uint8[] Thrift.Protocol.TCompactProtocol::i32buf
    IL_0025:  ldarg.0
    IL_0026:  ldc.i4.s   10
    IL_0028:  newarr     [mscorlib]System.Byte
    IL_002d:  stfld      uint8[] Thrift.Protocol.TCompactProtocol::varint64out
    IL_0032:  ldarg.0
    IL_0033:  ldc.i4.1
    IL_0034:  newarr     [mscorlib]System.Byte
    IL_0039:  stfld      uint8[] Thrift.Protocol.TCompactProtocol::byteRawBuf
    IL_003e:  ldarg.0
    IL_003f:  ldarg.1
    IL_0040:  call       instance void Thrift.Protocol.TProtocol::.ctor(class Thrift.Transport.TTransport)
    IL_0045:  ldsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_004a:  ldc.i4.0
    IL_004b:  ldc.i4.0
    IL_004c:  stelem.i1
    IL_004d:  ldsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_0052:  ldc.i4.2
    IL_0053:  ldc.i4.1
    IL_0054:  stelem.i1
    IL_0055:  ldsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_005a:  ldc.i4.3
    IL_005b:  ldc.i4.3
    IL_005c:  stelem.i1
    IL_005d:  ldsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_0062:  ldc.i4.6
    IL_0063:  ldc.i4.4
    IL_0064:  stelem.i1
    IL_0065:  ldsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_006a:  ldc.i4.8
    IL_006b:  ldc.i4.5
    IL_006c:  stelem.i1
    IL_006d:  ldsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_0072:  ldc.i4.s   10
    IL_0074:  ldc.i4.6
    IL_0075:  stelem.i1
    IL_0076:  ldsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_007b:  ldc.i4.4
    IL_007c:  ldc.i4.7
    IL_007d:  stelem.i1
    IL_007e:  ldsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_0083:  ldc.i4.s   11
    IL_0085:  ldc.i4.8
    IL_0086:  stelem.i1
    IL_0087:  ldsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_008c:  ldc.i4.s   15
    IL_008e:  ldc.i4.s   9
    IL_0090:  stelem.i1
    IL_0091:  ldsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_0096:  ldc.i4.s   14
    IL_0098:  ldc.i4.s   10
    IL_009a:  stelem.i1
    IL_009b:  ldsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_00a0:  ldc.i4.s   13
    IL_00a2:  ldc.i4.s   11
    IL_00a4:  stelem.i1
    IL_00a5:  ldsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_00aa:  ldc.i4.s   12
    IL_00ac:  ldc.i4.s   12
    IL_00ae:  stelem.i1
    IL_00af:  ret
  } // end of method TCompactProtocol::.ctor

  .method public hidebysig instance void 
          reset() cil managed
  {
    // Code size       19 (0x13)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Collections.Generic.Stack`1<int16> Thrift.Protocol.TCompactProtocol::lastField_
    IL_0006:  callvirt   instance void class [System]System.Collections.Generic.Stack`1<int16>::Clear()
    IL_000b:  ldarg.0
    IL_000c:  ldc.i4.0
    IL_000d:  stfld      int16 Thrift.Protocol.TCompactProtocol::lastFieldId_
    IL_0012:  ret
  } // end of method TCompactProtocol::reset

  .method private hidebysig instance void 
          WriteByteDirect(uint8 b) cil managed
  {
    // Code size       27 (0x1b)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      uint8[] Thrift.Protocol.TCompactProtocol::byteDirectBuffer
    IL_0006:  ldc.i4.0
    IL_0007:  ldarg.1
    IL_0008:  stelem.i1
    IL_0009:  ldarg.0
    IL_000a:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_000f:  ldarg.0
    IL_0010:  ldfld      uint8[] Thrift.Protocol.TCompactProtocol::byteDirectBuffer
    IL_0015:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_001a:  ret
  } // end of method TCompactProtocol::WriteByteDirect

  .method private hidebysig instance void 
          WriteByteDirect(int32 n) cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  conv.u1
    IL_0003:  call       instance void Thrift.Protocol.TCompactProtocol::WriteByteDirect(uint8)
    IL_0008:  ret
  } // end of method TCompactProtocol::WriteByteDirect

  .method private hidebysig instance void 
          WriteVarint32(uint32 n) cil managed
  {
    // Code size       87 (0x57)
    .maxstack  4
    .locals init (int32 V_0)
    IL_0000:  ldc.i4.0
    IL_0001:  stloc.0
    IL_0002:  ldarg.1
    IL_0003:  conv.u8
    IL_0004:  ldc.i4.s   -128
    IL_0006:  conv.i8
    IL_0007:  and
    IL_0008:  ldc.i4.0
    IL_0009:  conv.i8
    IL_000a:  bne.un     IL_0022

    IL_000f:  ldarg.0
    IL_0010:  ldfld      uint8[] Thrift.Protocol.TCompactProtocol::i32buf
    IL_0015:  ldloc.0
    IL_0016:  dup
    IL_0017:  ldc.i4.1
    IL_0018:  add
    IL_0019:  stloc.0
    IL_001a:  ldarg.1
    IL_001b:  conv.u1
    IL_001c:  stelem.i1
    IL_001d:  br         IL_0043

    IL_0022:  ldarg.0
    IL_0023:  ldfld      uint8[] Thrift.Protocol.TCompactProtocol::i32buf
    IL_0028:  ldloc.0
    IL_0029:  dup
    IL_002a:  ldc.i4.1
    IL_002b:  add
    IL_002c:  stloc.0
    IL_002d:  ldarg.1
    IL_002e:  ldc.i4.s   127
    IL_0030:  and
    IL_0031:  ldc.i4     0x80
    IL_0036:  or
    IL_0037:  conv.u1
    IL_0038:  stelem.i1
    IL_0039:  ldarg.1
    IL_003a:  ldc.i4.7
    IL_003b:  shr.un
    IL_003c:  starg.s    n
    IL_003e:  br         IL_0002

    IL_0043:  ldarg.0
    IL_0044:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0049:  ldarg.0
    IL_004a:  ldfld      uint8[] Thrift.Protocol.TCompactProtocol::i32buf
    IL_004f:  ldc.i4.0
    IL_0050:  ldloc.0
    IL_0051:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_0056:  ret
  } // end of method TCompactProtocol::WriteVarint32

  .method public hidebysig virtual instance void 
          WriteMessageBegin(valuetype Thrift.Protocol.TMessage message) cil managed
  {
    // Code size       62 (0x3e)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4     0x82
    IL_0006:  call       instance void Thrift.Protocol.TCompactProtocol::WriteByteDirect(uint8)
    IL_000b:  ldarg.0
    IL_000c:  ldc.i4.1
    IL_000d:  ldarga.s   message
    IL_000f:  call       instance valuetype Thrift.Protocol.TMessageType Thrift.Protocol.TMessage::get_Type()
    IL_0014:  ldc.i4.5
    IL_0015:  shl
    IL_0016:  ldc.i4     0xe0
    IL_001b:  and
    IL_001c:  or
    IL_001d:  conv.u1
    IL_001e:  call       instance void Thrift.Protocol.TCompactProtocol::WriteByteDirect(uint8)
    IL_0023:  ldarg.0
    IL_0024:  ldarga.s   message
    IL_0026:  call       instance int32 Thrift.Protocol.TMessage::get_SeqID()
    IL_002b:  call       instance void Thrift.Protocol.TCompactProtocol::WriteVarint32(uint32)
    IL_0030:  ldarg.0
    IL_0031:  ldarga.s   message
    IL_0033:  call       instance string Thrift.Protocol.TMessage::get_Name()
    IL_0038:  callvirt   instance void Thrift.Protocol.TProtocol::WriteString(string)
    IL_003d:  ret
  } // end of method TCompactProtocol::WriteMessageBegin

  .method public hidebysig virtual instance void 
          WriteStructBegin(valuetype Thrift.Protocol.TStruct strct) cil managed
  {
    // Code size       25 (0x19)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Collections.Generic.Stack`1<int16> Thrift.Protocol.TCompactProtocol::lastField_
    IL_0006:  ldarg.0
    IL_0007:  ldfld      int16 Thrift.Protocol.TCompactProtocol::lastFieldId_
    IL_000c:  callvirt   instance void class [System]System.Collections.Generic.Stack`1<int16>::Push(!0)
    IL_0011:  ldarg.0
    IL_0012:  ldc.i4.0
    IL_0013:  stfld      int16 Thrift.Protocol.TCompactProtocol::lastFieldId_
    IL_0018:  ret
  } // end of method TCompactProtocol::WriteStructBegin

  .method public hidebysig virtual instance void 
          WriteStructEnd() cil managed
  {
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  ldfld      class [System]System.Collections.Generic.Stack`1<int16> Thrift.Protocol.TCompactProtocol::lastField_
    IL_0007:  callvirt   instance !0 class [System]System.Collections.Generic.Stack`1<int16>::Pop()
    IL_000c:  stfld      int16 Thrift.Protocol.TCompactProtocol::lastFieldId_
    IL_0011:  ret
  } // end of method TCompactProtocol::WriteStructEnd

  .method public hidebysig virtual instance void 
          WriteFieldBegin(valuetype Thrift.Protocol.TField 'field') cil managed
  {
    // Code size       43 (0x2b)
    .maxstack  8
    IL_0000:  ldarga.s   'field'
    IL_0002:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
    IL_0007:  ldc.i4.2
    IL_0008:  bne.un     IL_001e

    IL_000d:  ldarg.0
    IL_000e:  ldarg.1
    IL_000f:  newobj     instance void valuetype [mscorlib]System.Nullable`1<valuetype Thrift.Protocol.TField>::.ctor(!0)
    IL_0014:  stfld      valuetype [mscorlib]System.Nullable`1<valuetype Thrift.Protocol.TField> Thrift.Protocol.TCompactProtocol::booleanField_
    IL_0019:  br         IL_002a

    IL_001e:  ldarg.0
    IL_001f:  ldarg.1
    IL_0020:  ldc.i4     0xff
    IL_0025:  call       instance void Thrift.Protocol.TCompactProtocol::WriteFieldBeginInternal(valuetype Thrift.Protocol.TField,
                                                                                                 uint8)
    IL_002a:  ret
  } // end of method TCompactProtocol::WriteFieldBegin

  .method private hidebysig instance void 
          WriteFieldBeginInternal(valuetype Thrift.Protocol.TField 'field',
                                  uint8 typeOverride) cil managed
  {
    // Code size       133 (0x85)
    .maxstack  3
    .locals init (uint8 V_0)
    IL_0000:  ldarg.2
    IL_0001:  ldc.i4     0xff
    IL_0006:  bne.un     IL_001d

    IL_000b:  ldarg.0
    IL_000c:  ldarga.s   'field'
    IL_000e:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
    IL_0013:  call       instance uint8 Thrift.Protocol.TCompactProtocol::getCompactType(valuetype Thrift.Protocol.TType)
    IL_0018:  br         IL_001e

    IL_001d:  ldarg.2
    IL_001e:  stloc.0
    IL_001f:  ldarga.s   'field'
    IL_0021:  call       instance int16 Thrift.Protocol.TField::get_ID()
    IL_0026:  ldarg.0
    IL_0027:  ldfld      int16 Thrift.Protocol.TCompactProtocol::lastFieldId_
    IL_002c:  ble        IL_0063

    IL_0031:  ldarga.s   'field'
    IL_0033:  call       instance int16 Thrift.Protocol.TField::get_ID()
    IL_0038:  ldarg.0
    IL_0039:  ldfld      int16 Thrift.Protocol.TCompactProtocol::lastFieldId_
    IL_003e:  sub
    IL_003f:  ldc.i4.s   15
    IL_0041:  bgt        IL_0063

    IL_0046:  ldarg.0
    IL_0047:  ldarga.s   'field'
    IL_0049:  call       instance int16 Thrift.Protocol.TField::get_ID()
    IL_004e:  ldarg.0
    IL_004f:  ldfld      int16 Thrift.Protocol.TCompactProtocol::lastFieldId_
    IL_0054:  sub
    IL_0055:  ldc.i4.4
    IL_0056:  shl
    IL_0057:  ldloc.0
    IL_0058:  or
    IL_0059:  call       instance void Thrift.Protocol.TCompactProtocol::WriteByteDirect(int32)
    IL_005e:  br         IL_0077

    IL_0063:  ldarg.0
    IL_0064:  ldloc.0
    IL_0065:  call       instance void Thrift.Protocol.TCompactProtocol::WriteByteDirect(uint8)
    IL_006a:  ldarg.0
    IL_006b:  ldarga.s   'field'
    IL_006d:  call       instance int16 Thrift.Protocol.TField::get_ID()
    IL_0072:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI16(int16)
    IL_0077:  ldarg.0
    IL_0078:  ldarga.s   'field'
    IL_007a:  call       instance int16 Thrift.Protocol.TField::get_ID()
    IL_007f:  stfld      int16 Thrift.Protocol.TCompactProtocol::lastFieldId_
    IL_0084:  ret
  } // end of method TCompactProtocol::WriteFieldBeginInternal

  .method public hidebysig virtual instance void 
          WriteFieldStop() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.0
    IL_0002:  call       instance void Thrift.Protocol.TCompactProtocol::WriteByteDirect(uint8)
    IL_0007:  ret
  } // end of method TCompactProtocol::WriteFieldStop

  .method public hidebysig virtual instance void 
          WriteMapBegin(valuetype Thrift.Protocol.TMap map) cil managed
  {
    // Code size       73 (0x49)
    .maxstack  4
    IL_0000:  ldarga.s   map
    IL_0002:  call       instance int32 Thrift.Protocol.TMap::get_Count()
    IL_0007:  brtrue     IL_0018

    IL_000c:  ldarg.0
    IL_000d:  ldc.i4.0
    IL_000e:  call       instance void Thrift.Protocol.TCompactProtocol::WriteByteDirect(int32)
    IL_0013:  br         IL_0048

    IL_0018:  ldarg.0
    IL_0019:  ldarga.s   map
    IL_001b:  call       instance int32 Thrift.Protocol.TMap::get_Count()
    IL_0020:  call       instance void Thrift.Protocol.TCompactProtocol::WriteVarint32(uint32)
    IL_0025:  ldarg.0
    IL_0026:  ldarg.0
    IL_0027:  ldarga.s   map
    IL_0029:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::get_KeyType()
    IL_002e:  call       instance uint8 Thrift.Protocol.TCompactProtocol::getCompactType(valuetype Thrift.Protocol.TType)
    IL_0033:  ldc.i4.4
    IL_0034:  shl
    IL_0035:  ldarg.0
    IL_0036:  ldarga.s   map
    IL_0038:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::get_ValueType()
    IL_003d:  call       instance uint8 Thrift.Protocol.TCompactProtocol::getCompactType(valuetype Thrift.Protocol.TType)
    IL_0042:  or
    IL_0043:  call       instance void Thrift.Protocol.TCompactProtocol::WriteByteDirect(int32)
    IL_0048:  ret
  } // end of method TCompactProtocol::WriteMapBegin

  .method public hidebysig virtual instance void 
          WriteListBegin(valuetype Thrift.Protocol.TList list) cil managed
  {
    // Code size       21 (0x15)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarga.s   list
    IL_0003:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TList::get_ElementType()
    IL_0008:  ldarga.s   list
    IL_000a:  call       instance int32 Thrift.Protocol.TList::get_Count()
    IL_000f:  call       instance void Thrift.Protocol.TCompactProtocol::WriteCollectionBegin(valuetype Thrift.Protocol.TType,
                                                                                              int32)
    IL_0014:  ret
  } // end of method TCompactProtocol::WriteListBegin

  .method public hidebysig virtual instance void 
          WriteSetBegin(valuetype Thrift.Protocol.TSet set) cil managed
  {
    // Code size       21 (0x15)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarga.s   set
    IL_0003:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TSet::get_ElementType()
    IL_0008:  ldarga.s   set
    IL_000a:  call       instance int32 Thrift.Protocol.TSet::get_Count()
    IL_000f:  call       instance void Thrift.Protocol.TCompactProtocol::WriteCollectionBegin(valuetype Thrift.Protocol.TType,
                                                                                              int32)
    IL_0014:  ret
  } // end of method TCompactProtocol::WriteSetBegin

  .method public hidebysig virtual instance void 
          WriteBool(bool b) cil managed
  {
    // Code size       89 (0x59)
    .maxstack  3
    .locals init (valuetype [mscorlib]System.Nullable`1<valuetype Thrift.Protocol.TField> V_0,
             valuetype [mscorlib]System.Nullable`1<valuetype Thrift.Protocol.TField> V_1)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      valuetype [mscorlib]System.Nullable`1<valuetype Thrift.Protocol.TField> Thrift.Protocol.TCompactProtocol::booleanField_
    IL_0006:  stloc.0
    IL_0007:  ldloca.s   V_0
    IL_0009:  call       instance bool valuetype [mscorlib]System.Nullable`1<valuetype Thrift.Protocol.TField>::get_HasValue()
    IL_000e:  brfalse    IL_0045

    IL_0013:  ldarg.0
    IL_0014:  ldarg.0
    IL_0015:  ldflda     valuetype [mscorlib]System.Nullable`1<valuetype Thrift.Protocol.TField> Thrift.Protocol.TCompactProtocol::booleanField_
    IL_001a:  call       instance !0 valuetype [mscorlib]System.Nullable`1<valuetype Thrift.Protocol.TField>::get_Value()
    IL_001f:  ldarg.1
    IL_0020:  brfalse    IL_002b

    IL_0025:  ldc.i4.1
    IL_0026:  br         IL_002c

    IL_002b:  ldc.i4.2
    IL_002c:  call       instance void Thrift.Protocol.TCompactProtocol::WriteFieldBeginInternal(valuetype Thrift.Protocol.TField,
                                                                                                 uint8)
    IL_0031:  ldarg.0
    IL_0032:  ldloca.s   V_1
    IL_0034:  initobj    valuetype [mscorlib]System.Nullable`1<valuetype Thrift.Protocol.TField>
    IL_003a:  ldloc.1
    IL_003b:  stfld      valuetype [mscorlib]System.Nullable`1<valuetype Thrift.Protocol.TField> Thrift.Protocol.TCompactProtocol::booleanField_
    IL_0040:  br         IL_0058

    IL_0045:  ldarg.0
    IL_0046:  ldarg.1
    IL_0047:  brfalse    IL_0052

    IL_004c:  ldc.i4.1
    IL_004d:  br         IL_0053

    IL_0052:  ldc.i4.2
    IL_0053:  call       instance void Thrift.Protocol.TCompactProtocol::WriteByteDirect(uint8)
    IL_0058:  ret
  } // end of method TCompactProtocol::WriteBool

  .method public hidebysig virtual instance void 
          WriteByte(int8 b) cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  conv.u1
    IL_0003:  call       instance void Thrift.Protocol.TCompactProtocol::WriteByteDirect(uint8)
    IL_0008:  ret
  } // end of method TCompactProtocol::WriteByte

  .method public hidebysig virtual instance void 
          WriteI16(int16 i16) cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  call       instance uint32 Thrift.Protocol.TCompactProtocol::intToZigZag(int32)
    IL_0008:  call       instance void Thrift.Protocol.TCompactProtocol::WriteVarint32(uint32)
    IL_000d:  ret
  } // end of method TCompactProtocol::WriteI16

  .method public hidebysig virtual instance void 
          WriteI32(int32 i32) cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  call       instance uint32 Thrift.Protocol.TCompactProtocol::intToZigZag(int32)
    IL_0008:  call       instance void Thrift.Protocol.TCompactProtocol::WriteVarint32(uint32)
    IL_000d:  ret
  } // end of method TCompactProtocol::WriteI32

  .method public hidebysig virtual instance void 
          WriteI64(int64 i64) cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  call       instance uint64 Thrift.Protocol.TCompactProtocol::longToZigzag(int64)
    IL_0008:  call       instance void Thrift.Protocol.TCompactProtocol::WriteVarint64(uint64)
    IL_000d:  ret
  } // end of method TCompactProtocol::WriteI64

  .method public hidebysig virtual instance void 
          WriteDouble(float64 dub) cil managed
  {
    // Code size       34 (0x22)
    .maxstack  4
    .locals init (uint8[] V_0)
    IL_0000:  ldc.i4.8
    IL_0001:  newarr     [mscorlib]System.Byte
    IL_0006:  stloc.0
    IL_0007:  ldarg.0
    IL_0008:  ldarg.1
    IL_0009:  call       int64 [mscorlib]System.BitConverter::DoubleToInt64Bits(float64)
    IL_000e:  ldloc.0
    IL_000f:  ldc.i4.0
    IL_0010:  call       instance void Thrift.Protocol.TCompactProtocol::fixedLongToBytes(int64,
                                                                                          uint8[],
                                                                                          int32)
    IL_0015:  ldarg.0
    IL_0016:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_001b:  ldloc.0
    IL_001c:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_0021:  ret
  } // end of method TCompactProtocol::WriteDouble

  .method public hidebysig virtual instance void 
          WriteString(string str) cil managed
  {
    // Code size       24 (0x18)
    .maxstack  4
    .locals init (uint8[] V_0)
    IL_0000:  call       class [mscorlib]System.Text.Encoding [mscorlib]System.Text.Encoding::get_UTF8()
    IL_0005:  ldarg.1
    IL_0006:  callvirt   instance uint8[] [mscorlib]System.Text.Encoding::GetBytes(string)
    IL_000b:  stloc.0
    IL_000c:  ldarg.0
    IL_000d:  ldloc.0
    IL_000e:  ldc.i4.0
    IL_000f:  ldloc.0
    IL_0010:  ldlen
    IL_0011:  conv.i4
    IL_0012:  call       instance void Thrift.Protocol.TCompactProtocol::WriteBinary(uint8[],
                                                                                     int32,
                                                                                     int32)
    IL_0017:  ret
  } // end of method TCompactProtocol::WriteString

  .method public hidebysig virtual instance void 
          WriteBinary(uint8[] bin) cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldc.i4.0
    IL_0003:  ldarg.1
    IL_0004:  ldlen
    IL_0005:  conv.i4
    IL_0006:  call       instance void Thrift.Protocol.TCompactProtocol::WriteBinary(uint8[],
                                                                                     int32,
                                                                                     int32)
    IL_000b:  ret
  } // end of method TCompactProtocol::WriteBinary

  .method private hidebysig instance void 
          WriteBinary(uint8[] buf,
                      int32 offset,
                      int32 length) cil managed
  {
    // Code size       22 (0x16)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.3
    IL_0002:  call       instance void Thrift.Protocol.TCompactProtocol::WriteVarint32(uint32)
    IL_0007:  ldarg.0
    IL_0008:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_000d:  ldarg.1
    IL_000e:  ldarg.2
    IL_000f:  ldarg.3
    IL_0010:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_0015:  ret
  } // end of method TCompactProtocol::WriteBinary

  .method public hidebysig virtual instance void 
          WriteMessageEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TCompactProtocol::WriteMessageEnd

  .method public hidebysig virtual instance void 
          WriteMapEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TCompactProtocol::WriteMapEnd

  .method public hidebysig virtual instance void 
          WriteListEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TCompactProtocol::WriteListEnd

  .method public hidebysig virtual instance void 
          WriteSetEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TCompactProtocol::WriteSetEnd

  .method public hidebysig virtual instance void 
          WriteFieldEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TCompactProtocol::WriteFieldEnd

  .method family hidebysig instance void 
          WriteCollectionBegin(valuetype Thrift.Protocol.TType elemType,
                               int32 size) cil managed
  {
    // Code size       57 (0x39)
    .maxstack  8
    IL_0000:  ldarg.2
    IL_0001:  ldc.i4.s   14
    IL_0003:  bgt        IL_001e

    IL_0008:  ldarg.0
    IL_0009:  ldarg.2
    IL_000a:  ldc.i4.4
    IL_000b:  shl
    IL_000c:  ldarg.0
    IL_000d:  ldarg.1
    IL_000e:  call       instance uint8 Thrift.Protocol.TCompactProtocol::getCompactType(valuetype Thrift.Protocol.TType)
    IL_0013:  or
    IL_0014:  call       instance void Thrift.Protocol.TCompactProtocol::WriteByteDirect(int32)
    IL_0019:  br         IL_0038

    IL_001e:  ldarg.0
    IL_001f:  ldc.i4     0xf0
    IL_0024:  ldarg.0
    IL_0025:  ldarg.1
    IL_0026:  call       instance uint8 Thrift.Protocol.TCompactProtocol::getCompactType(valuetype Thrift.Protocol.TType)
    IL_002b:  or
    IL_002c:  call       instance void Thrift.Protocol.TCompactProtocol::WriteByteDirect(int32)
    IL_0031:  ldarg.0
    IL_0032:  ldarg.2
    IL_0033:  call       instance void Thrift.Protocol.TCompactProtocol::WriteVarint32(uint32)
    IL_0038:  ret
  } // end of method TCompactProtocol::WriteCollectionBegin

  .method private hidebysig instance void 
          WriteVarint64(uint64 n) cil managed
  {
    // Code size       88 (0x58)
    .maxstack  4
    .locals init (int32 V_0)
    IL_0000:  ldc.i4.0
    IL_0001:  stloc.0
    IL_0002:  ldarg.1
    IL_0003:  ldc.i4.s   -128
    IL_0005:  conv.i8
    IL_0006:  and
    IL_0007:  ldc.i4.0
    IL_0008:  conv.i8
    IL_0009:  bne.un     IL_0021

    IL_000e:  ldarg.0
    IL_000f:  ldfld      uint8[] Thrift.Protocol.TCompactProtocol::varint64out
    IL_0014:  ldloc.0
    IL_0015:  dup
    IL_0016:  ldc.i4.1
    IL_0017:  add
    IL_0018:  stloc.0
    IL_0019:  ldarg.1
    IL_001a:  conv.u1
    IL_001b:  stelem.i1
    IL_001c:  br         IL_0044

    IL_0021:  ldarg.0
    IL_0022:  ldfld      uint8[] Thrift.Protocol.TCompactProtocol::varint64out
    IL_0027:  ldloc.0
    IL_0028:  dup
    IL_0029:  ldc.i4.1
    IL_002a:  add
    IL_002b:  stloc.0
    IL_002c:  ldarg.1
    IL_002d:  ldc.i4.s   127
    IL_002f:  conv.i8
    IL_0030:  and
    IL_0031:  ldc.i4     0x80
    IL_0036:  conv.i8
    IL_0037:  or
    IL_0038:  conv.u1
    IL_0039:  stelem.i1
    IL_003a:  ldarg.1
    IL_003b:  ldc.i4.7
    IL_003c:  shr.un
    IL_003d:  starg.s    n
    IL_003f:  br         IL_0002

    IL_0044:  ldarg.0
    IL_0045:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_004a:  ldarg.0
    IL_004b:  ldfld      uint8[] Thrift.Protocol.TCompactProtocol::varint64out
    IL_0050:  ldc.i4.0
    IL_0051:  ldloc.0
    IL_0052:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_0057:  ret
  } // end of method TCompactProtocol::WriteVarint64

  .method private hidebysig instance uint64 
          longToZigzag(int64 n) cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  ldc.i4.1
    IL_0002:  shl
    IL_0003:  ldarg.1
    IL_0004:  ldc.i4.s   63
    IL_0006:  shr
    IL_0007:  xor
    IL_0008:  ret
  } // end of method TCompactProtocol::longToZigzag

  .method private hidebysig instance uint32 
          intToZigZag(int32 n) cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  ldc.i4.1
    IL_0002:  shl
    IL_0003:  ldarg.1
    IL_0004:  ldc.i4.s   31
    IL_0006:  shr
    IL_0007:  xor
    IL_0008:  ret
  } // end of method TCompactProtocol::intToZigZag

  .method private hidebysig instance void 
          fixedLongToBytes(int64 n,
                           uint8[] buf,
                           int32 'off') cil managed
  {
    // Code size       131 (0x83)
    .maxstack  4
    IL_0000:  ldarg.2
    IL_0001:  ldarg.3
    IL_0002:  ldarg.1
    IL_0003:  ldc.i4     0xff
    IL_0008:  conv.i8
    IL_0009:  and
    IL_000a:  conv.u1
    IL_000b:  stelem.i1
    IL_000c:  ldarg.2
    IL_000d:  ldarg.3
    IL_000e:  ldc.i4.1
    IL_000f:  add
    IL_0010:  ldarg.1
    IL_0011:  ldc.i4.8
    IL_0012:  shr
    IL_0013:  ldc.i4     0xff
    IL_0018:  conv.i8
    IL_0019:  and
    IL_001a:  conv.u1
    IL_001b:  stelem.i1
    IL_001c:  ldarg.2
    IL_001d:  ldarg.3
    IL_001e:  ldc.i4.2
    IL_001f:  add
    IL_0020:  ldarg.1
    IL_0021:  ldc.i4.s   16
    IL_0023:  shr
    IL_0024:  ldc.i4     0xff
    IL_0029:  conv.i8
    IL_002a:  and
    IL_002b:  conv.u1
    IL_002c:  stelem.i1
    IL_002d:  ldarg.2
    IL_002e:  ldarg.3
    IL_002f:  ldc.i4.3
    IL_0030:  add
    IL_0031:  ldarg.1
    IL_0032:  ldc.i4.s   24
    IL_0034:  shr
    IL_0035:  ldc.i4     0xff
    IL_003a:  conv.i8
    IL_003b:  and
    IL_003c:  conv.u1
    IL_003d:  stelem.i1
    IL_003e:  ldarg.2
    IL_003f:  ldarg.3
    IL_0040:  ldc.i4.4
    IL_0041:  add
    IL_0042:  ldarg.1
    IL_0043:  ldc.i4.s   32
    IL_0045:  shr
    IL_0046:  ldc.i4     0xff
    IL_004b:  conv.i8
    IL_004c:  and
    IL_004d:  conv.u1
    IL_004e:  stelem.i1
    IL_004f:  ldarg.2
    IL_0050:  ldarg.3
    IL_0051:  ldc.i4.5
    IL_0052:  add
    IL_0053:  ldarg.1
    IL_0054:  ldc.i4.s   40
    IL_0056:  shr
    IL_0057:  ldc.i4     0xff
    IL_005c:  conv.i8
    IL_005d:  and
    IL_005e:  conv.u1
    IL_005f:  stelem.i1
    IL_0060:  ldarg.2
    IL_0061:  ldarg.3
    IL_0062:  ldc.i4.6
    IL_0063:  add
    IL_0064:  ldarg.1
    IL_0065:  ldc.i4.s   48
    IL_0067:  shr
    IL_0068:  ldc.i4     0xff
    IL_006d:  conv.i8
    IL_006e:  and
    IL_006f:  conv.u1
    IL_0070:  stelem.i1
    IL_0071:  ldarg.2
    IL_0072:  ldarg.3
    IL_0073:  ldc.i4.7
    IL_0074:  add
    IL_0075:  ldarg.1
    IL_0076:  ldc.i4.s   56
    IL_0078:  shr
    IL_0079:  ldc.i4     0xff
    IL_007e:  conv.i8
    IL_007f:  and
    IL_0080:  conv.u1
    IL_0081:  stelem.i1
    IL_0082:  ret
  } // end of method TCompactProtocol::fixedLongToBytes

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TMessage 
          ReadMessageBegin() cil managed
  {
    // Code size       178 (0xb2)
    .maxstack  5
    .locals init (uint8 V_0,
             uint8 V_1,
             uint8 V_2,
             uint8 V_3,
             uint8 V_4,
             int32 V_5,
             string V_6)
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_0006:  conv.u1
    IL_0007:  stloc.0
    IL_0008:  ldloc.0
    IL_0009:  ldc.i4     0x82
    IL_000e:  beq        IL_0046

    IL_0013:  ldstr      "Expected protocol id "
    IL_0018:  ldc.i4     0x82
    IL_001d:  stloc.1
    IL_001e:  ldloca.s   V_1
    IL_0020:  ldstr      "X"
    IL_0025:  call       instance string [mscorlib]System.Byte::ToString(string)
    IL_002a:  ldstr      " but got "
    IL_002f:  ldloca.s   V_0
    IL_0031:  ldstr      "X"
    IL_0036:  call       instance string [mscorlib]System.Byte::ToString(string)
    IL_003b:  call       string [mscorlib]System.String::Concat(string,
                                                                string,
                                                                string,
                                                                string)
    IL_0040:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(string)
    IL_0045:  throw

    IL_0046:  ldarg.0
    IL_0047:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_004c:  conv.u1
    IL_004d:  stloc.2
    IL_004e:  ldloc.2
    IL_004f:  ldc.i4.s   31
    IL_0051:  and
    IL_0052:  conv.u1
    IL_0053:  stloc.3
    IL_0054:  ldloc.3
    IL_0055:  ldc.i4.1
    IL_0056:  beq        IL_008e

    IL_005b:  ldc.i4.4
    IL_005c:  newarr     [mscorlib]System.Object
    IL_0061:  dup
    IL_0062:  ldc.i4.0
    IL_0063:  ldstr      "Expected version "
    IL_0068:  stelem.ref
    IL_0069:  dup
    IL_006a:  ldc.i4.1
    IL_006b:  ldc.i4.1
    IL_006c:  box        [mscorlib]System.Byte
    IL_0071:  stelem.ref
    IL_0072:  dup
    IL_0073:  ldc.i4.2
    IL_0074:  ldstr      " but got "
    IL_0079:  stelem.ref
    IL_007a:  dup
    IL_007b:  ldc.i4.3
    IL_007c:  ldloc.3
    IL_007d:  box        [mscorlib]System.Byte
    IL_0082:  stelem.ref
    IL_0083:  call       string [mscorlib]System.String::Concat(object[])
    IL_0088:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(string)
    IL_008d:  throw

    IL_008e:  ldloc.2
    IL_008f:  ldc.i4.5
    IL_0090:  shr
    IL_0091:  ldc.i4.7
    IL_0092:  and
    IL_0093:  conv.u1
    IL_0094:  stloc.s    V_4
    IL_0096:  ldarg.0
    IL_0097:  call       instance uint32 Thrift.Protocol.TCompactProtocol::ReadVarint32()
    IL_009c:  stloc.s    V_5
    IL_009e:  ldarg.0
    IL_009f:  callvirt   instance string Thrift.Protocol.TProtocol::ReadString()
    IL_00a4:  stloc.s    V_6
    IL_00a6:  ldloc.s    V_6
    IL_00a8:  ldloc.s    V_4
    IL_00aa:  ldloc.s    V_5
    IL_00ac:  newobj     instance void Thrift.Protocol.TMessage::.ctor(string,
                                                                       valuetype Thrift.Protocol.TMessageType,
                                                                       int32)
    IL_00b1:  ret
  } // end of method TCompactProtocol::ReadMessageBegin

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TStruct 
          ReadStructBegin() cil managed
  {
    // Code size       30 (0x1e)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Collections.Generic.Stack`1<int16> Thrift.Protocol.TCompactProtocol::lastField_
    IL_0006:  ldarg.0
    IL_0007:  ldfld      int16 Thrift.Protocol.TCompactProtocol::lastFieldId_
    IL_000c:  callvirt   instance void class [System]System.Collections.Generic.Stack`1<int16>::Push(!0)
    IL_0011:  ldarg.0
    IL_0012:  ldc.i4.0
    IL_0013:  stfld      int16 Thrift.Protocol.TCompactProtocol::lastFieldId_
    IL_0018:  ldsfld     valuetype Thrift.Protocol.TStruct Thrift.Protocol.TCompactProtocol::ANONYMOUS_STRUCT
    IL_001d:  ret
  } // end of method TCompactProtocol::ReadStructBegin

  .method public hidebysig virtual instance void 
          ReadStructEnd() cil managed
  {
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  ldfld      class [System]System.Collections.Generic.Stack`1<int16> Thrift.Protocol.TCompactProtocol::lastField_
    IL_0007:  callvirt   instance !0 class [System]System.Collections.Generic.Stack`1<int16>::Pop()
    IL_000c:  stfld      int16 Thrift.Protocol.TCompactProtocol::lastFieldId_
    IL_0011:  ret
  } // end of method TCompactProtocol::ReadStructEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TField 
          ReadFieldBegin() cil managed
  {
    // Code size       139 (0x8b)
    .maxstack  5
    .locals init (uint8 V_0,
             int16 V_1,
             int16 V_2,
             valuetype Thrift.Protocol.TField V_3)
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_0006:  conv.u1
    IL_0007:  stloc.0
    IL_0008:  ldloc.0
    IL_0009:  brtrue     IL_0014

    IL_000e:  ldsfld     valuetype Thrift.Protocol.TField Thrift.Protocol.TCompactProtocol::TSTOP
    IL_0013:  ret

    IL_0014:  ldloc.0
    IL_0015:  ldc.i4     0xf0
    IL_001a:  and
    IL_001b:  ldc.i4.4
    IL_001c:  shr
    IL_001d:  conv.i2
    IL_001e:  stloc.2
    IL_001f:  ldloc.2
    IL_0020:  brtrue     IL_0031

    IL_0025:  ldarg.0
    IL_0026:  callvirt   instance int16 Thrift.Protocol.TProtocol::ReadI16()
    IL_002b:  stloc.1
    IL_002c:  br         IL_003b

    IL_0031:  ldarg.0
    IL_0032:  ldfld      int16 Thrift.Protocol.TCompactProtocol::lastFieldId_
    IL_0037:  ldloc.2
    IL_0038:  add
    IL_0039:  conv.i2
    IL_003a:  stloc.1
    IL_003b:  ldloca.s   V_3
    IL_003d:  ldsfld     string [mscorlib]System.String::Empty
    IL_0042:  ldarg.0
    IL_0043:  ldloc.0
    IL_0044:  ldc.i4.s   15
    IL_0046:  and
    IL_0047:  conv.u1
    IL_0048:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TCompactProtocol::getTType(uint8)
    IL_004d:  ldloc.1
    IL_004e:  call       instance void Thrift.Protocol.TField::.ctor(string,
                                                                     valuetype Thrift.Protocol.TType,
                                                                     int16)
    IL_0053:  ldarg.0
    IL_0054:  ldloc.0
    IL_0055:  call       instance bool Thrift.Protocol.TCompactProtocol::isBoolType(uint8)
    IL_005a:  brfalse    IL_007c

    IL_005f:  ldarg.0
    IL_0060:  ldloc.0
    IL_0061:  ldc.i4.s   15
    IL_0063:  and
    IL_0064:  conv.u1
    IL_0065:  ldc.i4.1
    IL_0066:  bne.un     IL_0071

    IL_006b:  ldc.i4.1
    IL_006c:  br         IL_0072

    IL_0071:  ldc.i4.0
    IL_0072:  newobj     instance void valuetype [mscorlib]System.Nullable`1<bool>::.ctor(!0)
    IL_0077:  stfld      valuetype [mscorlib]System.Nullable`1<bool> Thrift.Protocol.TCompactProtocol::boolValue_
    IL_007c:  ldarg.0
    IL_007d:  ldloca.s   V_3
    IL_007f:  call       instance int16 Thrift.Protocol.TField::get_ID()
    IL_0084:  stfld      int16 Thrift.Protocol.TCompactProtocol::lastFieldId_
    IL_0089:  ldloc.3
    IL_008a:  ret
  } // end of method TCompactProtocol::ReadFieldBegin

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TMap 
          ReadMapBegin() cil managed
  {
    // Code size       55 (0x37)
    .maxstack  4
    .locals init (int32 V_0,
             uint8 V_1)
    IL_0000:  ldarg.0
    IL_0001:  call       instance uint32 Thrift.Protocol.TCompactProtocol::ReadVarint32()
    IL_0006:  stloc.0
    IL_0007:  ldloc.0
    IL_0008:  brtrue     IL_0013

    IL_000d:  ldc.i4.0
    IL_000e:  br         IL_001a

    IL_0013:  ldarg.0
    IL_0014:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_0019:  conv.u1
    IL_001a:  stloc.1
    IL_001b:  ldarg.0
    IL_001c:  ldloc.1
    IL_001d:  ldc.i4.4
    IL_001e:  shr
    IL_001f:  conv.u1
    IL_0020:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TCompactProtocol::getTType(uint8)
    IL_0025:  ldarg.0
    IL_0026:  ldloc.1
    IL_0027:  ldc.i4.s   15
    IL_0029:  and
    IL_002a:  conv.u1
    IL_002b:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TCompactProtocol::getTType(uint8)
    IL_0030:  ldloc.0
    IL_0031:  newobj     instance void Thrift.Protocol.TMap::.ctor(valuetype Thrift.Protocol.TType,
                                                                   valuetype Thrift.Protocol.TType,
                                                                   int32)
    IL_0036:  ret
  } // end of method TCompactProtocol::ReadMapBegin

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TList 
          ReadListBegin() cil managed
  {
    // Code size       46 (0x2e)
    .maxstack  2
    .locals init (uint8 V_0,
             int32 V_1,
             valuetype Thrift.Protocol.TType V_2)
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_0006:  conv.u1
    IL_0007:  stloc.0
    IL_0008:  ldloc.0
    IL_0009:  ldc.i4.4
    IL_000a:  shr
    IL_000b:  ldc.i4.s   15
    IL_000d:  and
    IL_000e:  stloc.1
    IL_000f:  ldloc.1
    IL_0010:  ldc.i4.s   15
    IL_0012:  bne.un     IL_001e

    IL_0017:  ldarg.0
    IL_0018:  call       instance uint32 Thrift.Protocol.TCompactProtocol::ReadVarint32()
    IL_001d:  stloc.1
    IL_001e:  ldarg.0
    IL_001f:  ldloc.0
    IL_0020:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TCompactProtocol::getTType(uint8)
    IL_0025:  stloc.2
    IL_0026:  ldloc.2
    IL_0027:  ldloc.1
    IL_0028:  newobj     instance void Thrift.Protocol.TList::.ctor(valuetype Thrift.Protocol.TType,
                                                                    int32)
    IL_002d:  ret
  } // end of method TCompactProtocol::ReadListBegin

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TSet 
          ReadSetBegin() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance valuetype Thrift.Protocol.TList Thrift.Protocol.TProtocol::ReadListBegin()
    IL_0006:  newobj     instance void Thrift.Protocol.TSet::.ctor(valuetype Thrift.Protocol.TList)
    IL_000b:  ret
  } // end of method TCompactProtocol::ReadSetBegin

  .method public hidebysig virtual instance bool 
          ReadBool() cil managed
  {
    // Code size       59 (0x3b)
    .maxstack  2
    .locals init (valuetype [mscorlib]System.Nullable`1<bool> V_0,
             bool V_1,
             valuetype [mscorlib]System.Nullable`1<bool> V_2)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      valuetype [mscorlib]System.Nullable`1<bool> Thrift.Protocol.TCompactProtocol::boolValue_
    IL_0006:  stloc.0
    IL_0007:  ldloca.s   V_0
    IL_0009:  call       instance bool valuetype [mscorlib]System.Nullable`1<bool>::get_HasValue()
    IL_000e:  brfalse    IL_0030

    IL_0013:  ldarg.0
    IL_0014:  ldflda     valuetype [mscorlib]System.Nullable`1<bool> Thrift.Protocol.TCompactProtocol::boolValue_
    IL_0019:  call       instance !0 valuetype [mscorlib]System.Nullable`1<bool>::get_Value()
    IL_001e:  stloc.1
    IL_001f:  ldarg.0
    IL_0020:  ldloca.s   V_2
    IL_0022:  initobj    valuetype [mscorlib]System.Nullable`1<bool>
    IL_0028:  ldloc.2
    IL_0029:  stfld      valuetype [mscorlib]System.Nullable`1<bool> Thrift.Protocol.TCompactProtocol::boolValue_
    IL_002e:  ldloc.1
    IL_002f:  ret

    IL_0030:  ldarg.0
    IL_0031:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_0036:  conv.i4
    IL_0037:  ldc.i4.1
    IL_0038:  ceq
    IL_003a:  ret
  } // end of method TCompactProtocol::ReadBool

  .method public hidebysig virtual instance int8 
          ReadByte() cil managed
  {
    // Code size       30 (0x1e)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0006:  ldarg.0
    IL_0007:  ldfld      uint8[] Thrift.Protocol.TCompactProtocol::byteRawBuf
    IL_000c:  ldc.i4.0
    IL_000d:  ldc.i4.1
    IL_000e:  callvirt   instance int32 Thrift.Transport.TTransport::ReadAll(uint8[],
                                                                             int32,
                                                                             int32)
    IL_0013:  pop
    IL_0014:  ldarg.0
    IL_0015:  ldfld      uint8[] Thrift.Protocol.TCompactProtocol::byteRawBuf
    IL_001a:  ldc.i4.0
    IL_001b:  ldelem.u1
    IL_001c:  conv.i1
    IL_001d:  ret
  } // end of method TCompactProtocol::ReadByte

  .method public hidebysig virtual instance int16 
          ReadI16() cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  call       instance uint32 Thrift.Protocol.TCompactProtocol::ReadVarint32()
    IL_0007:  call       instance int32 Thrift.Protocol.TCompactProtocol::zigzagToInt(uint32)
    IL_000c:  conv.i2
    IL_000d:  ret
  } // end of method TCompactProtocol::ReadI16

  .method public hidebysig virtual instance int32 
          ReadI32() cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  call       instance uint32 Thrift.Protocol.TCompactProtocol::ReadVarint32()
    IL_0007:  call       instance int32 Thrift.Protocol.TCompactProtocol::zigzagToInt(uint32)
    IL_000c:  ret
  } // end of method TCompactProtocol::ReadI32

  .method public hidebysig virtual instance int64 
          ReadI64() cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  call       instance uint64 Thrift.Protocol.TCompactProtocol::ReadVarint64()
    IL_0007:  call       instance int64 Thrift.Protocol.TCompactProtocol::zigzagToLong(uint64)
    IL_000c:  ret
  } // end of method TCompactProtocol::ReadI64

  .method public hidebysig virtual instance float64 
          ReadDouble() cil managed
  {
    // Code size       35 (0x23)
    .maxstack  4
    .locals init (uint8[] V_0)
    IL_0000:  ldc.i4.8
    IL_0001:  newarr     [mscorlib]System.Byte
    IL_0006:  stloc.0
    IL_0007:  ldarg.0
    IL_0008:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_000d:  ldloc.0
    IL_000e:  ldc.i4.0
    IL_000f:  ldc.i4.8
    IL_0010:  callvirt   instance int32 Thrift.Transport.TTransport::ReadAll(uint8[],
                                                                             int32,
                                                                             int32)
    IL_0015:  pop
    IL_0016:  ldarg.0
    IL_0017:  ldloc.0
    IL_0018:  call       instance int64 Thrift.Protocol.TCompactProtocol::bytesToLong(uint8[])
    IL_001d:  call       float64 [mscorlib]System.BitConverter::Int64BitsToDouble(int64)
    IL_0022:  ret
  } // end of method TCompactProtocol::ReadDouble

  .method public hidebysig virtual instance string 
          ReadString() cil managed
  {
    // Code size       37 (0x25)
    .maxstack  3
    .locals init (int32 V_0)
    IL_0000:  ldarg.0
    IL_0001:  call       instance uint32 Thrift.Protocol.TCompactProtocol::ReadVarint32()
    IL_0006:  stloc.0
    IL_0007:  ldloc.0
    IL_0008:  brtrue     IL_0013

    IL_000d:  ldsfld     string [mscorlib]System.String::Empty
    IL_0012:  ret

    IL_0013:  call       class [mscorlib]System.Text.Encoding [mscorlib]System.Text.Encoding::get_UTF8()
    IL_0018:  ldarg.0
    IL_0019:  ldloc.0
    IL_001a:  call       instance uint8[] Thrift.Protocol.TCompactProtocol::ReadBinary(int32)
    IL_001f:  callvirt   instance string [mscorlib]System.Text.Encoding::GetString(uint8[])
    IL_0024:  ret
  } // end of method TCompactProtocol::ReadString

  .method public hidebysig virtual instance uint8[] 
          ReadBinary() cil managed
  {
    // Code size       44 (0x2c)
    .maxstack  4
    .locals init (int32 V_0,
             uint8[] V_1)
    IL_0000:  ldarg.0
    IL_0001:  call       instance uint32 Thrift.Protocol.TCompactProtocol::ReadVarint32()
    IL_0006:  stloc.0
    IL_0007:  ldloc.0
    IL_0008:  brtrue     IL_0014

    IL_000d:  ldc.i4.0
    IL_000e:  newarr     [mscorlib]System.Byte
    IL_0013:  ret

    IL_0014:  ldloc.0
    IL_0015:  newarr     [mscorlib]System.Byte
    IL_001a:  stloc.1
    IL_001b:  ldarg.0
    IL_001c:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0021:  ldloc.1
    IL_0022:  ldc.i4.0
    IL_0023:  ldloc.0
    IL_0024:  callvirt   instance int32 Thrift.Transport.TTransport::ReadAll(uint8[],
                                                                             int32,
                                                                             int32)
    IL_0029:  pop
    IL_002a:  ldloc.1
    IL_002b:  ret
  } // end of method TCompactProtocol::ReadBinary

  .method private hidebysig instance uint8[] 
          ReadBinary(int32 length) cil managed
  {
    // Code size       37 (0x25)
    .maxstack  4
    .locals init (uint8[] V_0)
    IL_0000:  ldarg.1
    IL_0001:  brtrue     IL_000d

    IL_0006:  ldc.i4.0
    IL_0007:  newarr     [mscorlib]System.Byte
    IL_000c:  ret

    IL_000d:  ldarg.1
    IL_000e:  newarr     [mscorlib]System.Byte
    IL_0013:  stloc.0
    IL_0014:  ldarg.0
    IL_0015:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_001a:  ldloc.0
    IL_001b:  ldc.i4.0
    IL_001c:  ldarg.1
    IL_001d:  callvirt   instance int32 Thrift.Transport.TTransport::ReadAll(uint8[],
                                                                             int32,
                                                                             int32)
    IL_0022:  pop
    IL_0023:  ldloc.0
    IL_0024:  ret
  } // end of method TCompactProtocol::ReadBinary

  .method public hidebysig virtual instance void 
          ReadMessageEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TCompactProtocol::ReadMessageEnd

  .method public hidebysig virtual instance void 
          ReadFieldEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TCompactProtocol::ReadFieldEnd

  .method public hidebysig virtual instance void 
          ReadMapEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TCompactProtocol::ReadMapEnd

  .method public hidebysig virtual instance void 
          ReadListEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TCompactProtocol::ReadListEnd

  .method public hidebysig virtual instance void 
          ReadSetEnd() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TCompactProtocol::ReadSetEnd

  .method private hidebysig instance uint32 
          ReadVarint32() cil managed
  {
    // Code size       57 (0x39)
    .maxstack  4
    .locals init (uint32 V_0,
             int32 V_1,
             uint8 V_2)
    IL_0000:  ldc.i4.0
    IL_0001:  stloc.0
    IL_0002:  ldc.i4.0
    IL_0003:  stloc.1
    IL_0004:  ldarg.0
    IL_0005:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_000a:  conv.u1
    IL_000b:  stloc.2
    IL_000c:  ldloc.0
    IL_000d:  ldloc.2
    IL_000e:  ldc.i4.s   127
    IL_0010:  and
    IL_0011:  ldloc.1
    IL_0012:  ldc.i4.s   31
    IL_0014:  and
    IL_0015:  shl
    IL_0016:  or
    IL_0017:  stloc.0
    IL_0018:  ldloc.2
    IL_0019:  ldc.i4     0x80
    IL_001e:  and
    IL_001f:  ldc.i4     0x80
    IL_0024:  beq        IL_002e

    IL_0029:  br         IL_0037

    IL_002e:  ldloc.1
    IL_002f:  ldc.i4.7
    IL_0030:  add
    IL_0031:  stloc.1
    IL_0032:  br         IL_0004

    IL_0037:  ldloc.0
    IL_0038:  ret
  } // end of method TCompactProtocol::ReadVarint32

  .method private hidebysig instance uint64 
          ReadVarint64() cil managed
  {
    // Code size       59 (0x3b)
    .maxstack  4
    .locals init (int32 V_0,
             uint64 V_1,
             uint8 V_2)
    IL_0000:  ldc.i4.0
    IL_0001:  stloc.0
    IL_0002:  ldc.i4.0
    IL_0003:  conv.i8
    IL_0004:  stloc.1
    IL_0005:  ldarg.0
    IL_0006:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_000b:  conv.u1
    IL_000c:  stloc.2
    IL_000d:  ldloc.1
    IL_000e:  ldloc.2
    IL_000f:  ldc.i4.s   127
    IL_0011:  and
    IL_0012:  conv.i8
    IL_0013:  ldloc.0
    IL_0014:  ldc.i4.s   63
    IL_0016:  and
    IL_0017:  shl
    IL_0018:  or
    IL_0019:  stloc.1
    IL_001a:  ldloc.2
    IL_001b:  ldc.i4     0x80
    IL_0020:  and
    IL_0021:  ldc.i4     0x80
    IL_0026:  beq        IL_0030

    IL_002b:  br         IL_0039

    IL_0030:  ldloc.0
    IL_0031:  ldc.i4.7
    IL_0032:  add
    IL_0033:  stloc.0
    IL_0034:  br         IL_0005

    IL_0039:  ldloc.1
    IL_003a:  ret
  } // end of method TCompactProtocol::ReadVarint64

  .method private hidebysig instance int32 
          zigzagToInt(uint32 n) cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  ldc.i4.1
    IL_0002:  shr.un
    IL_0003:  ldarg.1
    IL_0004:  ldc.i4.1
    IL_0005:  and
    IL_0006:  neg
    IL_0007:  xor
    IL_0008:  ret
  } // end of method TCompactProtocol::zigzagToInt

  .method private hidebysig instance int64 
          zigzagToLong(uint64 n) cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  ldc.i4.1
    IL_0002:  shr.un
    IL_0003:  ldarg.1
    IL_0004:  ldc.i4.1
    IL_0005:  conv.i8
    IL_0006:  and
    IL_0007:  neg
    IL_0008:  xor
    IL_0009:  ret
  } // end of method TCompactProtocol::zigzagToLong

  .method private hidebysig instance int64 
          bytesToLong(uint8[] bytes) cil managed
  {
    // Code size       116 (0x74)
    .maxstack  3
    IL_0000:  ldarg.1
    IL_0001:  ldc.i4.7
    IL_0002:  ldelem.u1
    IL_0003:  conv.i8
    IL_0004:  ldc.i4     0xff
    IL_0009:  conv.i8
    IL_000a:  and
    IL_000b:  ldc.i4.s   56
    IL_000d:  shl
    IL_000e:  ldarg.1
    IL_000f:  ldc.i4.6
    IL_0010:  ldelem.u1
    IL_0011:  conv.i8
    IL_0012:  ldc.i4     0xff
    IL_0017:  conv.i8
    IL_0018:  and
    IL_0019:  ldc.i4.s   48
    IL_001b:  shl
    IL_001c:  or
    IL_001d:  ldarg.1
    IL_001e:  ldc.i4.5
    IL_001f:  ldelem.u1
    IL_0020:  conv.i8
    IL_0021:  ldc.i4     0xff
    IL_0026:  conv.i8
    IL_0027:  and
    IL_0028:  ldc.i4.s   40
    IL_002a:  shl
    IL_002b:  or
    IL_002c:  ldarg.1
    IL_002d:  ldc.i4.4
    IL_002e:  ldelem.u1
    IL_002f:  conv.i8
    IL_0030:  ldc.i4     0xff
    IL_0035:  conv.i8
    IL_0036:  and
    IL_0037:  ldc.i4.s   32
    IL_0039:  shl
    IL_003a:  or
    IL_003b:  ldarg.1
    IL_003c:  ldc.i4.3
    IL_003d:  ldelem.u1
    IL_003e:  conv.i8
    IL_003f:  ldc.i4     0xff
    IL_0044:  conv.i8
    IL_0045:  and
    IL_0046:  ldc.i4.s   24
    IL_0048:  shl
    IL_0049:  or
    IL_004a:  ldarg.1
    IL_004b:  ldc.i4.2
    IL_004c:  ldelem.u1
    IL_004d:  conv.i8
    IL_004e:  ldc.i4     0xff
    IL_0053:  conv.i8
    IL_0054:  and
    IL_0055:  ldc.i4.s   16
    IL_0057:  shl
    IL_0058:  or
    IL_0059:  ldarg.1
    IL_005a:  ldc.i4.1
    IL_005b:  ldelem.u1
    IL_005c:  conv.i8
    IL_005d:  ldc.i4     0xff
    IL_0062:  conv.i8
    IL_0063:  and
    IL_0064:  ldc.i4.8
    IL_0065:  shl
    IL_0066:  or
    IL_0067:  ldarg.1
    IL_0068:  ldc.i4.0
    IL_0069:  ldelem.u1
    IL_006a:  conv.i8
    IL_006b:  ldc.i4     0xff
    IL_0070:  conv.i8
    IL_0071:  and
    IL_0072:  or
    IL_0073:  ret
  } // end of method TCompactProtocol::bytesToLong

  .method private hidebysig instance bool 
          isBoolType(uint8 b) cil managed
  {
    // Code size       20 (0x14)
    .maxstack  2
    .locals init (int32 V_0)
    IL_0000:  ldarg.1
    IL_0001:  ldc.i4.s   15
    IL_0003:  and
    IL_0004:  stloc.0
    IL_0005:  ldloc.0
    IL_0006:  ldc.i4.1
    IL_0007:  beq        IL_0012

    IL_000c:  ldloc.0
    IL_000d:  ldc.i4.2
    IL_000e:  ceq
    IL_0010:  br.s       IL_0013

    IL_0012:  ldc.i4.1
    IL_0013:  ret
  } // end of method TCompactProtocol::isBoolType

  .method private hidebysig instance valuetype Thrift.Protocol.TType 
          getTType(uint8 'type') cil managed
  {
    // Code size       125 (0x7d)
    .maxstack  3
    .locals init (uint8 V_0)
    IL_0000:  ldarg.1
    IL_0001:  ldc.i4.s   15
    IL_0003:  and
    IL_0004:  conv.u1
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  switch     ( 
                          IL_0045,
                          IL_0047,
                          IL_0047,
                          IL_0049,
                          IL_004b,
                          IL_004d,
                          IL_004f,
                          IL_0052,
                          IL_0054,
                          IL_0057,
                          IL_005a,
                          IL_005d,
                          IL_0060)
    IL_0040:  br         IL_0063

    IL_0045:  ldc.i4.0
    IL_0046:  ret

    IL_0047:  ldc.i4.2
    IL_0048:  ret

    IL_0049:  ldc.i4.3
    IL_004a:  ret

    IL_004b:  ldc.i4.6
    IL_004c:  ret

    IL_004d:  ldc.i4.8
    IL_004e:  ret

    IL_004f:  ldc.i4.s   10
    IL_0051:  ret

    IL_0052:  ldc.i4.4
    IL_0053:  ret

    IL_0054:  ldc.i4.s   11
    IL_0056:  ret

    IL_0057:  ldc.i4.s   15
    IL_0059:  ret

    IL_005a:  ldc.i4.s   14
    IL_005c:  ret

    IL_005d:  ldc.i4.s   13
    IL_005f:  ret

    IL_0060:  ldc.i4.s   12
    IL_0062:  ret

    IL_0063:  ldstr      "don't know what type: "
    IL_0068:  ldarg.1
    IL_0069:  ldc.i4.s   15
    IL_006b:  and
    IL_006c:  conv.u1
    IL_006d:  box        [mscorlib]System.Byte
    IL_0072:  call       string [mscorlib]System.String::Concat(object,
                                                                object)
    IL_0077:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(string)
    IL_007c:  throw
  } // end of method TCompactProtocol::getTType

  .method private hidebysig instance uint8 
          getCompactType(valuetype Thrift.Protocol.TType ttype) cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_0005:  ldarg.1
    IL_0006:  ldelem.u1
    IL_0007:  ret
  } // end of method TCompactProtocol::getCompactType

  .method private hidebysig specialname rtspecialname static 
          void  .cctor() cil managed
  {
    // Code size       45 (0x2d)
    .maxstack  8
    IL_0000:  ldsfld     string [mscorlib]System.String::Empty
    IL_0005:  newobj     instance void Thrift.Protocol.TStruct::.ctor(string)
    IL_000a:  stsfld     valuetype Thrift.Protocol.TStruct Thrift.Protocol.TCompactProtocol::ANONYMOUS_STRUCT
    IL_000f:  ldsfld     string [mscorlib]System.String::Empty
    IL_0014:  ldc.i4.0
    IL_0015:  ldc.i4.0
    IL_0016:  newobj     instance void Thrift.Protocol.TField::.ctor(string,
                                                                     valuetype Thrift.Protocol.TType,
                                                                     int16)
    IL_001b:  stsfld     valuetype Thrift.Protocol.TField Thrift.Protocol.TCompactProtocol::TSTOP
    IL_0020:  ldc.i4.s   16
    IL_0022:  newarr     [mscorlib]System.Byte
    IL_0027:  stsfld     uint8[] Thrift.Protocol.TCompactProtocol::ttypeToCompactType
    IL_002c:  ret
  } // end of method TCompactProtocol::.cctor

} // end of class Thrift.Protocol.TCompactProtocol

.class public sequential ansi sealed beforefieldinit Thrift.Protocol.TField
       extends [mscorlib]System.ValueType
{
  .field private string name
  .field private valuetype Thrift.Protocol.TType 'type'
  .field private int16 id
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string name,
                               valuetype Thrift.Protocol.TType 'type',
                               int16 id) cil managed
  {
    // Code size       29 (0x1d)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  initobj    Thrift.Protocol.TField
    IL_0007:  ldarg.0
    IL_0008:  ldarg.1
    IL_0009:  stfld      string Thrift.Protocol.TField::name
    IL_000e:  ldarg.0
    IL_000f:  ldarg.2
    IL_0010:  stfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TField::'type'
    IL_0015:  ldarg.0
    IL_0016:  ldarg.3
    IL_0017:  stfld      int16 Thrift.Protocol.TField::id
    IL_001c:  ret
  } // end of method TField::.ctor

  .method public hidebysig specialname instance string 
          get_Name() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      string Thrift.Protocol.TField::name
    IL_0006:  ret
  } // end of method TField::get_Name

  .method public hidebysig specialname instance void 
          set_Name(string 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      string Thrift.Protocol.TField::name
    IL_0007:  ret
  } // end of method TField::set_Name

  .method public hidebysig specialname instance valuetype Thrift.Protocol.TType 
          get_Type() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TField::'type'
    IL_0006:  ret
  } // end of method TField::get_Type

  .method public hidebysig specialname instance void 
          set_Type(valuetype Thrift.Protocol.TType 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TField::'type'
    IL_0007:  ret
  } // end of method TField::set_Type

  .method public hidebysig specialname instance int16 
          get_ID() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      int16 Thrift.Protocol.TField::id
    IL_0006:  ret
  } // end of method TField::get_ID

  .method public hidebysig specialname instance void 
          set_ID(int16 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      int16 Thrift.Protocol.TField::id
    IL_0007:  ret
  } // end of method TField::set_ID

  .property instance string Name()
  {
    .get instance string Thrift.Protocol.TField::get_Name()
    .set instance void Thrift.Protocol.TField::set_Name(string)
  } // end of property TField::Name
  .property instance valuetype Thrift.Protocol.TType
          Type()
  {
    .get instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
    .set instance void Thrift.Protocol.TField::set_Type(valuetype Thrift.Protocol.TType)
  } // end of property TField::Type
  .property instance int16 ID()
  {
    .get instance int16 Thrift.Protocol.TField::get_ID()
    .set instance void Thrift.Protocol.TField::set_ID(int16)
  } // end of property TField::ID
} // end of class Thrift.Protocol.TField

.class public auto ansi beforefieldinit Thrift.Protocol.TJSONProtocol
       extends Thrift.Protocol.TProtocol
{
  .class auto ansi nested public beforefieldinit Factory
         extends [mscorlib]System.Object
         implements Thrift.Protocol.TProtocolFactory
  {
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ret
    } // end of method Factory::.ctor

    .method public hidebysig newslot virtual final 
            instance class Thrift.Protocol.TProtocol 
            GetProtocol(class Thrift.Transport.TTransport trans) cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  newobj     instance void Thrift.Protocol.TJSONProtocol::.ctor(class Thrift.Transport.TTransport)
      IL_0006:  ret
    } // end of method Factory::GetProtocol

  } // end of class Factory

  .class auto ansi nested family beforefieldinit JSONBaseContext
         extends [mscorlib]System.Object
  {
    .field family class Thrift.Protocol.TJSONProtocol proto
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor(class Thrift.Protocol.TJSONProtocol proto) cil managed
    {
      // Code size       14 (0xe)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ldarg.0
      IL_0007:  ldarg.1
      IL_0008:  stfld      class Thrift.Protocol.TJSONProtocol Thrift.Protocol.TJSONProtocol/JSONBaseContext::proto
      IL_000d:  ret
    } // end of method JSONBaseContext::.ctor

    .method public hidebysig newslot virtual 
            instance void  Write() cil managed
    {
      // Code size       1 (0x1)
      .maxstack  8
      IL_0000:  ret
    } // end of method JSONBaseContext::Write

    .method public hidebysig newslot virtual 
            instance void  Read() cil managed
    {
      // Code size       1 (0x1)
      .maxstack  8
      IL_0000:  ret
    } // end of method JSONBaseContext::Read

    .method public hidebysig newslot virtual 
            instance bool  EscapeNumbers() cil managed
    {
      // Code size       2 (0x2)
      .maxstack  8
      IL_0000:  ldc.i4.0
      IL_0001:  ret
    } // end of method JSONBaseContext::EscapeNumbers

  } // end of class JSONBaseContext

  .class auto ansi nested family beforefieldinit JSONListContext
         extends Thrift.Protocol.TJSONProtocol/JSONBaseContext
  {
    .field private bool first
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor(class Thrift.Protocol.TJSONProtocol protocol) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldc.i4.1
      IL_0002:  stfld      bool Thrift.Protocol.TJSONProtocol/JSONListContext::first
      IL_0007:  ldarg.0
      IL_0008:  ldarg.1
      IL_0009:  call       instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::.ctor(class Thrift.Protocol.TJSONProtocol)
      IL_000e:  ret
    } // end of method JSONListContext::.ctor

    .method public hidebysig virtual instance void 
            Write() cil managed
    {
      // Code size       45 (0x2d)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      bool Thrift.Protocol.TJSONProtocol/JSONListContext::first
      IL_0006:  brfalse    IL_0017

      IL_000b:  ldarg.0
      IL_000c:  ldc.i4.0
      IL_000d:  stfld      bool Thrift.Protocol.TJSONProtocol/JSONListContext::first
      IL_0012:  br         IL_002c

      IL_0017:  ldarg.0
      IL_0018:  ldfld      class Thrift.Protocol.TJSONProtocol Thrift.Protocol.TJSONProtocol/JSONBaseContext::proto
      IL_001d:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
      IL_0022:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::COMMA
      IL_0027:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
      IL_002c:  ret
    } // end of method JSONListContext::Write

    .method public hidebysig virtual instance void 
            Read() cil managed
    {
      // Code size       40 (0x28)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      bool Thrift.Protocol.TJSONProtocol/JSONListContext::first
      IL_0006:  brfalse    IL_0017

      IL_000b:  ldarg.0
      IL_000c:  ldc.i4.0
      IL_000d:  stfld      bool Thrift.Protocol.TJSONProtocol/JSONListContext::first
      IL_0012:  br         IL_0027

      IL_0017:  ldarg.0
      IL_0018:  ldfld      class Thrift.Protocol.TJSONProtocol Thrift.Protocol.TJSONProtocol/JSONBaseContext::proto
      IL_001d:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::COMMA
      IL_0022:  callvirt   instance void Thrift.Protocol.TJSONProtocol::ReadJSONSyntaxChar(uint8[])
      IL_0027:  ret
    } // end of method JSONListContext::Read

  } // end of class JSONListContext

  .class auto ansi nested family beforefieldinit JSONPairContext
         extends Thrift.Protocol.TJSONProtocol/JSONBaseContext
  {
    .field private bool first
    .field private bool colon
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor(class Thrift.Protocol.TJSONProtocol proto) cil managed
    {
      // Code size       22 (0x16)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldc.i4.1
      IL_0002:  stfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::first
      IL_0007:  ldarg.0
      IL_0008:  ldc.i4.1
      IL_0009:  stfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::colon
      IL_000e:  ldarg.0
      IL_000f:  ldarg.1
      IL_0010:  call       instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::.ctor(class Thrift.Protocol.TJSONProtocol)
      IL_0015:  ret
    } // end of method JSONPairContext::.ctor

    .method public hidebysig virtual instance void 
            Write() cil managed
    {
      // Code size       88 (0x58)
      .maxstack  3
      IL_0000:  ldarg.0
      IL_0001:  ldfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::first
      IL_0006:  brfalse    IL_001e

      IL_000b:  ldarg.0
      IL_000c:  ldc.i4.0
      IL_000d:  stfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::first
      IL_0012:  ldarg.0
      IL_0013:  ldc.i4.1
      IL_0014:  stfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::colon
      IL_0019:  br         IL_0057

      IL_001e:  ldarg.0
      IL_001f:  ldfld      class Thrift.Protocol.TJSONProtocol Thrift.Protocol.TJSONProtocol/JSONBaseContext::proto
      IL_0024:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
      IL_0029:  ldarg.0
      IL_002a:  ldfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::colon
      IL_002f:  brfalse    IL_003e

      IL_0034:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::COLON
      IL_0039:  br         IL_0043

      IL_003e:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::COMMA
      IL_0043:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
      IL_0048:  ldarg.0
      IL_0049:  ldarg.0
      IL_004a:  ldfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::colon
      IL_004f:  ldc.i4.0
      IL_0050:  ceq
      IL_0052:  stfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::colon
      IL_0057:  ret
    } // end of method JSONPairContext::Write

    .method public hidebysig virtual instance void 
            Read() cil managed
    {
      // Code size       83 (0x53)
      .maxstack  3
      IL_0000:  ldarg.0
      IL_0001:  ldfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::first
      IL_0006:  brfalse    IL_001e

      IL_000b:  ldarg.0
      IL_000c:  ldc.i4.0
      IL_000d:  stfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::first
      IL_0012:  ldarg.0
      IL_0013:  ldc.i4.1
      IL_0014:  stfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::colon
      IL_0019:  br         IL_0052

      IL_001e:  ldarg.0
      IL_001f:  ldfld      class Thrift.Protocol.TJSONProtocol Thrift.Protocol.TJSONProtocol/JSONBaseContext::proto
      IL_0024:  ldarg.0
      IL_0025:  ldfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::colon
      IL_002a:  brfalse    IL_0039

      IL_002f:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::COLON
      IL_0034:  br         IL_003e

      IL_0039:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::COMMA
      IL_003e:  callvirt   instance void Thrift.Protocol.TJSONProtocol::ReadJSONSyntaxChar(uint8[])
      IL_0043:  ldarg.0
      IL_0044:  ldarg.0
      IL_0045:  ldfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::colon
      IL_004a:  ldc.i4.0
      IL_004b:  ceq
      IL_004d:  stfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::colon
      IL_0052:  ret
    } // end of method JSONPairContext::Read

    .method public hidebysig virtual instance bool 
            EscapeNumbers() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      bool Thrift.Protocol.TJSONProtocol/JSONPairContext::colon
      IL_0006:  ret
    } // end of method JSONPairContext::EscapeNumbers

  } // end of class JSONPairContext

  .class auto ansi nested family beforefieldinit LookaheadReader
         extends [mscorlib]System.Object
  {
    .field family class Thrift.Protocol.TJSONProtocol proto
    .field private bool hasData
    .field private uint8[] data
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor(class Thrift.Protocol.TJSONProtocol proto) cil managed
    {
      // Code size       26 (0x1a)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldc.i4.1
      IL_0002:  newarr     [mscorlib]System.Byte
      IL_0007:  stfld      uint8[] Thrift.Protocol.TJSONProtocol/LookaheadReader::data
      IL_000c:  ldarg.0
      IL_000d:  call       instance void [mscorlib]System.Object::.ctor()
      IL_0012:  ldarg.0
      IL_0013:  ldarg.1
      IL_0014:  stfld      class Thrift.Protocol.TJSONProtocol Thrift.Protocol.TJSONProtocol/LookaheadReader::proto
      IL_0019:  ret
    } // end of method LookaheadReader::.ctor

    .method public hidebysig instance uint8 
            Read() cil managed
    {
      // Code size       57 (0x39)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      bool Thrift.Protocol.TJSONProtocol/LookaheadReader::hasData
      IL_0006:  brfalse    IL_0017

      IL_000b:  ldarg.0
      IL_000c:  ldc.i4.0
      IL_000d:  stfld      bool Thrift.Protocol.TJSONProtocol/LookaheadReader::hasData
      IL_0012:  br         IL_0030

      IL_0017:  ldarg.0
      IL_0018:  ldfld      class Thrift.Protocol.TJSONProtocol Thrift.Protocol.TJSONProtocol/LookaheadReader::proto
      IL_001d:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
      IL_0022:  ldarg.0
      IL_0023:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol/LookaheadReader::data
      IL_0028:  ldc.i4.0
      IL_0029:  ldc.i4.1
      IL_002a:  callvirt   instance int32 Thrift.Transport.TTransport::ReadAll(uint8[],
                                                                               int32,
                                                                               int32)
      IL_002f:  pop
      IL_0030:  ldarg.0
      IL_0031:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol/LookaheadReader::data
      IL_0036:  ldc.i4.0
      IL_0037:  ldelem.u1
      IL_0038:  ret
    } // end of method LookaheadReader::Read

    .method public hidebysig instance uint8 
            Peek() cil managed
    {
      // Code size       52 (0x34)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      bool Thrift.Protocol.TJSONProtocol/LookaheadReader::hasData
      IL_0006:  brtrue     IL_0024

      IL_000b:  ldarg.0
      IL_000c:  ldfld      class Thrift.Protocol.TJSONProtocol Thrift.Protocol.TJSONProtocol/LookaheadReader::proto
      IL_0011:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
      IL_0016:  ldarg.0
      IL_0017:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol/LookaheadReader::data
      IL_001c:  ldc.i4.0
      IL_001d:  ldc.i4.1
      IL_001e:  callvirt   instance int32 Thrift.Transport.TTransport::ReadAll(uint8[],
                                                                               int32,
                                                                               int32)
      IL_0023:  pop
      IL_0024:  ldarg.0
      IL_0025:  ldc.i4.1
      IL_0026:  stfld      bool Thrift.Protocol.TJSONProtocol/LookaheadReader::hasData
      IL_002b:  ldarg.0
      IL_002c:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol/LookaheadReader::data
      IL_0031:  ldc.i4.0
      IL_0032:  ldelem.u1
      IL_0033:  ret
    } // end of method LookaheadReader::Peek

  } // end of class LookaheadReader

  .field private static uint8[] COMMA
  .field private static uint8[] COLON
  .field private static uint8[] LBRACE
  .field private static uint8[] RBRACE
  .field private static uint8[] LBRACKET
  .field private static uint8[] RBRACKET
  .field private static uint8[] QUOTE
  .field private static uint8[] BACKSLASH
  .field private uint8[] ESCSEQ
  .field private static literal int64 VERSION = int64(0x1)
  .field private uint8[] JSON_CHAR_TABLE
  .field private char[] ESCAPE_CHARS
  .field private uint8[] ESCAPE_CHAR_VALS
  .field private static literal int32 DEF_STRING_SIZE = int32(0x00000010)
  .field private static uint8[] NAME_BOOL
  .field private static uint8[] NAME_BYTE
  .field private static uint8[] NAME_I16
  .field private static uint8[] NAME_I32
  .field private static uint8[] NAME_I64
  .field private static uint8[] NAME_DOUBLE
  .field private static uint8[] NAME_STRUCT
  .field private static uint8[] NAME_STRING
  .field private static uint8[] NAME_MAP
  .field private static uint8[] NAME_LIST
  .field private static uint8[] NAME_SET
  .field family class [mscorlib]System.Text.Encoding utf8Encoding
  .field family class [System]System.Collections.Generic.Stack`1<class Thrift.Protocol.TJSONProtocol/JSONBaseContext> contextStack
  .field family class Thrift.Protocol.TJSONProtocol/JSONBaseContext context
  .field family class Thrift.Protocol.TJSONProtocol/LookaheadReader reader
  .field private uint8[] tempBuffer
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.Transport.TTransport trans) cil managed
  {
    // Code size       152 (0x98)
    .maxstack  4
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.4
    IL_0002:  newarr     [mscorlib]System.Byte
    IL_0007:  dup
    IL_0008:  ldtoken    field valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'::'$field-1'
    IL_000d:  call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array,
                                                                                                        valuetype [mscorlib]System.RuntimeFieldHandle)
    IL_0012:  stfld      uint8[] Thrift.Protocol.TJSONProtocol::ESCSEQ
    IL_0017:  ldarg.0
    IL_0018:  ldc.i4.s   48
    IL_001a:  newarr     [mscorlib]System.Byte
    IL_001f:  dup
    IL_0020:  ldtoken    field valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=48' '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'::'$field-2'
    IL_0025:  call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array,
                                                                                                        valuetype [mscorlib]System.RuntimeFieldHandle)
    IL_002a:  stfld      uint8[] Thrift.Protocol.TJSONProtocol::JSON_CHAR_TABLE
    IL_002f:  ldarg.0
    IL_0030:  ldstr      "\"\\/bfnrt"
    IL_0035:  callvirt   instance char[] [mscorlib]System.String::ToCharArray()
    IL_003a:  stfld      char[] Thrift.Protocol.TJSONProtocol::ESCAPE_CHARS
    IL_003f:  ldarg.0
    IL_0040:  ldc.i4.8
    IL_0041:  newarr     [mscorlib]System.Byte
    IL_0046:  dup
    IL_0047:  ldtoken    field valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=8' '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'::'$field-3'
    IL_004c:  call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array,
                                                                                                        valuetype [mscorlib]System.RuntimeFieldHandle)
    IL_0051:  stfld      uint8[] Thrift.Protocol.TJSONProtocol::ESCAPE_CHAR_VALS
    IL_0056:  ldarg.0
    IL_0057:  call       class [mscorlib]System.Text.Encoding [mscorlib]System.Text.Encoding::get_UTF8()
    IL_005c:  stfld      class [mscorlib]System.Text.Encoding Thrift.Protocol.TJSONProtocol::utf8Encoding
    IL_0061:  ldarg.0
    IL_0062:  newobj     instance void class [System]System.Collections.Generic.Stack`1<class Thrift.Protocol.TJSONProtocol/JSONBaseContext>::.ctor()
    IL_0067:  stfld      class [System]System.Collections.Generic.Stack`1<class Thrift.Protocol.TJSONProtocol/JSONBaseContext> Thrift.Protocol.TJSONProtocol::contextStack
    IL_006c:  ldarg.0
    IL_006d:  ldc.i4.4
    IL_006e:  newarr     [mscorlib]System.Byte
    IL_0073:  stfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_0078:  ldarg.0
    IL_0079:  ldarg.1
    IL_007a:  call       instance void Thrift.Protocol.TProtocol::.ctor(class Thrift.Transport.TTransport)
    IL_007f:  ldarg.0
    IL_0080:  ldarg.0
    IL_0081:  newobj     instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::.ctor(class Thrift.Protocol.TJSONProtocol)
    IL_0086:  stfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_008b:  ldarg.0
    IL_008c:  ldarg.0
    IL_008d:  newobj     instance void Thrift.Protocol.TJSONProtocol/LookaheadReader::.ctor(class Thrift.Protocol.TJSONProtocol)
    IL_0092:  stfld      class Thrift.Protocol.TJSONProtocol/LookaheadReader Thrift.Protocol.TJSONProtocol::reader
    IL_0097:  ret
  } // end of method TJSONProtocol::.ctor

  .method private hidebysig static uint8[] 
          GetTypeNameForTypeID(valuetype Thrift.Protocol.TType typeID) cil managed
  {
    // Code size       147 (0x93)
    .maxstack  2
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.2
    IL_0002:  sub
    IL_0003:  switch     ( 
                          IL_0045,
                          IL_004b,
                          IL_0063,
                          IL_0087,
                          IL_0051,
                          IL_0087,
                          IL_0057,
                          IL_0087,
                          IL_005d,
                          IL_0069,
                          IL_006f,
                          IL_0075,
                          IL_007b,
                          IL_0081)
    IL_0040:  br         IL_0087

    IL_0045:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_BOOL
    IL_004a:  ret

    IL_004b:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_BYTE
    IL_0050:  ret

    IL_0051:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_I16
    IL_0056:  ret

    IL_0057:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_I32
    IL_005c:  ret

    IL_005d:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_I64
    IL_0062:  ret

    IL_0063:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_DOUBLE
    IL_0068:  ret

    IL_0069:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_STRING
    IL_006e:  ret

    IL_006f:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_STRUCT
    IL_0074:  ret

    IL_0075:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_MAP
    IL_007a:  ret

    IL_007b:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_SET
    IL_0080:  ret

    IL_0081:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_LIST
    IL_0086:  ret

    IL_0087:  ldc.i4.5
    IL_0088:  ldstr      "Unrecognized type"
    IL_008d:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(int32,
                                                                                 string)
    IL_0092:  throw
  } // end of method TJSONProtocol::GetTypeNameForTypeID

  .method private hidebysig static valuetype Thrift.Protocol.TType 
          GetTypeIDForTypeName(uint8[] name) cil managed
  {
    // Code size       256 (0x100)
    .maxstack  2
    .locals init (valuetype Thrift.Protocol.TType V_0,
             uint8 V_1,
             uint8 V_2)
    IL_0000:  ldc.i4.0
    IL_0001:  stloc.0
    IL_0002:  ldarg.0
    IL_0003:  ldlen
    IL_0004:  conv.i4
    IL_0005:  ldc.i4.1
    IL_0006:  ble        IL_00ec

    IL_000b:  ldarg.0
    IL_000c:  ldc.i4.0
    IL_000d:  ldelem.u1
    IL_000e:  stloc.1
    IL_000f:  ldloc.1
    IL_0010:  ldc.i4.s   105
    IL_0012:  sub
    IL_0013:  switch     ( 
                          IL_0055,
                          IL_002c,
                          IL_002c,
                          IL_00a9,
                          IL_00b1)
    IL_002c:  ldloc.1
    IL_002d:  ldc.i4.s   114
    IL_002f:  sub
    IL_0030:  switch     ( 
                          IL_00b9,
                          IL_00c1,
                          IL_00e5)
    IL_0041:  ldloc.1
    IL_0042:  ldc.i4.s   100
    IL_0044:  beq        IL_004e

    IL_0049:  br         IL_00ec

    IL_004e:  ldc.i4.4
    IL_004f:  stloc.0
    IL_0050:  br         IL_00ec

    IL_0055:  ldarg.0
    IL_0056:  ldc.i4.1
    IL_0057:  ldelem.u1
    IL_0058:  stloc.2
    IL_0059:  ldloc.2
    IL_005a:  ldc.i4.s   49
    IL_005c:  sub
    IL_005d:  switch     ( 
                          IL_008e,
                          IL_00a4,
                          IL_0095,
                          IL_00a4,
                          IL_00a4,
                          IL_009c,
                          IL_00a4,
                          IL_0087)
    IL_0082:  br         IL_00a4

    IL_0087:  ldc.i4.3
    IL_0088:  stloc.0
    IL_0089:  br         IL_00a4

    IL_008e:  ldc.i4.6
    IL_008f:  stloc.0
    IL_0090:  br         IL_00a4

    IL_0095:  ldc.i4.8
    IL_0096:  stloc.0
    IL_0097:  br         IL_00a4

    IL_009c:  ldc.i4.s   10
    IL_009e:  stloc.0
    IL_009f:  br         IL_00a4

    IL_00a4:  br         IL_00ec

    IL_00a9:  ldc.i4.s   15
    IL_00ab:  stloc.0
    IL_00ac:  br         IL_00ec

    IL_00b1:  ldc.i4.s   13
    IL_00b3:  stloc.0
    IL_00b4:  br         IL_00ec

    IL_00b9:  ldc.i4.s   12
    IL_00bb:  stloc.0
    IL_00bc:  br         IL_00ec

    IL_00c1:  ldarg.0
    IL_00c2:  ldc.i4.1
    IL_00c3:  ldelem.u1
    IL_00c4:  ldc.i4.s   116
    IL_00c6:  bne.un     IL_00d3

    IL_00cb:  ldc.i4.s   11
    IL_00cd:  stloc.0
    IL_00ce:  br         IL_00e0

    IL_00d3:  ldarg.0
    IL_00d4:  ldc.i4.1
    IL_00d5:  ldelem.u1
    IL_00d6:  ldc.i4.s   101
    IL_00d8:  bne.un     IL_00e0

    IL_00dd:  ldc.i4.s   14
    IL_00df:  stloc.0
    IL_00e0:  br         IL_00ec

    IL_00e5:  ldc.i4.2
    IL_00e6:  stloc.0
    IL_00e7:  br         IL_00ec

    IL_00ec:  ldloc.0
    IL_00ed:  brtrue     IL_00fe

    IL_00f2:  ldc.i4.5
    IL_00f3:  ldstr      "Unrecognized type"
    IL_00f8:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(int32,
                                                                                 string)
    IL_00fd:  throw

    IL_00fe:  ldloc.0
    IL_00ff:  ret
  } // end of method TJSONProtocol::GetTypeIDForTypeName

  .method family hidebysig instance void 
          PushContext(class Thrift.Protocol.TJSONProtocol/JSONBaseContext c) cil managed
  {
    // Code size       25 (0x19)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Collections.Generic.Stack`1<class Thrift.Protocol.TJSONProtocol/JSONBaseContext> Thrift.Protocol.TJSONProtocol::contextStack
    IL_0006:  ldarg.0
    IL_0007:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_000c:  callvirt   instance void class [System]System.Collections.Generic.Stack`1<class Thrift.Protocol.TJSONProtocol/JSONBaseContext>::Push(!0)
    IL_0011:  ldarg.0
    IL_0012:  ldarg.1
    IL_0013:  stfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0018:  ret
  } // end of method TJSONProtocol::PushContext

  .method family hidebysig instance void 
          PopContext() cil managed
  {
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.0
    IL_0002:  ldfld      class [System]System.Collections.Generic.Stack`1<class Thrift.Protocol.TJSONProtocol/JSONBaseContext> Thrift.Protocol.TJSONProtocol::contextStack
    IL_0007:  callvirt   instance !0 class [System]System.Collections.Generic.Stack`1<class Thrift.Protocol.TJSONProtocol/JSONBaseContext>::Pop()
    IL_000c:  stfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0011:  ret
  } // end of method TJSONProtocol::PopContext

  .method family hidebysig instance void 
          ReadJSONSyntaxChar(uint8[] b) cil managed
  {
    // Code size       46 (0x2e)
    .maxstack  3
    .locals init (uint8 V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TJSONProtocol/LookaheadReader Thrift.Protocol.TJSONProtocol::reader
    IL_0006:  callvirt   instance uint8 Thrift.Protocol.TJSONProtocol/LookaheadReader::Read()
    IL_000b:  stloc.0
    IL_000c:  ldloc.0
    IL_000d:  ldarg.1
    IL_000e:  ldc.i4.0
    IL_000f:  ldelem.u1
    IL_0010:  beq        IL_002d

    IL_0015:  ldc.i4.1
    IL_0016:  ldstr      "Unexpected character:"
    IL_001b:  ldloc.0
    IL_001c:  conv.u2
    IL_001d:  box        [mscorlib]System.Char
    IL_0022:  call       string [mscorlib]System.String::Concat(object,
                                                                object)
    IL_0027:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(int32,
                                                                                 string)
    IL_002c:  throw

    IL_002d:  ret
  } // end of method TJSONProtocol::ReadJSONSyntaxChar

  .method private hidebysig static uint8 
          HexVal(uint8 ch) cil managed
  {
    // Code size       65 (0x41)
    .maxstack  2
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.s   48
    IL_0003:  blt        IL_0017

    IL_0008:  ldarg.0
    IL_0009:  ldc.i4.s   57
    IL_000b:  bgt        IL_0017

    IL_0010:  ldarg.0
    IL_0011:  conv.u2
    IL_0012:  ldc.i4.s   48
    IL_0014:  sub
    IL_0015:  conv.u1
    IL_0016:  ret

    IL_0017:  ldarg.0
    IL_0018:  ldc.i4.s   97
    IL_001a:  blt        IL_0035

    IL_001f:  ldarg.0
    IL_0020:  ldc.i4.s   102
    IL_0022:  bgt        IL_0035

    IL_0027:  ldarg.0
    IL_0028:  ldc.i4.s   10
    IL_002a:  add
    IL_002b:  conv.u1
    IL_002c:  starg.s    ch
    IL_002e:  ldarg.0
    IL_002f:  conv.u2
    IL_0030:  ldc.i4.s   97
    IL_0032:  sub
    IL_0033:  conv.u1
    IL_0034:  ret

    IL_0035:  ldc.i4.1
    IL_0036:  ldstr      "Expected hex character"
    IL_003b:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(int32,
                                                                                 string)
    IL_0040:  throw
  } // end of method TJSONProtocol::HexVal

  .method private hidebysig static uint8 
          HexChar(uint8 val) cil managed
  {
    // Code size       36 (0x24)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.s   15
    IL_0003:  and
    IL_0004:  conv.u1
    IL_0005:  starg.s    val
    IL_0007:  ldarg.0
    IL_0008:  ldc.i4.s   10
    IL_000a:  bge        IL_0016

    IL_000f:  ldarg.0
    IL_0010:  conv.u2
    IL_0011:  ldc.i4.s   48
    IL_0013:  add
    IL_0014:  conv.u1
    IL_0015:  ret

    IL_0016:  ldarg.0
    IL_0017:  ldc.i4.s   10
    IL_0019:  sub
    IL_001a:  conv.u1
    IL_001b:  starg.s    val
    IL_001d:  ldarg.0
    IL_001e:  conv.u2
    IL_001f:  ldc.i4.s   97
    IL_0021:  add
    IL_0022:  conv.u1
    IL_0023:  ret
  } // end of method TJSONProtocol::HexChar

  .method private hidebysig instance void 
          WriteJSONString(uint8[] b) cil managed
  {
    // Code size       329 (0x149)
    .maxstack  5
    .locals init (int32 V_0,
             int32 V_1)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0006:  callvirt   instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::Write()
    IL_000b:  ldarg.0
    IL_000c:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0011:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_0016:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_001b:  ldarg.1
    IL_001c:  ldlen
    IL_001d:  conv.i4
    IL_001e:  stloc.0
    IL_001f:  ldc.i4.0
    IL_0020:  stloc.1
    IL_0021:  br         IL_0131

    IL_0026:  ldarg.1
    IL_0027:  ldloc.1
    IL_0028:  ldelem.u1
    IL_0029:  ldc.i4     0xff
    IL_002e:  and
    IL_002f:  ldc.i4.s   48
    IL_0031:  blt        IL_007d

    IL_0036:  ldarg.1
    IL_0037:  ldloc.1
    IL_0038:  ldelem.u1
    IL_0039:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::BACKSLASH
    IL_003e:  ldc.i4.0
    IL_003f:  ldelem.u1
    IL_0040:  bne.un     IL_006a

    IL_0045:  ldarg.0
    IL_0046:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_004b:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::BACKSLASH
    IL_0050:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_0055:  ldarg.0
    IL_0056:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_005b:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::BACKSLASH
    IL_0060:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_0065:  br         IL_0078

    IL_006a:  ldarg.0
    IL_006b:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0070:  ldarg.1
    IL_0071:  ldloc.1
    IL_0072:  ldc.i4.1
    IL_0073:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_0078:  br         IL_012d

    IL_007d:  ldarg.0
    IL_007e:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_0083:  ldc.i4.0
    IL_0084:  ldarg.0
    IL_0085:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::JSON_CHAR_TABLE
    IL_008a:  ldarg.1
    IL_008b:  ldloc.1
    IL_008c:  ldelem.u1
    IL_008d:  ldelem.u1
    IL_008e:  stelem.i1
    IL_008f:  ldarg.0
    IL_0090:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_0095:  ldc.i4.0
    IL_0096:  ldelem.u1
    IL_0097:  ldc.i4.1
    IL_0098:  bne.un     IL_00b0

    IL_009d:  ldarg.0
    IL_009e:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_00a3:  ldarg.1
    IL_00a4:  ldloc.1
    IL_00a5:  ldc.i4.1
    IL_00a6:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_00ab:  br         IL_012d

    IL_00b0:  ldarg.0
    IL_00b1:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_00b6:  ldc.i4.0
    IL_00b7:  ldelem.u1
    IL_00b8:  ldc.i4.1
    IL_00b9:  ble        IL_00e6

    IL_00be:  ldarg.0
    IL_00bf:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_00c4:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::BACKSLASH
    IL_00c9:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_00ce:  ldarg.0
    IL_00cf:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_00d4:  ldarg.0
    IL_00d5:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_00da:  ldc.i4.0
    IL_00db:  ldc.i4.1
    IL_00dc:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_00e1:  br         IL_012d

    IL_00e6:  ldarg.0
    IL_00e7:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_00ec:  ldarg.0
    IL_00ed:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::ESCSEQ
    IL_00f2:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_00f7:  ldarg.0
    IL_00f8:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_00fd:  ldc.i4.0
    IL_00fe:  ldarg.1
    IL_00ff:  ldloc.1
    IL_0100:  ldelem.u1
    IL_0101:  ldc.i4.4
    IL_0102:  shr
    IL_0103:  conv.u1
    IL_0104:  call       uint8 Thrift.Protocol.TJSONProtocol::HexChar(uint8)
    IL_0109:  stelem.i1
    IL_010a:  ldarg.0
    IL_010b:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_0110:  ldc.i4.1
    IL_0111:  ldarg.1
    IL_0112:  ldloc.1
    IL_0113:  ldelem.u1
    IL_0114:  call       uint8 Thrift.Protocol.TJSONProtocol::HexChar(uint8)
    IL_0119:  stelem.i1
    IL_011a:  ldarg.0
    IL_011b:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0120:  ldarg.0
    IL_0121:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_0126:  ldc.i4.0
    IL_0127:  ldc.i4.2
    IL_0128:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_012d:  ldloc.1
    IL_012e:  ldc.i4.1
    IL_012f:  add
    IL_0130:  stloc.1
    IL_0131:  ldloc.1
    IL_0132:  ldloc.0
    IL_0133:  blt        IL_0026

    IL_0138:  ldarg.0
    IL_0139:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_013e:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_0143:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_0148:  ret
  } // end of method TJSONProtocol::WriteJSONString

  .method private hidebysig instance void 
          WriteJSONInteger(int64 num) cil managed
  {
    // Code size       105 (0x69)
    .maxstack  3
    .locals init (string V_0,
             bool V_1)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0006:  callvirt   instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::Write()
    IL_000b:  ldarga.s   num
    IL_000d:  constrained. [mscorlib]System.Int64
    IL_0013:  callvirt   instance string [mscorlib]System.Object::ToString()
    IL_0018:  stloc.0
    IL_0019:  ldarg.0
    IL_001a:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_001f:  callvirt   instance bool Thrift.Protocol.TJSONProtocol/JSONBaseContext::EscapeNumbers()
    IL_0024:  stloc.1
    IL_0025:  ldloc.1
    IL_0026:  brfalse    IL_003b

    IL_002b:  ldarg.0
    IL_002c:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0031:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_0036:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_003b:  ldarg.0
    IL_003c:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0041:  ldarg.0
    IL_0042:  ldfld      class [mscorlib]System.Text.Encoding Thrift.Protocol.TJSONProtocol::utf8Encoding
    IL_0047:  ldloc.0
    IL_0048:  callvirt   instance uint8[] [mscorlib]System.Text.Encoding::GetBytes(string)
    IL_004d:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_0052:  ldloc.1
    IL_0053:  brfalse    IL_0068

    IL_0058:  ldarg.0
    IL_0059:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_005e:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_0063:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_0068:  ret
  } // end of method TJSONProtocol::WriteJSONInteger

  .method private hidebysig instance void 
          WriteJSONDouble(float64 num) cil managed
  {
    // Code size       180 (0xb4)
    .maxstack  3
    .locals init (string V_0,
             bool V_1,
             char V_2,
             bool V_3)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0006:  callvirt   instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::Write()
    IL_000b:  ldarga.s   num
    IL_000d:  call       class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()
    IL_0012:  call       instance string [mscorlib]System.Double::ToString(class [mscorlib]System.IFormatProvider)
    IL_0017:  stloc.0
    IL_0018:  ldc.i4.0
    IL_0019:  stloc.1
    IL_001a:  ldloc.0
    IL_001b:  ldc.i4.0
    IL_001c:  callvirt   instance char [mscorlib]System.String::get_Chars(int32)
    IL_0021:  stloc.2
    IL_0022:  ldloc.2
    IL_0023:  ldc.i4.s   78
    IL_0025:  beq        IL_003f

    IL_002a:  ldloc.2
    IL_002b:  ldc.i4.s   73
    IL_002d:  beq        IL_003f

    IL_0032:  ldloc.2
    IL_0033:  ldc.i4.s   45
    IL_0035:  beq        IL_0046

    IL_003a:  br         IL_005b

    IL_003f:  ldc.i4.1
    IL_0040:  stloc.1
    IL_0041:  br         IL_005b

    IL_0046:  ldloc.0
    IL_0047:  ldc.i4.1
    IL_0048:  callvirt   instance char [mscorlib]System.String::get_Chars(int32)
    IL_004d:  ldc.i4.s   73
    IL_004f:  bne.un     IL_0056

    IL_0054:  ldc.i4.1
    IL_0055:  stloc.1
    IL_0056:  br         IL_005b

    IL_005b:  ldloc.1
    IL_005c:  brtrue     IL_006e

    IL_0061:  ldarg.0
    IL_0062:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0067:  callvirt   instance bool Thrift.Protocol.TJSONProtocol/JSONBaseContext::EscapeNumbers()
    IL_006c:  br.s       IL_006f

    IL_006e:  ldc.i4.1
    IL_006f:  stloc.3
    IL_0070:  ldloc.3
    IL_0071:  brfalse    IL_0086

    IL_0076:  ldarg.0
    IL_0077:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_007c:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_0081:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_0086:  ldarg.0
    IL_0087:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_008c:  ldarg.0
    IL_008d:  ldfld      class [mscorlib]System.Text.Encoding Thrift.Protocol.TJSONProtocol::utf8Encoding
    IL_0092:  ldloc.0
    IL_0093:  callvirt   instance uint8[] [mscorlib]System.Text.Encoding::GetBytes(string)
    IL_0098:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_009d:  ldloc.3
    IL_009e:  brfalse    IL_00b3

    IL_00a3:  ldarg.0
    IL_00a4:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_00a9:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_00ae:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_00b3:  ret
  } // end of method TJSONProtocol::WriteJSONDouble

  .method private hidebysig instance void 
          WriteJSONBase64(uint8[] b) cil managed
  {
    // Code size       147 (0x93)
    .maxstack  5
    .locals init (int32 V_0,
             int32 V_1)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0006:  callvirt   instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::Write()
    IL_000b:  ldarg.0
    IL_000c:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0011:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_0016:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_001b:  ldarg.1
    IL_001c:  ldlen
    IL_001d:  conv.i4
    IL_001e:  stloc.0
    IL_001f:  ldc.i4.0
    IL_0020:  stloc.1
    IL_0021:  br         IL_0050

    IL_0026:  ldarg.1
    IL_0027:  ldloc.1
    IL_0028:  ldc.i4.3
    IL_0029:  ldarg.0
    IL_002a:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_002f:  ldc.i4.0
    IL_0030:  call       void Thrift.Protocol.TBase64Utils::encode(uint8[],
                                                                   int32,
                                                                   int32,
                                                                   uint8[],
                                                                   int32)
    IL_0035:  ldarg.0
    IL_0036:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_003b:  ldarg.0
    IL_003c:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_0041:  ldc.i4.0
    IL_0042:  ldc.i4.4
    IL_0043:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_0048:  ldloc.1
    IL_0049:  ldc.i4.3
    IL_004a:  add
    IL_004b:  stloc.1
    IL_004c:  ldloc.0
    IL_004d:  ldc.i4.3
    IL_004e:  sub
    IL_004f:  stloc.0
    IL_0050:  ldloc.0
    IL_0051:  ldc.i4.3
    IL_0052:  bge        IL_0026

    IL_0057:  ldloc.0
    IL_0058:  ldc.i4.0
    IL_0059:  ble        IL_0082

    IL_005e:  ldarg.1
    IL_005f:  ldloc.1
    IL_0060:  ldloc.0
    IL_0061:  ldarg.0
    IL_0062:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_0067:  ldc.i4.0
    IL_0068:  call       void Thrift.Protocol.TBase64Utils::encode(uint8[],
                                                                   int32,
                                                                   int32,
                                                                   uint8[],
                                                                   int32)
    IL_006d:  ldarg.0
    IL_006e:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0073:  ldarg.0
    IL_0074:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_0079:  ldc.i4.0
    IL_007a:  ldloc.0
    IL_007b:  ldc.i4.1
    IL_007c:  add
    IL_007d:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_0082:  ldarg.0
    IL_0083:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0088:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_008d:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_0092:  ret
  } // end of method TJSONProtocol::WriteJSONBase64

  .method private hidebysig instance void 
          WriteJSONObjectStart() cil managed
  {
    // Code size       40 (0x28)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0006:  callvirt   instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::Write()
    IL_000b:  ldarg.0
    IL_000c:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0011:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::LBRACE
    IL_0016:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_001b:  ldarg.0
    IL_001c:  ldarg.0
    IL_001d:  newobj     instance void Thrift.Protocol.TJSONProtocol/JSONPairContext::.ctor(class Thrift.Protocol.TJSONProtocol)
    IL_0022:  call       instance void Thrift.Protocol.TJSONProtocol::PushContext(class Thrift.Protocol.TJSONProtocol/JSONBaseContext)
    IL_0027:  ret
  } // end of method TJSONProtocol::WriteJSONObjectStart

  .method private hidebysig instance void 
          WriteJSONObjectEnd() cil managed
  {
    // Code size       23 (0x17)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::PopContext()
    IL_0006:  ldarg.0
    IL_0007:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_000c:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::RBRACE
    IL_0011:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_0016:  ret
  } // end of method TJSONProtocol::WriteJSONObjectEnd

  .method private hidebysig instance void 
          WriteJSONArrayStart() cil managed
  {
    // Code size       40 (0x28)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0006:  callvirt   instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::Write()
    IL_000b:  ldarg.0
    IL_000c:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0011:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::LBRACKET
    IL_0016:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_001b:  ldarg.0
    IL_001c:  ldarg.0
    IL_001d:  newobj     instance void Thrift.Protocol.TJSONProtocol/JSONListContext::.ctor(class Thrift.Protocol.TJSONProtocol)
    IL_0022:  call       instance void Thrift.Protocol.TJSONProtocol::PushContext(class Thrift.Protocol.TJSONProtocol/JSONBaseContext)
    IL_0027:  ret
  } // end of method TJSONProtocol::WriteJSONArrayStart

  .method private hidebysig instance void 
          WriteJSONArrayEnd() cil managed
  {
    // Code size       23 (0x17)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::PopContext()
    IL_0006:  ldarg.0
    IL_0007:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_000c:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::RBRACKET
    IL_0011:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[])
    IL_0016:  ret
  } // end of method TJSONProtocol::WriteJSONArrayEnd

  .method public hidebysig virtual instance void 
          WriteMessageBegin(valuetype Thrift.Protocol.TMessage message) cil managed
  {
    // Code size       69 (0x45)
    .maxstack  2
    .locals init (uint8[] V_0)
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONArrayStart()
    IL_0006:  ldarg.0
    IL_0007:  ldc.i4.1
    IL_0008:  conv.i8
    IL_0009:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONInteger(int64)
    IL_000e:  ldarg.0
    IL_000f:  ldfld      class [mscorlib]System.Text.Encoding Thrift.Protocol.TJSONProtocol::utf8Encoding
    IL_0014:  ldarga.s   message
    IL_0016:  call       instance string Thrift.Protocol.TMessage::get_Name()
    IL_001b:  callvirt   instance uint8[] [mscorlib]System.Text.Encoding::GetBytes(string)
    IL_0020:  stloc.0
    IL_0021:  ldarg.0
    IL_0022:  ldloc.0
    IL_0023:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONString(uint8[])
    IL_0028:  ldarg.0
    IL_0029:  ldarga.s   message
    IL_002b:  call       instance valuetype Thrift.Protocol.TMessageType Thrift.Protocol.TMessage::get_Type()
    IL_0030:  conv.i8
    IL_0031:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONInteger(int64)
    IL_0036:  ldarg.0
    IL_0037:  ldarga.s   message
    IL_0039:  call       instance int32 Thrift.Protocol.TMessage::get_SeqID()
    IL_003e:  conv.i8
    IL_003f:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONInteger(int64)
    IL_0044:  ret
  } // end of method TJSONProtocol::WriteMessageBegin

  .method public hidebysig virtual instance void 
          WriteMessageEnd() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONArrayEnd()
    IL_0006:  ret
  } // end of method TJSONProtocol::WriteMessageEnd

  .method public hidebysig virtual instance void 
          WriteStructBegin(valuetype Thrift.Protocol.TStruct str) cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONObjectStart()
    IL_0006:  ret
  } // end of method TJSONProtocol::WriteStructBegin

  .method public hidebysig virtual instance void 
          WriteStructEnd() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONObjectEnd()
    IL_0006:  ret
  } // end of method TJSONProtocol::WriteStructEnd

  .method public hidebysig virtual instance void 
          WriteFieldBegin(valuetype Thrift.Protocol.TField 'field') cil managed
  {
    // Code size       39 (0x27)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarga.s   'field'
    IL_0003:  call       instance int16 Thrift.Protocol.TField::get_ID()
    IL_0008:  conv.i8
    IL_0009:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONInteger(int64)
    IL_000e:  ldarg.0
    IL_000f:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONObjectStart()
    IL_0014:  ldarg.0
    IL_0015:  ldarga.s   'field'
    IL_0017:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
    IL_001c:  call       uint8[] Thrift.Protocol.TJSONProtocol::GetTypeNameForTypeID(valuetype Thrift.Protocol.TType)
    IL_0021:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONString(uint8[])
    IL_0026:  ret
  } // end of method TJSONProtocol::WriteFieldBegin

  .method public hidebysig virtual instance void 
          WriteFieldEnd() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONObjectEnd()
    IL_0006:  ret
  } // end of method TJSONProtocol::WriteFieldEnd

  .method public hidebysig virtual instance void 
          WriteFieldStop() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TJSONProtocol::WriteFieldStop

  .method public hidebysig virtual instance void 
          WriteMapBegin(valuetype Thrift.Protocol.TMap map) cil managed
  {
    // Code size       63 (0x3f)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONArrayStart()
    IL_0006:  ldarg.0
    IL_0007:  ldarga.s   map
    IL_0009:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::get_KeyType()
    IL_000e:  call       uint8[] Thrift.Protocol.TJSONProtocol::GetTypeNameForTypeID(valuetype Thrift.Protocol.TType)
    IL_0013:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONString(uint8[])
    IL_0018:  ldarg.0
    IL_0019:  ldarga.s   map
    IL_001b:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::get_ValueType()
    IL_0020:  call       uint8[] Thrift.Protocol.TJSONProtocol::GetTypeNameForTypeID(valuetype Thrift.Protocol.TType)
    IL_0025:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONString(uint8[])
    IL_002a:  ldarg.0
    IL_002b:  ldarga.s   map
    IL_002d:  call       instance int32 Thrift.Protocol.TMap::get_Count()
    IL_0032:  conv.i8
    IL_0033:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONInteger(int64)
    IL_0038:  ldarg.0
    IL_0039:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONObjectStart()
    IL_003e:  ret
  } // end of method TJSONProtocol::WriteMapBegin

  .method public hidebysig virtual instance void 
          WriteMapEnd() cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONObjectEnd()
    IL_0006:  ldarg.0
    IL_0007:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONArrayEnd()
    IL_000c:  ret
  } // end of method TJSONProtocol::WriteMapEnd

  .method public hidebysig virtual instance void 
          WriteListBegin(valuetype Thrift.Protocol.TList list) cil managed
  {
    // Code size       39 (0x27)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONArrayStart()
    IL_0006:  ldarg.0
    IL_0007:  ldarga.s   list
    IL_0009:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TList::get_ElementType()
    IL_000e:  call       uint8[] Thrift.Protocol.TJSONProtocol::GetTypeNameForTypeID(valuetype Thrift.Protocol.TType)
    IL_0013:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONString(uint8[])
    IL_0018:  ldarg.0
    IL_0019:  ldarga.s   list
    IL_001b:  call       instance int32 Thrift.Protocol.TList::get_Count()
    IL_0020:  conv.i8
    IL_0021:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONInteger(int64)
    IL_0026:  ret
  } // end of method TJSONProtocol::WriteListBegin

  .method public hidebysig virtual instance void 
          WriteListEnd() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONArrayEnd()
    IL_0006:  ret
  } // end of method TJSONProtocol::WriteListEnd

  .method public hidebysig virtual instance void 
          WriteSetBegin(valuetype Thrift.Protocol.TSet set) cil managed
  {
    // Code size       39 (0x27)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONArrayStart()
    IL_0006:  ldarg.0
    IL_0007:  ldarga.s   set
    IL_0009:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TSet::get_ElementType()
    IL_000e:  call       uint8[] Thrift.Protocol.TJSONProtocol::GetTypeNameForTypeID(valuetype Thrift.Protocol.TType)
    IL_0013:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONString(uint8[])
    IL_0018:  ldarg.0
    IL_0019:  ldarga.s   set
    IL_001b:  call       instance int32 Thrift.Protocol.TSet::get_Count()
    IL_0020:  conv.i8
    IL_0021:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONInteger(int64)
    IL_0026:  ret
  } // end of method TJSONProtocol::WriteSetBegin

  .method public hidebysig virtual instance void 
          WriteSetEnd() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONArrayEnd()
    IL_0006:  ret
  } // end of method TJSONProtocol::WriteSetEnd

  .method public hidebysig virtual instance void 
          WriteBool(bool b) cil managed
  {
    // Code size       22 (0x16)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  brfalse    IL_000e

    IL_0007:  ldc.i4.1
    IL_0008:  conv.i8
    IL_0009:  br         IL_0010

    IL_000e:  ldc.i4.0
    IL_000f:  conv.i8
    IL_0010:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONInteger(int64)
    IL_0015:  ret
  } // end of method TJSONProtocol::WriteBool

  .method public hidebysig virtual instance void 
          WriteByte(int8 b) cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  conv.i8
    IL_0003:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONInteger(int64)
    IL_0008:  ret
  } // end of method TJSONProtocol::WriteByte

  .method public hidebysig virtual instance void 
          WriteI16(int16 i16) cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  conv.i8
    IL_0003:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONInteger(int64)
    IL_0008:  ret
  } // end of method TJSONProtocol::WriteI16

  .method public hidebysig virtual instance void 
          WriteI32(int32 i32) cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  conv.i8
    IL_0003:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONInteger(int64)
    IL_0008:  ret
  } // end of method TJSONProtocol::WriteI32

  .method public hidebysig virtual instance void 
          WriteI64(int64 i64) cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONInteger(int64)
    IL_0007:  ret
  } // end of method TJSONProtocol::WriteI64

  .method public hidebysig virtual instance void 
          WriteDouble(float64 dub) cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONDouble(float64)
    IL_0007:  ret
  } // end of method TJSONProtocol::WriteDouble

  .method public hidebysig virtual instance void 
          WriteString(string str) cil managed
  {
    // Code size       21 (0x15)
    .maxstack  2
    .locals init (uint8[] V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.Text.Encoding Thrift.Protocol.TJSONProtocol::utf8Encoding
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance uint8[] [mscorlib]System.Text.Encoding::GetBytes(string)
    IL_000c:  stloc.0
    IL_000d:  ldarg.0
    IL_000e:  ldloc.0
    IL_000f:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONString(uint8[])
    IL_0014:  ret
  } // end of method TJSONProtocol::WriteString

  .method public hidebysig virtual instance void 
          WriteBinary(uint8[] bin) cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  call       instance void Thrift.Protocol.TJSONProtocol::WriteJSONBase64(uint8[])
    IL_0007:  ret
  } // end of method TJSONProtocol::WriteBinary

  .method private hidebysig instance uint8[] 
          ReadJSONString(bool skipContext) cil managed
  {
    // Code size       325 (0x145)
    .maxstack  5
    .locals init (class [mscorlib]System.IO.MemoryStream V_0,
             uint8 V_1,
             int32 V_2,
             int16 V_3,
             uint8[] V_4)
    IL_0000:  newobj     instance void [mscorlib]System.IO.MemoryStream::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldarg.1
    IL_0007:  brtrue     IL_0017

    IL_000c:  ldarg.0
    IL_000d:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0012:  callvirt   instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::Read()
    IL_0017:  ldarg.0
    IL_0018:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_001d:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONSyntaxChar(uint8[])
    IL_0022:  ldarg.0
    IL_0023:  ldfld      class Thrift.Protocol.TJSONProtocol/LookaheadReader Thrift.Protocol.TJSONProtocol::reader
    IL_0028:  callvirt   instance uint8 Thrift.Protocol.TJSONProtocol/LookaheadReader::Read()
    IL_002d:  stloc.1
    IL_002e:  ldloc.1
    IL_002f:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_0034:  ldc.i4.0
    IL_0035:  ldelem.u1
    IL_0036:  bne.un     IL_0040

    IL_003b:  br         IL_013e

    IL_0040:  ldloc.1
    IL_0041:  ldarg.0
    IL_0042:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::ESCSEQ
    IL_0047:  ldc.i4.0
    IL_0048:  ldelem.u1
    IL_0049:  beq        IL_0065

    IL_004e:  ldloc.0
    IL_004f:  ldc.i4.1
    IL_0050:  newarr     [mscorlib]System.Byte
    IL_0055:  dup
    IL_0056:  ldc.i4.0
    IL_0057:  ldloc.1
    IL_0058:  stelem.i1
    IL_0059:  ldc.i4.0
    IL_005a:  ldc.i4.1
    IL_005b:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                         int32,
                                                                         int32)
    IL_0060:  br         IL_0022

    IL_0065:  ldarg.0
    IL_0066:  ldfld      class Thrift.Protocol.TJSONProtocol/LookaheadReader Thrift.Protocol.TJSONProtocol::reader
    IL_006b:  callvirt   instance uint8 Thrift.Protocol.TJSONProtocol/LookaheadReader::Read()
    IL_0070:  stloc.1
    IL_0071:  ldloc.1
    IL_0072:  ldarg.0
    IL_0073:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::ESCSEQ
    IL_0078:  ldc.i4.1
    IL_0079:  ldelem.u1
    IL_007a:  beq        IL_00c0

    IL_007f:  ldarg.0
    IL_0080:  ldfld      char[] Thrift.Protocol.TJSONProtocol::ESCAPE_CHARS
    IL_0085:  ldloc.1
    IL_0086:  conv.u2
    IL_0087:  call       int32 [mscorlib]System.Array::IndexOf<char>(!!0[],
                                                                     !!0)
    IL_008c:  stloc.2
    IL_008d:  ldloc.2
    IL_008e:  ldc.i4.m1
    IL_008f:  bne.un     IL_00a0

    IL_0094:  ldc.i4.1
    IL_0095:  ldstr      "Expected control char"
    IL_009a:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(int32,
                                                                                 string)
    IL_009f:  throw

    IL_00a0:  ldarg.0
    IL_00a1:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::ESCAPE_CHAR_VALS
    IL_00a6:  ldloc.2
    IL_00a7:  ldelem.u1
    IL_00a8:  stloc.1
    IL_00a9:  ldloc.0
    IL_00aa:  ldc.i4.1
    IL_00ab:  newarr     [mscorlib]System.Byte
    IL_00b0:  dup
    IL_00b1:  ldc.i4.0
    IL_00b2:  ldloc.1
    IL_00b3:  stelem.i1
    IL_00b4:  ldc.i4.0
    IL_00b5:  ldc.i4.1
    IL_00b6:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                         int32,
                                                                         int32)
    IL_00bb:  br         IL_0022

    IL_00c0:  ldarg.0
    IL_00c1:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_00c6:  ldarg.0
    IL_00c7:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_00cc:  ldc.i4.0
    IL_00cd:  ldc.i4.4
    IL_00ce:  callvirt   instance int32 Thrift.Transport.TTransport::ReadAll(uint8[],
                                                                             int32,
                                                                             int32)
    IL_00d3:  pop
    IL_00d4:  ldarg.0
    IL_00d5:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_00da:  ldc.i4.0
    IL_00db:  ldelem.u1
    IL_00dc:  call       uint8 Thrift.Protocol.TJSONProtocol::HexVal(uint8)
    IL_00e1:  ldc.i4.s   12
    IL_00e3:  shl
    IL_00e4:  ldarg.0
    IL_00e5:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_00ea:  ldc.i4.1
    IL_00eb:  ldelem.u1
    IL_00ec:  call       uint8 Thrift.Protocol.TJSONProtocol::HexVal(uint8)
    IL_00f1:  ldc.i4.8
    IL_00f2:  shl
    IL_00f3:  add
    IL_00f4:  ldarg.0
    IL_00f5:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_00fa:  ldc.i4.2
    IL_00fb:  ldelem.u1
    IL_00fc:  call       uint8 Thrift.Protocol.TJSONProtocol::HexVal(uint8)
    IL_0101:  ldc.i4.4
    IL_0102:  shl
    IL_0103:  add
    IL_0104:  ldarg.0
    IL_0105:  ldfld      uint8[] Thrift.Protocol.TJSONProtocol::tempBuffer
    IL_010a:  ldc.i4.3
    IL_010b:  ldelem.u1
    IL_010c:  call       uint8 Thrift.Protocol.TJSONProtocol::HexVal(uint8)
    IL_0111:  add
    IL_0112:  conv.i2
    IL_0113:  stloc.3
    IL_0114:  ldarg.0
    IL_0115:  ldfld      class [mscorlib]System.Text.Encoding Thrift.Protocol.TJSONProtocol::utf8Encoding
    IL_011a:  ldc.i4.1
    IL_011b:  newarr     [mscorlib]System.Char
    IL_0120:  dup
    IL_0121:  ldc.i4.0
    IL_0122:  ldloc.3
    IL_0123:  conv.u2
    IL_0124:  stelem.i2
    IL_0125:  callvirt   instance uint8[] [mscorlib]System.Text.Encoding::GetBytes(char[])
    IL_012a:  stloc.s    V_4
    IL_012c:  ldloc.0
    IL_012d:  ldloc.s    V_4
    IL_012f:  ldc.i4.0
    IL_0130:  ldloc.s    V_4
    IL_0132:  ldlen
    IL_0133:  conv.i4
    IL_0134:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                         int32,
                                                                         int32)
    IL_0139:  br         IL_0022

    IL_013e:  ldloc.0
    IL_013f:  callvirt   instance uint8[] [mscorlib]System.IO.MemoryStream::ToArray()
    IL_0144:  ret
  } // end of method TJSONProtocol::ReadJSONString

  .method private hidebysig instance bool 
          IsJSONNumeric(uint8 b) cil managed
  {
    // Code size       134 (0x86)
    .maxstack  2
    IL_0000:  ldarg.1
    IL_0001:  ldc.i4.s   43
    IL_0003:  sub
    IL_0004:  switch     ( 
                          IL_0082,
                          IL_0075,
                          IL_0082,
                          IL_0082,
                          IL_0075,
                          IL_0082,
                          IL_0082,
                          IL_0082,
                          IL_0082,
                          IL_0082,
                          IL_0082,
                          IL_0082,
                          IL_0082,
                          IL_0082,
                          IL_0082,
                          IL_0075,
                          IL_0075,
                          IL_0075,
                          IL_0075,
                          IL_0075,
                          IL_0075,
                          IL_0075,
                          IL_0075,
                          IL_0075,
                          IL_0075,
                          IL_0075,
                          IL_0082)
    IL_0075:  ldarg.1
    IL_0076:  ldc.i4.s   101
    IL_0078:  beq        IL_0082

    IL_007d:  br         IL_0084

    IL_0082:  ldc.i4.1
    IL_0083:  ret

    IL_0084:  ldc.i4.0
    IL_0085:  ret
  } // end of method TJSONProtocol::IsJSONNumeric

  .method private hidebysig instance string 
          ReadJSONNumericChars() cil managed
  {
    // Code size       66 (0x42)
    .maxstack  2
    .locals init (class [mscorlib]System.Text.StringBuilder V_0,
             uint8 V_1)
    IL_0000:  newobj     instance void [mscorlib]System.Text.StringBuilder::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldarg.0
    IL_0007:  ldfld      class Thrift.Protocol.TJSONProtocol/LookaheadReader Thrift.Protocol.TJSONProtocol::reader
    IL_000c:  callvirt   instance uint8 Thrift.Protocol.TJSONProtocol/LookaheadReader::Peek()
    IL_0011:  stloc.1
    IL_0012:  ldarg.0
    IL_0013:  ldloc.1
    IL_0014:  call       instance bool Thrift.Protocol.TJSONProtocol::IsJSONNumeric(uint8)
    IL_0019:  brtrue     IL_0023

    IL_001e:  br         IL_003b

    IL_0023:  ldloc.0
    IL_0024:  ldarg.0
    IL_0025:  ldfld      class Thrift.Protocol.TJSONProtocol/LookaheadReader Thrift.Protocol.TJSONProtocol::reader
    IL_002a:  callvirt   instance uint8 Thrift.Protocol.TJSONProtocol/LookaheadReader::Read()
    IL_002f:  conv.u2
    IL_0030:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(char)
    IL_0035:  pop
    IL_0036:  br         IL_0006

    IL_003b:  ldloc.0
    IL_003c:  callvirt   instance string [mscorlib]System.Object::ToString()
    IL_0041:  ret
  } // end of method TJSONProtocol::ReadJSONNumericChars

  .method private hidebysig instance int64 
          ReadJSONInteger() cil managed
  {
    // Code size       99 (0x63)
    .maxstack  2
    .locals init (string V_0,
             int64 V_1)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0006:  callvirt   instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::Read()
    IL_000b:  ldarg.0
    IL_000c:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0011:  callvirt   instance bool Thrift.Protocol.TJSONProtocol/JSONBaseContext::EscapeNumbers()
    IL_0016:  brfalse    IL_0026

    IL_001b:  ldarg.0
    IL_001c:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_0021:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONSyntaxChar(uint8[])
    IL_0026:  ldarg.0
    IL_0027:  call       instance string Thrift.Protocol.TJSONProtocol::ReadJSONNumericChars()
    IL_002c:  stloc.0
    IL_002d:  ldarg.0
    IL_002e:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0033:  callvirt   instance bool Thrift.Protocol.TJSONProtocol/JSONBaseContext::EscapeNumbers()
    IL_0038:  brfalse    IL_0048

    IL_003d:  ldarg.0
    IL_003e:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_0043:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONSyntaxChar(uint8[])
    .try
    {
      IL_0048:  ldloc.0
      IL_0049:  call       int64 [mscorlib]System.Int64::Parse(string)
      IL_004e:  stloc.1
      IL_004f:  leave      IL_0061

    }  // end .try
    catch [mscorlib]System.FormatException 
    {
      IL_0054:  pop
      IL_0055:  ldc.i4.1
      IL_0056:  ldstr      "Bad data encounted in numeric data"
      IL_005b:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(int32,
                                                                                   string)
      IL_0060:  throw

    }  // end handler
    IL_0061:  ldloc.1
    IL_0062:  ret
  } // end of method TJSONProtocol::ReadJSONInteger

  .method private hidebysig instance float64 
          ReadJSONDouble() cil managed
  {
    // Code size       185 (0xb9)
    .maxstack  4
    .locals init (uint8[] V_0,
             float64 V_1,
             float64 V_2)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0006:  callvirt   instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::Read()
    IL_000b:  ldarg.0
    IL_000c:  ldfld      class Thrift.Protocol.TJSONProtocol/LookaheadReader Thrift.Protocol.TJSONProtocol::reader
    IL_0011:  callvirt   instance uint8 Thrift.Protocol.TJSONProtocol/LookaheadReader::Peek()
    IL_0016:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_001b:  ldc.i4.0
    IL_001c:  ldelem.u1
    IL_001d:  bne.un     IL_0079

    IL_0022:  ldarg.0
    IL_0023:  ldc.i4.1
    IL_0024:  call       instance uint8[] Thrift.Protocol.TJSONProtocol::ReadJSONString(bool)
    IL_0029:  stloc.0
    IL_002a:  ldarg.0
    IL_002b:  ldfld      class [mscorlib]System.Text.Encoding Thrift.Protocol.TJSONProtocol::utf8Encoding
    IL_0030:  ldloc.0
    IL_0031:  ldc.i4.0
    IL_0032:  ldloc.0
    IL_0033:  ldlen
    IL_0034:  conv.i4
    IL_0035:  callvirt   instance string [mscorlib]System.Text.Encoding::GetString(uint8[],
                                                                                   int32,
                                                                                   int32)
    IL_003a:  call       class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()
    IL_003f:  call       float64 [mscorlib]System.Double::Parse(string,
                                                                class [mscorlib]System.IFormatProvider)
    IL_0044:  stloc.1
    IL_0045:  ldarg.0
    IL_0046:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_004b:  callvirt   instance bool Thrift.Protocol.TJSONProtocol/JSONBaseContext::EscapeNumbers()
    IL_0050:  brtrue     IL_0077

    IL_0055:  ldloc.1
    IL_0056:  call       bool [mscorlib]System.Double::IsNaN(float64)
    IL_005b:  brtrue     IL_0077

    IL_0060:  ldloc.1
    IL_0061:  call       bool [mscorlib]System.Double::IsInfinity(float64)
    IL_0066:  brtrue     IL_0077

    IL_006b:  ldc.i4.1
    IL_006c:  ldstr      "Numeric data unexpectedly quoted"
    IL_0071:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(int32,
                                                                                 string)
    IL_0076:  throw

    IL_0077:  ldloc.1
    IL_0078:  ret

    IL_0079:  ldarg.0
    IL_007a:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_007f:  callvirt   instance bool Thrift.Protocol.TJSONProtocol/JSONBaseContext::EscapeNumbers()
    IL_0084:  brfalse    IL_0094

    IL_0089:  ldarg.0
    IL_008a:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_008f:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONSyntaxChar(uint8[])
    .try
    {
      IL_0094:  ldarg.0
      IL_0095:  call       instance string Thrift.Protocol.TJSONProtocol::ReadJSONNumericChars()
      IL_009a:  call       class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()
      IL_009f:  call       float64 [mscorlib]System.Double::Parse(string,
                                                                  class [mscorlib]System.IFormatProvider)
      IL_00a4:  stloc.2
      IL_00a5:  leave      IL_00b7

    }  // end .try
    catch [mscorlib]System.FormatException 
    {
      IL_00aa:  pop
      IL_00ab:  ldc.i4.1
      IL_00ac:  ldstr      "Bad data encounted in numeric data"
      IL_00b1:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(int32,
                                                                                   string)
      IL_00b6:  throw

    }  // end handler
    IL_00b7:  ldloc.2
    IL_00b8:  ret
  } // end of method TJSONProtocol::ReadJSONDouble

  .method private hidebysig instance uint8[] 
          ReadJSONBase64() cil managed
  {
    // Code size       123 (0x7b)
    .maxstack  5
    .locals init (uint8[] V_0,
             int32 V_1,
             int32 V_2,
             int32 V_3,
             uint8[] V_4)
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.0
    IL_0002:  call       instance uint8[] Thrift.Protocol.TJSONProtocol::ReadJSONString(bool)
    IL_0007:  stloc.0
    IL_0008:  ldloc.0
    IL_0009:  ldlen
    IL_000a:  conv.i4
    IL_000b:  stloc.1
    IL_000c:  ldc.i4.0
    IL_000d:  stloc.2
    IL_000e:  ldc.i4.0
    IL_000f:  stloc.3
    IL_0010:  br         IL_0019

    IL_0015:  ldloc.1
    IL_0016:  ldc.i4.1
    IL_0017:  sub
    IL_0018:  stloc.1
    IL_0019:  ldloc.1
    IL_001a:  ldc.i4.0
    IL_001b:  ble        IL_002c

    IL_0020:  ldloc.0
    IL_0021:  ldloc.1
    IL_0022:  ldc.i4.1
    IL_0023:  sub
    IL_0024:  ldelem.u1
    IL_0025:  ldc.i4.s   61
    IL_0027:  beq        IL_0015

    IL_002c:  br         IL_0047

    IL_0031:  ldloc.0
    IL_0032:  ldloc.2
    IL_0033:  ldc.i4.4
    IL_0034:  ldloc.0
    IL_0035:  ldloc.3
    IL_0036:  call       void Thrift.Protocol.TBase64Utils::decode(uint8[],
                                                                   int32,
                                                                   int32,
                                                                   uint8[],
                                                                   int32)
    IL_003b:  ldloc.2
    IL_003c:  ldc.i4.4
    IL_003d:  add
    IL_003e:  stloc.2
    IL_003f:  ldloc.1
    IL_0040:  ldc.i4.4
    IL_0041:  sub
    IL_0042:  stloc.1
    IL_0043:  ldloc.3
    IL_0044:  ldc.i4.3
    IL_0045:  add
    IL_0046:  stloc.3
    IL_0047:  ldloc.1
    IL_0048:  ldc.i4.4
    IL_0049:  bgt        IL_0031

    IL_004e:  ldloc.1
    IL_004f:  ldc.i4.1
    IL_0050:  ble        IL_0065

    IL_0055:  ldloc.0
    IL_0056:  ldloc.2
    IL_0057:  ldloc.1
    IL_0058:  ldloc.0
    IL_0059:  ldloc.3
    IL_005a:  call       void Thrift.Protocol.TBase64Utils::decode(uint8[],
                                                                   int32,
                                                                   int32,
                                                                   uint8[],
                                                                   int32)
    IL_005f:  ldloc.3
    IL_0060:  ldloc.1
    IL_0061:  ldc.i4.1
    IL_0062:  sub
    IL_0063:  add
    IL_0064:  stloc.3
    IL_0065:  ldloc.3
    IL_0066:  newarr     [mscorlib]System.Byte
    IL_006b:  stloc.s    V_4
    IL_006d:  ldloc.0
    IL_006e:  ldc.i4.0
    IL_006f:  ldloc.s    V_4
    IL_0071:  ldc.i4.0
    IL_0072:  ldloc.3
    IL_0073:  call       void [mscorlib]System.Array::Copy(class [mscorlib]System.Array,
                                                           int32,
                                                           class [mscorlib]System.Array,
                                                           int32,
                                                           int32)
    IL_0078:  ldloc.s    V_4
    IL_007a:  ret
  } // end of method TJSONProtocol::ReadJSONBase64

  .method private hidebysig instance void 
          ReadJSONObjectStart() cil managed
  {
    // Code size       35 (0x23)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0006:  callvirt   instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::Read()
    IL_000b:  ldarg.0
    IL_000c:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::LBRACE
    IL_0011:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONSyntaxChar(uint8[])
    IL_0016:  ldarg.0
    IL_0017:  ldarg.0
    IL_0018:  newobj     instance void Thrift.Protocol.TJSONProtocol/JSONPairContext::.ctor(class Thrift.Protocol.TJSONProtocol)
    IL_001d:  call       instance void Thrift.Protocol.TJSONProtocol::PushContext(class Thrift.Protocol.TJSONProtocol/JSONBaseContext)
    IL_0022:  ret
  } // end of method TJSONProtocol::ReadJSONObjectStart

  .method private hidebysig instance void 
          ReadJSONObjectEnd() cil managed
  {
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::RBRACE
    IL_0006:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONSyntaxChar(uint8[])
    IL_000b:  ldarg.0
    IL_000c:  call       instance void Thrift.Protocol.TJSONProtocol::PopContext()
    IL_0011:  ret
  } // end of method TJSONProtocol::ReadJSONObjectEnd

  .method private hidebysig instance void 
          ReadJSONArrayStart() cil managed
  {
    // Code size       35 (0x23)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TJSONProtocol/JSONBaseContext Thrift.Protocol.TJSONProtocol::context
    IL_0006:  callvirt   instance void Thrift.Protocol.TJSONProtocol/JSONBaseContext::Read()
    IL_000b:  ldarg.0
    IL_000c:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::LBRACKET
    IL_0011:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONSyntaxChar(uint8[])
    IL_0016:  ldarg.0
    IL_0017:  ldarg.0
    IL_0018:  newobj     instance void Thrift.Protocol.TJSONProtocol/JSONListContext::.ctor(class Thrift.Protocol.TJSONProtocol)
    IL_001d:  call       instance void Thrift.Protocol.TJSONProtocol::PushContext(class Thrift.Protocol.TJSONProtocol/JSONBaseContext)
    IL_0022:  ret
  } // end of method TJSONProtocol::ReadJSONArrayStart

  .method private hidebysig instance void 
          ReadJSONArrayEnd() cil managed
  {
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::RBRACKET
    IL_0006:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONSyntaxChar(uint8[])
    IL_000b:  ldarg.0
    IL_000c:  call       instance void Thrift.Protocol.TJSONProtocol::PopContext()
    IL_0011:  ret
  } // end of method TJSONProtocol::ReadJSONArrayEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TMessage 
          ReadMessageBegin() cil managed
  {
    // Code size       100 (0x64)
    .maxstack  5
    .locals init (valuetype Thrift.Protocol.TMessage V_0,
             uint8[] V_1)
    IL_0000:  ldloca.s   V_0
    IL_0002:  initobj    Thrift.Protocol.TMessage
    IL_0008:  ldarg.0
    IL_0009:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONArrayStart()
    IL_000e:  ldarg.0
    IL_000f:  call       instance int64 Thrift.Protocol.TJSONProtocol::ReadJSONInteger()
    IL_0014:  ldc.i4.1
    IL_0015:  conv.i8
    IL_0016:  beq        IL_0027

    IL_001b:  ldc.i4.4
    IL_001c:  ldstr      "Message contained bad version."
    IL_0021:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(int32,
                                                                                 string)
    IL_0026:  throw

    IL_0027:  ldarg.0
    IL_0028:  ldc.i4.0
    IL_0029:  call       instance uint8[] Thrift.Protocol.TJSONProtocol::ReadJSONString(bool)
    IL_002e:  stloc.1
    IL_002f:  ldloca.s   V_0
    IL_0031:  ldarg.0
    IL_0032:  ldfld      class [mscorlib]System.Text.Encoding Thrift.Protocol.TJSONProtocol::utf8Encoding
    IL_0037:  ldloc.1
    IL_0038:  ldc.i4.0
    IL_0039:  ldloc.1
    IL_003a:  ldlen
    IL_003b:  conv.i4
    IL_003c:  callvirt   instance string [mscorlib]System.Text.Encoding::GetString(uint8[],
                                                                                   int32,
                                                                                   int32)
    IL_0041:  call       instance void Thrift.Protocol.TMessage::set_Name(string)
    IL_0046:  ldloca.s   V_0
    IL_0048:  ldarg.0
    IL_0049:  call       instance int64 Thrift.Protocol.TJSONProtocol::ReadJSONInteger()
    IL_004e:  conv.i4
    IL_004f:  call       instance void Thrift.Protocol.TMessage::set_Type(valuetype Thrift.Protocol.TMessageType)
    IL_0054:  ldloca.s   V_0
    IL_0056:  ldarg.0
    IL_0057:  call       instance int64 Thrift.Protocol.TJSONProtocol::ReadJSONInteger()
    IL_005c:  conv.i4
    IL_005d:  call       instance void Thrift.Protocol.TMessage::set_SeqID(int32)
    IL_0062:  ldloc.0
    IL_0063:  ret
  } // end of method TJSONProtocol::ReadMessageBegin

  .method public hidebysig virtual instance void 
          ReadMessageEnd() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONArrayEnd()
    IL_0006:  ret
  } // end of method TJSONProtocol::ReadMessageEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TStruct 
          ReadStructBegin() cil managed
  {
    // Code size       16 (0x10)
    .maxstack  1
    .locals init (valuetype Thrift.Protocol.TStruct V_0)
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONObjectStart()
    IL_0006:  ldloca.s   V_0
    IL_0008:  initobj    Thrift.Protocol.TStruct
    IL_000e:  ldloc.0
    IL_000f:  ret
  } // end of method TJSONProtocol::ReadStructBegin

  .method public hidebysig virtual instance void 
          ReadStructEnd() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONObjectEnd()
    IL_0006:  ret
  } // end of method TJSONProtocol::ReadStructEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TField 
          ReadFieldBegin() cil managed
  {
    // Code size       87 (0x57)
    .maxstack  3
    .locals init (valuetype Thrift.Protocol.TField V_0,
             uint8 V_1)
    IL_0000:  ldloca.s   V_0
    IL_0002:  initobj    Thrift.Protocol.TField
    IL_0008:  ldarg.0
    IL_0009:  ldfld      class Thrift.Protocol.TJSONProtocol/LookaheadReader Thrift.Protocol.TJSONProtocol::reader
    IL_000e:  callvirt   instance uint8 Thrift.Protocol.TJSONProtocol/LookaheadReader::Peek()
    IL_0013:  stloc.1
    IL_0014:  ldloc.1
    IL_0015:  ldsfld     uint8[] Thrift.Protocol.TJSONProtocol::RBRACE
    IL_001a:  ldc.i4.0
    IL_001b:  ldelem.u1
    IL_001c:  bne.un     IL_002e

    IL_0021:  ldloca.s   V_0
    IL_0023:  ldc.i4.0
    IL_0024:  call       instance void Thrift.Protocol.TField::set_Type(valuetype Thrift.Protocol.TType)
    IL_0029:  br         IL_0055

    IL_002e:  ldloca.s   V_0
    IL_0030:  ldarg.0
    IL_0031:  call       instance int64 Thrift.Protocol.TJSONProtocol::ReadJSONInteger()
    IL_0036:  conv.i2
    IL_0037:  call       instance void Thrift.Protocol.TField::set_ID(int16)
    IL_003c:  ldarg.0
    IL_003d:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONObjectStart()
    IL_0042:  ldloca.s   V_0
    IL_0044:  ldarg.0
    IL_0045:  ldc.i4.0
    IL_0046:  call       instance uint8[] Thrift.Protocol.TJSONProtocol::ReadJSONString(bool)
    IL_004b:  call       valuetype Thrift.Protocol.TType Thrift.Protocol.TJSONProtocol::GetTypeIDForTypeName(uint8[])
    IL_0050:  call       instance void Thrift.Protocol.TField::set_Type(valuetype Thrift.Protocol.TType)
    IL_0055:  ldloc.0
    IL_0056:  ret
  } // end of method TJSONProtocol::ReadFieldBegin

  .method public hidebysig virtual instance void 
          ReadFieldEnd() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONObjectEnd()
    IL_0006:  ret
  } // end of method TJSONProtocol::ReadFieldEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TMap 
          ReadMapBegin() cil managed
  {
    // Code size       74 (0x4a)
    .maxstack  3
    .locals init (valuetype Thrift.Protocol.TMap V_0)
    IL_0000:  ldloca.s   V_0
    IL_0002:  initobj    Thrift.Protocol.TMap
    IL_0008:  ldarg.0
    IL_0009:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONArrayStart()
    IL_000e:  ldloca.s   V_0
    IL_0010:  ldarg.0
    IL_0011:  ldc.i4.0
    IL_0012:  call       instance uint8[] Thrift.Protocol.TJSONProtocol::ReadJSONString(bool)
    IL_0017:  call       valuetype Thrift.Protocol.TType Thrift.Protocol.TJSONProtocol::GetTypeIDForTypeName(uint8[])
    IL_001c:  call       instance void Thrift.Protocol.TMap::set_KeyType(valuetype Thrift.Protocol.TType)
    IL_0021:  ldloca.s   V_0
    IL_0023:  ldarg.0
    IL_0024:  ldc.i4.0
    IL_0025:  call       instance uint8[] Thrift.Protocol.TJSONProtocol::ReadJSONString(bool)
    IL_002a:  call       valuetype Thrift.Protocol.TType Thrift.Protocol.TJSONProtocol::GetTypeIDForTypeName(uint8[])
    IL_002f:  call       instance void Thrift.Protocol.TMap::set_ValueType(valuetype Thrift.Protocol.TType)
    IL_0034:  ldloca.s   V_0
    IL_0036:  ldarg.0
    IL_0037:  call       instance int64 Thrift.Protocol.TJSONProtocol::ReadJSONInteger()
    IL_003c:  conv.i4
    IL_003d:  call       instance void Thrift.Protocol.TMap::set_Count(int32)
    IL_0042:  ldarg.0
    IL_0043:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONObjectStart()
    IL_0048:  ldloc.0
    IL_0049:  ret
  } // end of method TJSONProtocol::ReadMapBegin

  .method public hidebysig virtual instance void 
          ReadMapEnd() cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONObjectEnd()
    IL_0006:  ldarg.0
    IL_0007:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONArrayEnd()
    IL_000c:  ret
  } // end of method TJSONProtocol::ReadMapEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TList 
          ReadListBegin() cil managed
  {
    // Code size       49 (0x31)
    .maxstack  3
    .locals init (valuetype Thrift.Protocol.TList V_0)
    IL_0000:  ldloca.s   V_0
    IL_0002:  initobj    Thrift.Protocol.TList
    IL_0008:  ldarg.0
    IL_0009:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONArrayStart()
    IL_000e:  ldloca.s   V_0
    IL_0010:  ldarg.0
    IL_0011:  ldc.i4.0
    IL_0012:  call       instance uint8[] Thrift.Protocol.TJSONProtocol::ReadJSONString(bool)
    IL_0017:  call       valuetype Thrift.Protocol.TType Thrift.Protocol.TJSONProtocol::GetTypeIDForTypeName(uint8[])
    IL_001c:  call       instance void Thrift.Protocol.TList::set_ElementType(valuetype Thrift.Protocol.TType)
    IL_0021:  ldloca.s   V_0
    IL_0023:  ldarg.0
    IL_0024:  call       instance int64 Thrift.Protocol.TJSONProtocol::ReadJSONInteger()
    IL_0029:  conv.i4
    IL_002a:  call       instance void Thrift.Protocol.TList::set_Count(int32)
    IL_002f:  ldloc.0
    IL_0030:  ret
  } // end of method TJSONProtocol::ReadListBegin

  .method public hidebysig virtual instance void 
          ReadListEnd() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONArrayEnd()
    IL_0006:  ret
  } // end of method TJSONProtocol::ReadListEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TSet 
          ReadSetBegin() cil managed
  {
    // Code size       49 (0x31)
    .maxstack  3
    .locals init (valuetype Thrift.Protocol.TSet V_0)
    IL_0000:  ldloca.s   V_0
    IL_0002:  initobj    Thrift.Protocol.TSet
    IL_0008:  ldarg.0
    IL_0009:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONArrayStart()
    IL_000e:  ldloca.s   V_0
    IL_0010:  ldarg.0
    IL_0011:  ldc.i4.0
    IL_0012:  call       instance uint8[] Thrift.Protocol.TJSONProtocol::ReadJSONString(bool)
    IL_0017:  call       valuetype Thrift.Protocol.TType Thrift.Protocol.TJSONProtocol::GetTypeIDForTypeName(uint8[])
    IL_001c:  call       instance void Thrift.Protocol.TSet::set_ElementType(valuetype Thrift.Protocol.TType)
    IL_0021:  ldloca.s   V_0
    IL_0023:  ldarg.0
    IL_0024:  call       instance int64 Thrift.Protocol.TJSONProtocol::ReadJSONInteger()
    IL_0029:  conv.i4
    IL_002a:  call       instance void Thrift.Protocol.TSet::set_Count(int32)
    IL_002f:  ldloc.0
    IL_0030:  ret
  } // end of method TJSONProtocol::ReadSetBegin

  .method public hidebysig virtual instance void 
          ReadSetEnd() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Protocol.TJSONProtocol::ReadJSONArrayEnd()
    IL_0006:  ret
  } // end of method TJSONProtocol::ReadSetEnd

  .method public hidebysig virtual instance bool 
          ReadBool() cil managed
  {
    // Code size       21 (0x15)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance int64 Thrift.Protocol.TJSONProtocol::ReadJSONInteger()
    IL_0006:  ldc.i4.0
    IL_0007:  conv.i8
    IL_0008:  bne.un     IL_0013

    IL_000d:  ldc.i4.0
    IL_000e:  br         IL_0014

    IL_0013:  ldc.i4.1
    IL_0014:  ret
  } // end of method TJSONProtocol::ReadBool

  .method public hidebysig virtual instance int8 
          ReadByte() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance int64 Thrift.Protocol.TJSONProtocol::ReadJSONInteger()
    IL_0006:  conv.i1
    IL_0007:  ret
  } // end of method TJSONProtocol::ReadByte

  .method public hidebysig virtual instance int16 
          ReadI16() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance int64 Thrift.Protocol.TJSONProtocol::ReadJSONInteger()
    IL_0006:  conv.i2
    IL_0007:  ret
  } // end of method TJSONProtocol::ReadI16

  .method public hidebysig virtual instance int32 
          ReadI32() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance int64 Thrift.Protocol.TJSONProtocol::ReadJSONInteger()
    IL_0006:  conv.i4
    IL_0007:  ret
  } // end of method TJSONProtocol::ReadI32

  .method public hidebysig virtual instance int64 
          ReadI64() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance int64 Thrift.Protocol.TJSONProtocol::ReadJSONInteger()
    IL_0006:  ret
  } // end of method TJSONProtocol::ReadI64

  .method public hidebysig virtual instance float64 
          ReadDouble() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance float64 Thrift.Protocol.TJSONProtocol::ReadJSONDouble()
    IL_0006:  ret
  } // end of method TJSONProtocol::ReadDouble

  .method public hidebysig virtual instance string 
          ReadString() cil managed
  {
    // Code size       25 (0x19)
    .maxstack  4
    .locals init (uint8[] V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.0
    IL_0002:  call       instance uint8[] Thrift.Protocol.TJSONProtocol::ReadJSONString(bool)
    IL_0007:  stloc.0
    IL_0008:  ldarg.0
    IL_0009:  ldfld      class [mscorlib]System.Text.Encoding Thrift.Protocol.TJSONProtocol::utf8Encoding
    IL_000e:  ldloc.0
    IL_000f:  ldc.i4.0
    IL_0010:  ldloc.0
    IL_0011:  ldlen
    IL_0012:  conv.i4
    IL_0013:  callvirt   instance string [mscorlib]System.Text.Encoding::GetString(uint8[],
                                                                                   int32,
                                                                                   int32)
    IL_0018:  ret
  } // end of method TJSONProtocol::ReadString

  .method public hidebysig virtual instance uint8[] 
          ReadBinary() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance uint8[] Thrift.Protocol.TJSONProtocol::ReadJSONBase64()
    IL_0006:  ret
  } // end of method TJSONProtocol::ReadBinary

  .method private hidebysig specialname rtspecialname static 
          void  .cctor() cil managed
  {
    // Code size       369 (0x171)
    .maxstack  4
    IL_0000:  ldc.i4.1
    IL_0001:  newarr     [mscorlib]System.Byte
    IL_0006:  dup
    IL_0007:  ldc.i4.0
    IL_0008:  ldc.i4.s   44
    IL_000a:  stelem.i1
    IL_000b:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::COMMA
    IL_0010:  ldc.i4.1
    IL_0011:  newarr     [mscorlib]System.Byte
    IL_0016:  dup
    IL_0017:  ldc.i4.0
    IL_0018:  ldc.i4.s   58
    IL_001a:  stelem.i1
    IL_001b:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::COLON
    IL_0020:  ldc.i4.1
    IL_0021:  newarr     [mscorlib]System.Byte
    IL_0026:  dup
    IL_0027:  ldc.i4.0
    IL_0028:  ldc.i4.s   123
    IL_002a:  stelem.i1
    IL_002b:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::LBRACE
    IL_0030:  ldc.i4.1
    IL_0031:  newarr     [mscorlib]System.Byte
    IL_0036:  dup
    IL_0037:  ldc.i4.0
    IL_0038:  ldc.i4.s   125
    IL_003a:  stelem.i1
    IL_003b:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::RBRACE
    IL_0040:  ldc.i4.1
    IL_0041:  newarr     [mscorlib]System.Byte
    IL_0046:  dup
    IL_0047:  ldc.i4.0
    IL_0048:  ldc.i4.s   91
    IL_004a:  stelem.i1
    IL_004b:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::LBRACKET
    IL_0050:  ldc.i4.1
    IL_0051:  newarr     [mscorlib]System.Byte
    IL_0056:  dup
    IL_0057:  ldc.i4.0
    IL_0058:  ldc.i4.s   93
    IL_005a:  stelem.i1
    IL_005b:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::RBRACKET
    IL_0060:  ldc.i4.1
    IL_0061:  newarr     [mscorlib]System.Byte
    IL_0066:  dup
    IL_0067:  ldc.i4.0
    IL_0068:  ldc.i4.s   34
    IL_006a:  stelem.i1
    IL_006b:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::QUOTE
    IL_0070:  ldc.i4.1
    IL_0071:  newarr     [mscorlib]System.Byte
    IL_0076:  dup
    IL_0077:  ldc.i4.0
    IL_0078:  ldc.i4.s   92
    IL_007a:  stelem.i1
    IL_007b:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::BACKSLASH
    IL_0080:  ldc.i4.2
    IL_0081:  newarr     [mscorlib]System.Byte
    IL_0086:  dup
    IL_0087:  ldc.i4.0
    IL_0088:  ldc.i4.s   116
    IL_008a:  stelem.i1
    IL_008b:  dup
    IL_008c:  ldc.i4.1
    IL_008d:  ldc.i4.s   102
    IL_008f:  stelem.i1
    IL_0090:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_BOOL
    IL_0095:  ldc.i4.2
    IL_0096:  newarr     [mscorlib]System.Byte
    IL_009b:  dup
    IL_009c:  ldc.i4.0
    IL_009d:  ldc.i4.s   105
    IL_009f:  stelem.i1
    IL_00a0:  dup
    IL_00a1:  ldc.i4.1
    IL_00a2:  ldc.i4.s   56
    IL_00a4:  stelem.i1
    IL_00a5:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_BYTE
    IL_00aa:  ldc.i4.3
    IL_00ab:  newarr     [mscorlib]System.Byte
    IL_00b0:  dup
    IL_00b1:  ldtoken    field valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'::'$field-4'
    IL_00b6:  call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array,
                                                                                                        valuetype [mscorlib]System.RuntimeFieldHandle)
    IL_00bb:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_I16
    IL_00c0:  ldc.i4.3
    IL_00c1:  newarr     [mscorlib]System.Byte
    IL_00c6:  dup
    IL_00c7:  ldtoken    field valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'::'$field-5'
    IL_00cc:  call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array,
                                                                                                        valuetype [mscorlib]System.RuntimeFieldHandle)
    IL_00d1:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_I32
    IL_00d6:  ldc.i4.3
    IL_00d7:  newarr     [mscorlib]System.Byte
    IL_00dc:  dup
    IL_00dd:  ldtoken    field valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'::'$field-6'
    IL_00e2:  call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array,
                                                                                                        valuetype [mscorlib]System.RuntimeFieldHandle)
    IL_00e7:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_I64
    IL_00ec:  ldc.i4.3
    IL_00ed:  newarr     [mscorlib]System.Byte
    IL_00f2:  dup
    IL_00f3:  ldtoken    field valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'::'$field-7'
    IL_00f8:  call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array,
                                                                                                        valuetype [mscorlib]System.RuntimeFieldHandle)
    IL_00fd:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_DOUBLE
    IL_0102:  ldc.i4.3
    IL_0103:  newarr     [mscorlib]System.Byte
    IL_0108:  dup
    IL_0109:  ldtoken    field valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'::'$field-8'
    IL_010e:  call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array,
                                                                                                        valuetype [mscorlib]System.RuntimeFieldHandle)
    IL_0113:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_STRUCT
    IL_0118:  ldc.i4.3
    IL_0119:  newarr     [mscorlib]System.Byte
    IL_011e:  dup
    IL_011f:  ldtoken    field valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'::'$field-9'
    IL_0124:  call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array,
                                                                                                        valuetype [mscorlib]System.RuntimeFieldHandle)
    IL_0129:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_STRING
    IL_012e:  ldc.i4.3
    IL_012f:  newarr     [mscorlib]System.Byte
    IL_0134:  dup
    IL_0135:  ldtoken    field valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'::'$field-A'
    IL_013a:  call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array,
                                                                                                        valuetype [mscorlib]System.RuntimeFieldHandle)
    IL_013f:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_MAP
    IL_0144:  ldc.i4.3
    IL_0145:  newarr     [mscorlib]System.Byte
    IL_014a:  dup
    IL_014b:  ldtoken    field valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'::'$field-B'
    IL_0150:  call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array,
                                                                                                        valuetype [mscorlib]System.RuntimeFieldHandle)
    IL_0155:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_LIST
    IL_015a:  ldc.i4.3
    IL_015b:  newarr     [mscorlib]System.Byte
    IL_0160:  dup
    IL_0161:  ldtoken    field valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'::'$field-C'
    IL_0166:  call       void [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array,
                                                                                                        valuetype [mscorlib]System.RuntimeFieldHandle)
    IL_016b:  stsfld     uint8[] Thrift.Protocol.TJSONProtocol::NAME_SET
    IL_0170:  ret
  } // end of method TJSONProtocol::.cctor

} // end of class Thrift.Protocol.TJSONProtocol

.class public sequential ansi sealed beforefieldinit Thrift.Protocol.TList
       extends [mscorlib]System.ValueType
{
  .field private valuetype Thrift.Protocol.TType elementType
  .field private int32 count
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype Thrift.Protocol.TType elementType,
                               int32 count) cil managed
  {
    // Code size       22 (0x16)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  initobj    Thrift.Protocol.TList
    IL_0007:  ldarg.0
    IL_0008:  ldarg.1
    IL_0009:  stfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TList::elementType
    IL_000e:  ldarg.0
    IL_000f:  ldarg.2
    IL_0010:  stfld      int32 Thrift.Protocol.TList::count
    IL_0015:  ret
  } // end of method TList::.ctor

  .method public hidebysig specialname instance valuetype Thrift.Protocol.TType 
          get_ElementType() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TList::elementType
    IL_0006:  ret
  } // end of method TList::get_ElementType

  .method public hidebysig specialname instance void 
          set_ElementType(valuetype Thrift.Protocol.TType 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TList::elementType
    IL_0007:  ret
  } // end of method TList::set_ElementType

  .method public hidebysig specialname instance int32 
          get_Count() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      int32 Thrift.Protocol.TList::count
    IL_0006:  ret
  } // end of method TList::get_Count

  .method public hidebysig specialname instance void 
          set_Count(int32 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      int32 Thrift.Protocol.TList::count
    IL_0007:  ret
  } // end of method TList::set_Count

  .property instance valuetype Thrift.Protocol.TType
          ElementType()
  {
    .get instance valuetype Thrift.Protocol.TType Thrift.Protocol.TList::get_ElementType()
    .set instance void Thrift.Protocol.TList::set_ElementType(valuetype Thrift.Protocol.TType)
  } // end of property TList::ElementType
  .property instance int32 Count()
  {
    .get instance int32 Thrift.Protocol.TList::get_Count()
    .set instance void Thrift.Protocol.TList::set_Count(int32)
  } // end of property TList::Count
} // end of class Thrift.Protocol.TList

.class public sequential ansi sealed beforefieldinit Thrift.Protocol.TMap
       extends [mscorlib]System.ValueType
{
  .field private valuetype Thrift.Protocol.TType keyType
  .field private valuetype Thrift.Protocol.TType valueType
  .field private int32 count
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype Thrift.Protocol.TType keyType,
                               valuetype Thrift.Protocol.TType valueType,
                               int32 count) cil managed
  {
    // Code size       29 (0x1d)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  initobj    Thrift.Protocol.TMap
    IL_0007:  ldarg.0
    IL_0008:  ldarg.1
    IL_0009:  stfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::keyType
    IL_000e:  ldarg.0
    IL_000f:  ldarg.2
    IL_0010:  stfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::valueType
    IL_0015:  ldarg.0
    IL_0016:  ldarg.3
    IL_0017:  stfld      int32 Thrift.Protocol.TMap::count
    IL_001c:  ret
  } // end of method TMap::.ctor

  .method public hidebysig specialname instance valuetype Thrift.Protocol.TType 
          get_KeyType() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::keyType
    IL_0006:  ret
  } // end of method TMap::get_KeyType

  .method public hidebysig specialname instance void 
          set_KeyType(valuetype Thrift.Protocol.TType 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::keyType
    IL_0007:  ret
  } // end of method TMap::set_KeyType

  .method public hidebysig specialname instance valuetype Thrift.Protocol.TType 
          get_ValueType() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::valueType
    IL_0006:  ret
  } // end of method TMap::get_ValueType

  .method public hidebysig specialname instance void 
          set_ValueType(valuetype Thrift.Protocol.TType 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::valueType
    IL_0007:  ret
  } // end of method TMap::set_ValueType

  .method public hidebysig specialname instance int32 
          get_Count() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      int32 Thrift.Protocol.TMap::count
    IL_0006:  ret
  } // end of method TMap::get_Count

  .method public hidebysig specialname instance void 
          set_Count(int32 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      int32 Thrift.Protocol.TMap::count
    IL_0007:  ret
  } // end of method TMap::set_Count

  .property instance valuetype Thrift.Protocol.TType
          KeyType()
  {
    .get instance valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::get_KeyType()
    .set instance void Thrift.Protocol.TMap::set_KeyType(valuetype Thrift.Protocol.TType)
  } // end of property TMap::KeyType
  .property instance valuetype Thrift.Protocol.TType
          ValueType()
  {
    .get instance valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::get_ValueType()
    .set instance void Thrift.Protocol.TMap::set_ValueType(valuetype Thrift.Protocol.TType)
  } // end of property TMap::ValueType
  .property instance int32 Count()
  {
    .get instance int32 Thrift.Protocol.TMap::get_Count()
    .set instance void Thrift.Protocol.TMap::set_Count(int32)
  } // end of property TMap::Count
} // end of class Thrift.Protocol.TMap

.class public sequential ansi sealed beforefieldinit Thrift.Protocol.TMessage
       extends [mscorlib]System.ValueType
{
  .field private string name
  .field private valuetype Thrift.Protocol.TMessageType 'type'
  .field private int32 seqID
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string name,
                               valuetype Thrift.Protocol.TMessageType 'type',
                               int32 seqid) cil managed
  {
    // Code size       29 (0x1d)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  initobj    Thrift.Protocol.TMessage
    IL_0007:  ldarg.0
    IL_0008:  ldarg.1
    IL_0009:  stfld      string Thrift.Protocol.TMessage::name
    IL_000e:  ldarg.0
    IL_000f:  ldarg.2
    IL_0010:  stfld      valuetype Thrift.Protocol.TMessageType Thrift.Protocol.TMessage::'type'
    IL_0015:  ldarg.0
    IL_0016:  ldarg.3
    IL_0017:  stfld      int32 Thrift.Protocol.TMessage::seqID
    IL_001c:  ret
  } // end of method TMessage::.ctor

  .method public hidebysig specialname instance string 
          get_Name() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      string Thrift.Protocol.TMessage::name
    IL_0006:  ret
  } // end of method TMessage::get_Name

  .method public hidebysig specialname instance void 
          set_Name(string 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      string Thrift.Protocol.TMessage::name
    IL_0007:  ret
  } // end of method TMessage::set_Name

  .method public hidebysig specialname instance valuetype Thrift.Protocol.TMessageType 
          get_Type() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      valuetype Thrift.Protocol.TMessageType Thrift.Protocol.TMessage::'type'
    IL_0006:  ret
  } // end of method TMessage::get_Type

  .method public hidebysig specialname instance void 
          set_Type(valuetype Thrift.Protocol.TMessageType 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      valuetype Thrift.Protocol.TMessageType Thrift.Protocol.TMessage::'type'
    IL_0007:  ret
  } // end of method TMessage::set_Type

  .method public hidebysig specialname instance int32 
          get_SeqID() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      int32 Thrift.Protocol.TMessage::seqID
    IL_0006:  ret
  } // end of method TMessage::get_SeqID

  .method public hidebysig specialname instance void 
          set_SeqID(int32 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      int32 Thrift.Protocol.TMessage::seqID
    IL_0007:  ret
  } // end of method TMessage::set_SeqID

  .property instance string Name()
  {
    .get instance string Thrift.Protocol.TMessage::get_Name()
    .set instance void Thrift.Protocol.TMessage::set_Name(string)
  } // end of property TMessage::Name
  .property instance valuetype Thrift.Protocol.TMessageType
          Type()
  {
    .get instance valuetype Thrift.Protocol.TMessageType Thrift.Protocol.TMessage::get_Type()
    .set instance void Thrift.Protocol.TMessage::set_Type(valuetype Thrift.Protocol.TMessageType)
  } // end of property TMessage::Type
  .property instance int32 SeqID()
  {
    .get instance int32 Thrift.Protocol.TMessage::get_SeqID()
    .set instance void Thrift.Protocol.TMessage::set_SeqID(int32)
  } // end of property TMessage::SeqID
} // end of class Thrift.Protocol.TMessage

.class public auto ansi sealed Thrift.Protocol.TMessageType
       extends [mscorlib]System.Enum
{
  .field public specialname rtspecialname int32 value__
  .field public static literal valuetype Thrift.Protocol.TMessageType Call = int32(0x00000001)
  .field public static literal valuetype Thrift.Protocol.TMessageType Reply = int32(0x00000002)
  .field public static literal valuetype Thrift.Protocol.TMessageType Exception = int32(0x00000003)
  .field public static literal valuetype Thrift.Protocol.TMessageType Oneway = int32(0x00000004)
} // end of class Thrift.Protocol.TMessageType

.class public auto ansi beforefieldinit Thrift.Protocol.TMultiplexedProcessor
       extends [mscorlib]System.Object
       implements Thrift.TProcessor
{
  .class auto ansi nested private beforefieldinit StoredMessageProtocol
         extends Thrift.Protocol.TProtocolDecorator
  {
    .field private valuetype Thrift.Protocol.TMessage MsgBegin
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor(class Thrift.Protocol.TProtocol protocol,
                                 valuetype Thrift.Protocol.TMessage messageBegin) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldarg.1
      IL_0002:  call       instance void Thrift.Protocol.TProtocolDecorator::.ctor(class Thrift.Protocol.TProtocol)
      IL_0007:  ldarg.0
      IL_0008:  ldarg.2
      IL_0009:  stfld      valuetype Thrift.Protocol.TMessage Thrift.Protocol.TMultiplexedProcessor/StoredMessageProtocol::MsgBegin
      IL_000e:  ret
    } // end of method StoredMessageProtocol::.ctor

    .method public hidebysig virtual instance valuetype Thrift.Protocol.TMessage 
            ReadMessageBegin() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      valuetype Thrift.Protocol.TMessage Thrift.Protocol.TMultiplexedProcessor/StoredMessageProtocol::MsgBegin
      IL_0006:  ret
    } // end of method StoredMessageProtocol::ReadMessageBegin

  } // end of class StoredMessageProtocol

  .field private class [mscorlib]System.Collections.Generic.Dictionary`2<string,class Thrift.TProcessor> ServiceProcessorMap
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  newobj     instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,class Thrift.TProcessor>::.ctor()
    IL_0006:  stfld      class [mscorlib]System.Collections.Generic.Dictionary`2<string,class Thrift.TProcessor> Thrift.Protocol.TMultiplexedProcessor::ServiceProcessorMap
    IL_000b:  ldarg.0
    IL_000c:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0011:  ret
  } // end of method TMultiplexedProcessor::.ctor

  .method public hidebysig instance void 
          RegisterProcessor(string serviceName,
                            class Thrift.TProcessor processor) cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.Collections.Generic.Dictionary`2<string,class Thrift.TProcessor> Thrift.Protocol.TMultiplexedProcessor::ServiceProcessorMap
    IL_0006:  ldarg.1
    IL_0007:  ldarg.2
    IL_0008:  callvirt   instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,class Thrift.TProcessor>::Add(!0,
                                                                                                                                    !1)
    IL_000d:  ret
  } // end of method TMultiplexedProcessor::RegisterProcessor

  .method private hidebysig instance void 
          Fail(class Thrift.Protocol.TProtocol oprot,
               valuetype Thrift.Protocol.TMessage message,
               valuetype Thrift.TApplicationException/ExceptionType extype,
               string etxt) cil managed
  {
    // Code size       63 (0x3f)
    .maxstack  4
    .locals init (class Thrift.TApplicationException V_0,
             valuetype Thrift.Protocol.TMessage V_1)
    IL_0000:  ldarg.3
    IL_0001:  ldarg.s    etxt
    IL_0003:  newobj     instance void Thrift.TApplicationException::.ctor(valuetype Thrift.TApplicationException/ExceptionType,
                                                                           string)
    IL_0008:  stloc.0
    IL_0009:  ldloca.s   V_1
    IL_000b:  ldarga.s   message
    IL_000d:  call       instance string Thrift.Protocol.TMessage::get_Name()
    IL_0012:  ldc.i4.3
    IL_0013:  ldarga.s   message
    IL_0015:  call       instance int32 Thrift.Protocol.TMessage::get_SeqID()
    IL_001a:  call       instance void Thrift.Protocol.TMessage::.ctor(string,
                                                                       valuetype Thrift.Protocol.TMessageType,
                                                                       int32)
    IL_001f:  ldarg.1
    IL_0020:  ldloc.1
    IL_0021:  callvirt   instance void Thrift.Protocol.TProtocol::WriteMessageBegin(valuetype Thrift.Protocol.TMessage)
    IL_0026:  ldloc.0
    IL_0027:  ldarg.1
    IL_0028:  callvirt   instance void Thrift.TApplicationException::Write(class Thrift.Protocol.TProtocol)
    IL_002d:  ldarg.1
    IL_002e:  callvirt   instance void Thrift.Protocol.TProtocol::WriteMessageEnd()
    IL_0033:  ldarg.1
    IL_0034:  callvirt   instance class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::get_Transport()
    IL_0039:  callvirt   instance void Thrift.Transport.TTransport::Flush()
    IL_003e:  ret
  } // end of method TMultiplexedProcessor::Fail

  .method public hidebysig newslot virtual final 
          instance bool  Process(class Thrift.Protocol.TProtocol iprot,
                                 class Thrift.Protocol.TProtocol oprot) cil managed
  {
    // Code size       266 (0x10a)
    .maxstack  7
    .locals init (valuetype Thrift.Protocol.TMessage V_0,
             bool V_1,
             int32 V_2,
             string V_3,
             class Thrift.TProcessor V_4,
             valuetype Thrift.Protocol.TMessage V_5)
    .try
    {
      IL_0000:  ldarg.1
      IL_0001:  callvirt   instance valuetype Thrift.Protocol.TMessage Thrift.Protocol.TProtocol::ReadMessageBegin()
      IL_0006:  stloc.0
      IL_0007:  ldloca.s   V_0
      IL_0009:  call       instance valuetype Thrift.Protocol.TMessageType Thrift.Protocol.TMessage::get_Type()
      IL_000e:  ldc.i4.1
      IL_000f:  beq        IL_0036

      IL_0014:  ldloca.s   V_0
      IL_0016:  call       instance valuetype Thrift.Protocol.TMessageType Thrift.Protocol.TMessage::get_Type()
      IL_001b:  ldc.i4.4
      IL_001c:  beq        IL_0036

      IL_0021:  ldarg.0
      IL_0022:  ldarg.2
      IL_0023:  ldloc.0
      IL_0024:  ldc.i4.2
      IL_0025:  ldstr      "Message type CALL or ONEWAY expected"
      IL_002a:  call       instance void Thrift.Protocol.TMultiplexedProcessor::Fail(class Thrift.Protocol.TProtocol,
                                                                                     valuetype Thrift.Protocol.TMessage,
                                                                                     valuetype Thrift.TApplicationException/ExceptionType,
                                                                                     string)
      IL_002f:  ldc.i4.0
      IL_0030:  stloc.1
      IL_0031:  leave      IL_0108

      IL_0036:  ldloca.s   V_0
      IL_0038:  call       instance string Thrift.Protocol.TMessage::get_Name()
      IL_003d:  ldsfld     string Thrift.Protocol.TMultiplexedProtocol::SEPARATOR
      IL_0042:  callvirt   instance int32 [mscorlib]System.String::IndexOf(string)
      IL_0047:  stloc.2
      IL_0048:  ldloc.2
      IL_0049:  ldc.i4.0
      IL_004a:  bge        IL_0076

      IL_004f:  ldarg.0
      IL_0050:  ldarg.2
      IL_0051:  ldloc.0
      IL_0052:  ldc.i4.s   9
      IL_0054:  ldstr      "Service name not found in message name: "
      IL_0059:  ldloca.s   V_0
      IL_005b:  call       instance string Thrift.Protocol.TMessage::get_Name()
      IL_0060:  ldstr      ". Did you forget to use a TMultiplexProtocol in yo"
      + "ur client\?"
      IL_0065:  call       string [mscorlib]System.String::Concat(string,
                                                                  string,
                                                                  string)
      IL_006a:  call       instance void Thrift.Protocol.TMultiplexedProcessor::Fail(class Thrift.Protocol.TProtocol,
                                                                                     valuetype Thrift.Protocol.TMessage,
                                                                                     valuetype Thrift.TApplicationException/ExceptionType,
                                                                                     string)
      IL_006f:  ldc.i4.0
      IL_0070:  stloc.1
      IL_0071:  leave      IL_0108

      IL_0076:  ldloca.s   V_0
      IL_0078:  call       instance string Thrift.Protocol.TMessage::get_Name()
      IL_007d:  ldc.i4.0
      IL_007e:  ldloc.2
      IL_007f:  callvirt   instance string [mscorlib]System.String::Substring(int32,
                                                                              int32)
      IL_0084:  stloc.3
      IL_0085:  ldarg.0
      IL_0086:  ldfld      class [mscorlib]System.Collections.Generic.Dictionary`2<string,class Thrift.TProcessor> Thrift.Protocol.TMultiplexedProcessor::ServiceProcessorMap
      IL_008b:  ldloc.3
      IL_008c:  ldloca.s   V_4
      IL_008e:  callvirt   instance bool class [mscorlib]System.Collections.Generic.Dictionary`2<string,class Thrift.TProcessor>::TryGetValue(!0,
                                                                                                                                              !1&)
      IL_0093:  brtrue     IL_00b8

      IL_0098:  ldarg.0
      IL_0099:  ldarg.2
      IL_009a:  ldloc.0
      IL_009b:  ldc.i4.6
      IL_009c:  ldstr      "Service name not found: "
      IL_00a1:  ldloc.3
      IL_00a2:  ldstr      ". Did you forget to call RegisterProcessor()\?"
      IL_00a7:  call       string [mscorlib]System.String::Concat(string,
                                                                  string,
                                                                  string)
      IL_00ac:  call       instance void Thrift.Protocol.TMultiplexedProcessor::Fail(class Thrift.Protocol.TProtocol,
                                                                                     valuetype Thrift.Protocol.TMessage,
                                                                                     valuetype Thrift.TApplicationException/ExceptionType,
                                                                                     string)
      IL_00b1:  ldc.i4.0
      IL_00b2:  stloc.1
      IL_00b3:  leave      IL_0108

      IL_00b8:  ldloca.s   V_5
      IL_00ba:  ldloca.s   V_0
      IL_00bc:  call       instance string Thrift.Protocol.TMessage::get_Name()
      IL_00c1:  ldloc.3
      IL_00c2:  callvirt   instance int32 [mscorlib]System.String::get_Length()
      IL_00c7:  ldsfld     string Thrift.Protocol.TMultiplexedProtocol::SEPARATOR
      IL_00cc:  callvirt   instance int32 [mscorlib]System.String::get_Length()
      IL_00d1:  add
      IL_00d2:  callvirt   instance string [mscorlib]System.String::Substring(int32)
      IL_00d7:  ldloca.s   V_0
      IL_00d9:  call       instance valuetype Thrift.Protocol.TMessageType Thrift.Protocol.TMessage::get_Type()
      IL_00de:  ldloca.s   V_0
      IL_00e0:  call       instance int32 Thrift.Protocol.TMessage::get_SeqID()
      IL_00e5:  call       instance void Thrift.Protocol.TMessage::.ctor(string,
                                                                         valuetype Thrift.Protocol.TMessageType,
                                                                         int32)
      IL_00ea:  ldloc.s    V_4
      IL_00ec:  ldarg.1
      IL_00ed:  ldloc.s    V_5
      IL_00ef:  newobj     instance void Thrift.Protocol.TMultiplexedProcessor/StoredMessageProtocol::.ctor(class Thrift.Protocol.TProtocol,
                                                                                                            valuetype Thrift.Protocol.TMessage)
      IL_00f4:  ldarg.2
      IL_00f5:  callvirt   instance bool Thrift.TProcessor::Process(class Thrift.Protocol.TProtocol,
                                                                    class Thrift.Protocol.TProtocol)
      IL_00fa:  stloc.1
      IL_00fb:  leave      IL_0108

    }  // end .try
    catch [mscorlib]System.IO.IOException 
    {
      IL_0100:  pop
      IL_0101:  ldc.i4.0
      IL_0102:  stloc.1
      IL_0103:  leave      IL_0108

    }  // end handler
    IL_0108:  ldloc.1
    IL_0109:  ret
  } // end of method TMultiplexedProcessor::Process

} // end of class Thrift.Protocol.TMultiplexedProcessor

.class public auto ansi beforefieldinit Thrift.Protocol.TMultiplexedProtocol
       extends Thrift.Protocol.TProtocolDecorator
{
  .field public static string SEPARATOR
  .field private string ServiceName
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.Protocol.TProtocol protocol,
                               string serviceName) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  call       instance void Thrift.Protocol.TProtocolDecorator::.ctor(class Thrift.Protocol.TProtocol)
    IL_0007:  ldarg.0
    IL_0008:  ldarg.2
    IL_0009:  stfld      string Thrift.Protocol.TMultiplexedProtocol::ServiceName
    IL_000e:  ret
  } // end of method TMultiplexedProtocol::.ctor

  .method public hidebysig virtual instance void 
          WriteMessageBegin(valuetype Thrift.Protocol.TMessage tMessage) cil managed
  {
    // Code size       93 (0x5d)
    .maxstack  4
    .locals init (valuetype Thrift.Protocol.TMessageType V_0)
    IL_0000:  ldarga.s   tMessage
    IL_0002:  call       instance valuetype Thrift.Protocol.TMessageType Thrift.Protocol.TMessage::get_Type()
    IL_0007:  stloc.0
    IL_0008:  ldloc.0
    IL_0009:  ldc.i4.1
    IL_000a:  beq        IL_001b

    IL_000f:  ldloc.0
    IL_0010:  ldc.i4.4
    IL_0011:  beq        IL_001b

    IL_0016:  br         IL_0050

    IL_001b:  ldarg.0
    IL_001c:  ldarg.0
    IL_001d:  ldfld      string Thrift.Protocol.TMultiplexedProtocol::ServiceName
    IL_0022:  ldsfld     string Thrift.Protocol.TMultiplexedProtocol::SEPARATOR
    IL_0027:  ldarga.s   tMessage
    IL_0029:  call       instance string Thrift.Protocol.TMessage::get_Name()
    IL_002e:  call       string [mscorlib]System.String::Concat(string,
                                                                string,
                                                                string)
    IL_0033:  ldarga.s   tMessage
    IL_0035:  call       instance valuetype Thrift.Protocol.TMessageType Thrift.Protocol.TMessage::get_Type()
    IL_003a:  ldarga.s   tMessage
    IL_003c:  call       instance int32 Thrift.Protocol.TMessage::get_SeqID()
    IL_0041:  newobj     instance void Thrift.Protocol.TMessage::.ctor(string,
                                                                       valuetype Thrift.Protocol.TMessageType,
                                                                       int32)
    IL_0046:  call       instance void Thrift.Protocol.TProtocolDecorator::WriteMessageBegin(valuetype Thrift.Protocol.TMessage)
    IL_004b:  br         IL_005c

    IL_0050:  ldarg.0
    IL_0051:  ldarg.1
    IL_0052:  call       instance void Thrift.Protocol.TProtocolDecorator::WriteMessageBegin(valuetype Thrift.Protocol.TMessage)
    IL_0057:  br         IL_005c

    IL_005c:  ret
  } // end of method TMultiplexedProtocol::WriteMessageBegin

  .method private hidebysig specialname rtspecialname static 
          void  .cctor() cil managed
  {
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  ldstr      ":"
    IL_0005:  stsfld     string Thrift.Protocol.TMultiplexedProtocol::SEPARATOR
    IL_000a:  ret
  } // end of method TMultiplexedProtocol::.cctor

} // end of class Thrift.Protocol.TMultiplexedProtocol

.class public abstract auto ansi beforefieldinit Thrift.Protocol.TProtocol
       extends [mscorlib]System.Object
       implements [mscorlib]System.IDisposable
{
  .field private static literal int32 DEFAULT_RECURSION_DEPTH = int32(0x00000040)
  .field family class Thrift.Transport.TTransport trans
  .field family int32 recursionLimit
  .field family int32 recursionDepth
  .field private bool _IsDisposed
  .method family hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.Transport.TTransport trans) cil managed
  {
    // Code size       29 (0x1d)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_000d:  ldarg.0
    IL_000e:  ldc.i4.s   64
    IL_0010:  stfld      int32 Thrift.Protocol.TProtocol::recursionLimit
    IL_0015:  ldarg.0
    IL_0016:  ldc.i4.0
    IL_0017:  stfld      int32 Thrift.Protocol.TProtocol::recursionDepth
    IL_001c:  ret
  } // end of method TProtocol::.ctor

  .method public hidebysig specialname instance class Thrift.Transport.TTransport 
          get_Transport() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0006:  ret
  } // end of method TProtocol::get_Transport

  .method public hidebysig specialname instance int32 
          get_RecursionLimit() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      int32 Thrift.Protocol.TProtocol::recursionLimit
    IL_0006:  ret
  } // end of method TProtocol::get_RecursionLimit

  .method public hidebysig specialname instance void 
          set_RecursionLimit(int32 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      int32 Thrift.Protocol.TProtocol::recursionLimit
    IL_0007:  ret
  } // end of method TProtocol::set_RecursionLimit

  .method public hidebysig instance void 
          IncrementRecursionDepth() cil managed
  {
    // Code size       49 (0x31)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      int32 Thrift.Protocol.TProtocol::recursionDepth
    IL_0006:  ldarg.0
    IL_0007:  ldfld      int32 Thrift.Protocol.TProtocol::recursionLimit
    IL_000c:  bge        IL_0024

    IL_0011:  ldarg.0
    IL_0012:  dup
    IL_0013:  ldfld      int32 Thrift.Protocol.TProtocol::recursionDepth
    IL_0018:  ldc.i4.1
    IL_0019:  add
    IL_001a:  stfld      int32 Thrift.Protocol.TProtocol::recursionDepth
    IL_001f:  br         IL_0030

    IL_0024:  ldc.i4.6
    IL_0025:  ldstr      "Depth limit exceeded"
    IL_002a:  newobj     instance void Thrift.Protocol.TProtocolException::.ctor(int32,
                                                                                 string)
    IL_002f:  throw

    IL_0030:  ret
  } // end of method TProtocol::IncrementRecursionDepth

  .method public hidebysig instance void 
          DecrementRecursionDepth() cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  dup
    IL_0002:  ldfld      int32 Thrift.Protocol.TProtocol::recursionDepth
    IL_0007:  ldc.i4.1
    IL_0008:  sub
    IL_0009:  stfld      int32 Thrift.Protocol.TProtocol::recursionDepth
    IL_000e:  ret
  } // end of method TProtocol::DecrementRecursionDepth

  .method public hidebysig newslot virtual final 
          instance void  Dispose() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.1
    IL_0002:  callvirt   instance void Thrift.Protocol.TProtocol::Dispose(bool)
    IL_0007:  ret
  } // end of method TProtocol::Dispose

  .method family hidebysig newslot virtual 
          instance void  Dispose(bool disposing) cil managed
  {
    // Code size       47 (0x2f)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      bool Thrift.Protocol.TProtocol::_IsDisposed
    IL_0006:  brtrue     IL_0027

    IL_000b:  ldarg.1
    IL_000c:  brfalse    IL_0027

    IL_0011:  ldarg.0
    IL_0012:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0017:  brfalse    IL_0027

    IL_001c:  ldarg.0
    IL_001d:  ldfld      class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::trans
    IL_0022:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
    IL_0027:  ldarg.0
    IL_0028:  ldc.i4.1
    IL_0029:  stfld      bool Thrift.Protocol.TProtocol::_IsDisposed
    IL_002e:  ret
  } // end of method TProtocol::Dispose

  .method public hidebysig newslot abstract virtual 
          instance void  WriteMessageBegin(valuetype Thrift.Protocol.TMessage message) cil managed
  {
  } // end of method TProtocol::WriteMessageBegin

  .method public hidebysig newslot abstract virtual 
          instance void  WriteMessageEnd() cil managed
  {
  } // end of method TProtocol::WriteMessageEnd

  .method public hidebysig newslot abstract virtual 
          instance void  WriteStructBegin(valuetype Thrift.Protocol.TStruct struc) cil managed
  {
  } // end of method TProtocol::WriteStructBegin

  .method public hidebysig newslot abstract virtual 
          instance void  WriteStructEnd() cil managed
  {
  } // end of method TProtocol::WriteStructEnd

  .method public hidebysig newslot abstract virtual 
          instance void  WriteFieldBegin(valuetype Thrift.Protocol.TField 'field') cil managed
  {
  } // end of method TProtocol::WriteFieldBegin

  .method public hidebysig newslot abstract virtual 
          instance void  WriteFieldEnd() cil managed
  {
  } // end of method TProtocol::WriteFieldEnd

  .method public hidebysig newslot abstract virtual 
          instance void  WriteFieldStop() cil managed
  {
  } // end of method TProtocol::WriteFieldStop

  .method public hidebysig newslot abstract virtual 
          instance void  WriteMapBegin(valuetype Thrift.Protocol.TMap map) cil managed
  {
  } // end of method TProtocol::WriteMapBegin

  .method public hidebysig newslot abstract virtual 
          instance void  WriteMapEnd() cil managed
  {
  } // end of method TProtocol::WriteMapEnd

  .method public hidebysig newslot abstract virtual 
          instance void  WriteListBegin(valuetype Thrift.Protocol.TList list) cil managed
  {
  } // end of method TProtocol::WriteListBegin

  .method public hidebysig newslot abstract virtual 
          instance void  WriteListEnd() cil managed
  {
  } // end of method TProtocol::WriteListEnd

  .method public hidebysig newslot abstract virtual 
          instance void  WriteSetBegin(valuetype Thrift.Protocol.TSet set) cil managed
  {
  } // end of method TProtocol::WriteSetBegin

  .method public hidebysig newslot abstract virtual 
          instance void  WriteSetEnd() cil managed
  {
  } // end of method TProtocol::WriteSetEnd

  .method public hidebysig newslot abstract virtual 
          instance void  WriteBool(bool b) cil managed
  {
  } // end of method TProtocol::WriteBool

  .method public hidebysig newslot abstract virtual 
          instance void  WriteByte(int8 b) cil managed
  {
  } // end of method TProtocol::WriteByte

  .method public hidebysig newslot abstract virtual 
          instance void  WriteI16(int16 i16) cil managed
  {
  } // end of method TProtocol::WriteI16

  .method public hidebysig newslot abstract virtual 
          instance void  WriteI32(int32 i32) cil managed
  {
  } // end of method TProtocol::WriteI32

  .method public hidebysig newslot abstract virtual 
          instance void  WriteI64(int64 i64) cil managed
  {
  } // end of method TProtocol::WriteI64

  .method public hidebysig newslot abstract virtual 
          instance void  WriteDouble(float64 d) cil managed
  {
  } // end of method TProtocol::WriteDouble

  .method public hidebysig newslot virtual 
          instance void  WriteString(string s) cil managed
  {
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       class [mscorlib]System.Text.Encoding [mscorlib]System.Text.Encoding::get_UTF8()
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance uint8[] [mscorlib]System.Text.Encoding::GetBytes(string)
    IL_000c:  callvirt   instance void Thrift.Protocol.TProtocol::WriteBinary(uint8[])
    IL_0011:  ret
  } // end of method TProtocol::WriteString

  .method public hidebysig newslot abstract virtual 
          instance void  WriteBinary(uint8[] b) cil managed
  {
  } // end of method TProtocol::WriteBinary

  .method public hidebysig newslot abstract virtual 
          instance valuetype Thrift.Protocol.TMessage 
          ReadMessageBegin() cil managed
  {
  } // end of method TProtocol::ReadMessageBegin

  .method public hidebysig newslot abstract virtual 
          instance void  ReadMessageEnd() cil managed
  {
  } // end of method TProtocol::ReadMessageEnd

  .method public hidebysig newslot abstract virtual 
          instance valuetype Thrift.Protocol.TStruct 
          ReadStructBegin() cil managed
  {
  } // end of method TProtocol::ReadStructBegin

  .method public hidebysig newslot abstract virtual 
          instance void  ReadStructEnd() cil managed
  {
  } // end of method TProtocol::ReadStructEnd

  .method public hidebysig newslot abstract virtual 
          instance valuetype Thrift.Protocol.TField 
          ReadFieldBegin() cil managed
  {
  } // end of method TProtocol::ReadFieldBegin

  .method public hidebysig newslot abstract virtual 
          instance void  ReadFieldEnd() cil managed
  {
  } // end of method TProtocol::ReadFieldEnd

  .method public hidebysig newslot abstract virtual 
          instance valuetype Thrift.Protocol.TMap 
          ReadMapBegin() cil managed
  {
  } // end of method TProtocol::ReadMapBegin

  .method public hidebysig newslot abstract virtual 
          instance void  ReadMapEnd() cil managed
  {
  } // end of method TProtocol::ReadMapEnd

  .method public hidebysig newslot abstract virtual 
          instance valuetype Thrift.Protocol.TList 
          ReadListBegin() cil managed
  {
  } // end of method TProtocol::ReadListBegin

  .method public hidebysig newslot abstract virtual 
          instance void  ReadListEnd() cil managed
  {
  } // end of method TProtocol::ReadListEnd

  .method public hidebysig newslot abstract virtual 
          instance valuetype Thrift.Protocol.TSet 
          ReadSetBegin() cil managed
  {
  } // end of method TProtocol::ReadSetBegin

  .method public hidebysig newslot abstract virtual 
          instance void  ReadSetEnd() cil managed
  {
  } // end of method TProtocol::ReadSetEnd

  .method public hidebysig newslot abstract virtual 
          instance bool  ReadBool() cil managed
  {
  } // end of method TProtocol::ReadBool

  .method public hidebysig newslot abstract virtual 
          instance int8  ReadByte() cil managed
  {
  } // end of method TProtocol::ReadByte

  .method public hidebysig newslot abstract virtual 
          instance int16  ReadI16() cil managed
  {
  } // end of method TProtocol::ReadI16

  .method public hidebysig newslot abstract virtual 
          instance int32  ReadI32() cil managed
  {
  } // end of method TProtocol::ReadI32

  .method public hidebysig newslot abstract virtual 
          instance int64  ReadI64() cil managed
  {
  } // end of method TProtocol::ReadI64

  .method public hidebysig newslot abstract virtual 
          instance float64  ReadDouble() cil managed
  {
  } // end of method TProtocol::ReadDouble

  .method public hidebysig newslot virtual 
          instance string  ReadString() cil managed
  {
    // Code size       23 (0x17)
    .maxstack  4
    .locals init (uint8[] V_0)
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance uint8[] Thrift.Protocol.TProtocol::ReadBinary()
    IL_0006:  stloc.0
    IL_0007:  call       class [mscorlib]System.Text.Encoding [mscorlib]System.Text.Encoding::get_UTF8()
    IL_000c:  ldloc.0
    IL_000d:  ldc.i4.0
    IL_000e:  ldloc.0
    IL_000f:  ldlen
    IL_0010:  conv.i4
    IL_0011:  callvirt   instance string [mscorlib]System.Text.Encoding::GetString(uint8[],
                                                                                   int32,
                                                                                   int32)
    IL_0016:  ret
  } // end of method TProtocol::ReadString

  .method public hidebysig newslot abstract virtual 
          instance uint8[]  ReadBinary() cil managed
  {
  } // end of method TProtocol::ReadBinary

  .property instance class Thrift.Transport.TTransport
          Transport()
  {
    .get instance class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::get_Transport()
  } // end of property TProtocol::Transport
  .property instance int32 RecursionLimit()
  {
    .get instance int32 Thrift.Protocol.TProtocol::get_RecursionLimit()
    .set instance void Thrift.Protocol.TProtocol::set_RecursionLimit(int32)
  } // end of property TProtocol::RecursionLimit
} // end of class Thrift.Protocol.TProtocol

.class public abstract auto ansi beforefieldinit Thrift.Protocol.TProtocolDecorator
       extends Thrift.Protocol.TProtocol
{
  .field private class Thrift.Protocol.TProtocol WrappedProtocol
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.Protocol.TProtocol protocol) cil managed
  {
    // Code size       20 (0x14)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  callvirt   instance class Thrift.Transport.TTransport Thrift.Protocol.TProtocol::get_Transport()
    IL_0007:  call       instance void Thrift.Protocol.TProtocol::.ctor(class Thrift.Transport.TTransport)
    IL_000c:  ldarg.0
    IL_000d:  ldarg.1
    IL_000e:  stfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0013:  ret
  } // end of method TProtocolDecorator::.ctor

  .method public hidebysig virtual instance void 
          WriteMessageBegin(valuetype Thrift.Protocol.TMessage tMessage) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteMessageBegin(valuetype Thrift.Protocol.TMessage)
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteMessageBegin

  .method public hidebysig virtual instance void 
          WriteMessageEnd() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance void Thrift.Protocol.TProtocol::WriteMessageEnd()
    IL_000b:  ret
  } // end of method TProtocolDecorator::WriteMessageEnd

  .method public hidebysig virtual instance void 
          WriteStructBegin(valuetype Thrift.Protocol.TStruct tStruct) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteStructBegin(valuetype Thrift.Protocol.TStruct)
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteStructBegin

  .method public hidebysig virtual instance void 
          WriteStructEnd() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance void Thrift.Protocol.TProtocol::WriteStructEnd()
    IL_000b:  ret
  } // end of method TProtocolDecorator::WriteStructEnd

  .method public hidebysig virtual instance void 
          WriteFieldBegin(valuetype Thrift.Protocol.TField tField) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteFieldBegin(valuetype Thrift.Protocol.TField)
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteFieldBegin

  .method public hidebysig virtual instance void 
          WriteFieldEnd() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance void Thrift.Protocol.TProtocol::WriteFieldEnd()
    IL_000b:  ret
  } // end of method TProtocolDecorator::WriteFieldEnd

  .method public hidebysig virtual instance void 
          WriteFieldStop() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance void Thrift.Protocol.TProtocol::WriteFieldStop()
    IL_000b:  ret
  } // end of method TProtocolDecorator::WriteFieldStop

  .method public hidebysig virtual instance void 
          WriteMapBegin(valuetype Thrift.Protocol.TMap tMap) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteMapBegin(valuetype Thrift.Protocol.TMap)
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteMapBegin

  .method public hidebysig virtual instance void 
          WriteMapEnd() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance void Thrift.Protocol.TProtocol::WriteMapEnd()
    IL_000b:  ret
  } // end of method TProtocolDecorator::WriteMapEnd

  .method public hidebysig virtual instance void 
          WriteListBegin(valuetype Thrift.Protocol.TList tList) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteListBegin(valuetype Thrift.Protocol.TList)
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteListBegin

  .method public hidebysig virtual instance void 
          WriteListEnd() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance void Thrift.Protocol.TProtocol::WriteListEnd()
    IL_000b:  ret
  } // end of method TProtocolDecorator::WriteListEnd

  .method public hidebysig virtual instance void 
          WriteSetBegin(valuetype Thrift.Protocol.TSet tSet) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteSetBegin(valuetype Thrift.Protocol.TSet)
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteSetBegin

  .method public hidebysig virtual instance void 
          WriteSetEnd() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance void Thrift.Protocol.TProtocol::WriteSetEnd()
    IL_000b:  ret
  } // end of method TProtocolDecorator::WriteSetEnd

  .method public hidebysig virtual instance void 
          WriteBool(bool b) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteBool(bool)
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteBool

  .method public hidebysig virtual instance void 
          WriteByte(int8 b) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteByte(int8)
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteByte

  .method public hidebysig virtual instance void 
          WriteI16(int16 i) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI16(int16)
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteI16

  .method public hidebysig virtual instance void 
          WriteI32(int32 i) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI32(int32)
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteI32

  .method public hidebysig virtual instance void 
          WriteI64(int64 l) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI64(int64)
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteI64

  .method public hidebysig virtual instance void 
          WriteDouble(float64 v) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteDouble(float64)
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteDouble

  .method public hidebysig virtual instance void 
          WriteString(string s) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteString(string)
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteString

  .method public hidebysig virtual instance void 
          WriteBinary(uint8[] bytes) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  ldarg.1
    IL_0007:  callvirt   instance void Thrift.Protocol.TProtocol::WriteBinary(uint8[])
    IL_000c:  ret
  } // end of method TProtocolDecorator::WriteBinary

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TMessage 
          ReadMessageBegin() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance valuetype Thrift.Protocol.TMessage Thrift.Protocol.TProtocol::ReadMessageBegin()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadMessageBegin

  .method public hidebysig virtual instance void 
          ReadMessageEnd() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance void Thrift.Protocol.TProtocol::ReadMessageEnd()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadMessageEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TStruct 
          ReadStructBegin() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance valuetype Thrift.Protocol.TStruct Thrift.Protocol.TProtocol::ReadStructBegin()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadStructBegin

  .method public hidebysig virtual instance void 
          ReadStructEnd() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance void Thrift.Protocol.TProtocol::ReadStructEnd()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadStructEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TField 
          ReadFieldBegin() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance valuetype Thrift.Protocol.TField Thrift.Protocol.TProtocol::ReadFieldBegin()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadFieldBegin

  .method public hidebysig virtual instance void 
          ReadFieldEnd() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance void Thrift.Protocol.TProtocol::ReadFieldEnd()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadFieldEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TMap 
          ReadMapBegin() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance valuetype Thrift.Protocol.TMap Thrift.Protocol.TProtocol::ReadMapBegin()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadMapBegin

  .method public hidebysig virtual instance void 
          ReadMapEnd() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance void Thrift.Protocol.TProtocol::ReadMapEnd()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadMapEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TList 
          ReadListBegin() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance valuetype Thrift.Protocol.TList Thrift.Protocol.TProtocol::ReadListBegin()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadListBegin

  .method public hidebysig virtual instance void 
          ReadListEnd() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance void Thrift.Protocol.TProtocol::ReadListEnd()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadListEnd

  .method public hidebysig virtual instance valuetype Thrift.Protocol.TSet 
          ReadSetBegin() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance valuetype Thrift.Protocol.TSet Thrift.Protocol.TProtocol::ReadSetBegin()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadSetBegin

  .method public hidebysig virtual instance void 
          ReadSetEnd() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance void Thrift.Protocol.TProtocol::ReadSetEnd()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadSetEnd

  .method public hidebysig virtual instance bool 
          ReadBool() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance bool Thrift.Protocol.TProtocol::ReadBool()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadBool

  .method public hidebysig virtual instance int8 
          ReadByte() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadByte

  .method public hidebysig virtual instance int16 
          ReadI16() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance int16 Thrift.Protocol.TProtocol::ReadI16()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadI16

  .method public hidebysig virtual instance int32 
          ReadI32() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance int32 Thrift.Protocol.TProtocol::ReadI32()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadI32

  .method public hidebysig virtual instance int64 
          ReadI64() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance int64 Thrift.Protocol.TProtocol::ReadI64()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadI64

  .method public hidebysig virtual instance float64 
          ReadDouble() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance float64 Thrift.Protocol.TProtocol::ReadDouble()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadDouble

  .method public hidebysig virtual instance string 
          ReadString() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance string Thrift.Protocol.TProtocol::ReadString()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadString

  .method public hidebysig virtual instance uint8[] 
          ReadBinary() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolDecorator::WrappedProtocol
    IL_0006:  callvirt   instance uint8[] Thrift.Protocol.TProtocol::ReadBinary()
    IL_000b:  ret
  } // end of method TProtocolDecorator::ReadBinary

} // end of class Thrift.Protocol.TProtocolDecorator

.class public auto ansi beforefieldinit Thrift.Protocol.TProtocolException
       extends Thrift.TException
{
  .field public static literal int32 UNKNOWN = int32(0x00000000)
  .field public static literal int32 INVALID_DATA = int32(0x00000001)
  .field public static literal int32 NEGATIVE_SIZE = int32(0x00000002)
  .field public static literal int32 SIZE_LIMIT = int32(0x00000003)
  .field public static literal int32 BAD_VERSION = int32(0x00000004)
  .field public static literal int32 NOT_IMPLEMENTED = int32(0x00000005)
  .field public static literal int32 DEPTH_LIMIT = int32(0x00000006)
  .field family int32 type_
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.TException::.ctor()
    IL_0006:  ret
  } // end of method TProtocolException::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(int32 'type') cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.TException::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      int32 Thrift.Protocol.TProtocolException::type_
    IL_000d:  ret
  } // end of method TProtocolException::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(int32 'type',
                               string message) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.2
    IL_0002:  call       instance void Thrift.TException::.ctor(string)
    IL_0007:  ldarg.0
    IL_0008:  ldarg.1
    IL_0009:  stfld      int32 Thrift.Protocol.TProtocolException::type_
    IL_000e:  ret
  } // end of method TProtocolException::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string message) cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  call       instance void Thrift.TException::.ctor(string)
    IL_0007:  ret
  } // end of method TProtocolException::.ctor

  .method public hidebysig instance int32 
          getType() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      int32 Thrift.Protocol.TProtocolException::type_
    IL_0006:  ret
  } // end of method TProtocolException::getType

} // end of class Thrift.Protocol.TProtocolException

.class interface public abstract auto ansi Thrift.Protocol.TProtocolFactory
{
  .method public hidebysig newslot abstract virtual 
          instance class Thrift.Protocol.TProtocol 
          GetProtocol(class Thrift.Transport.TTransport trans) cil managed
  {
  } // end of method TProtocolFactory::GetProtocol

} // end of class Thrift.Protocol.TProtocolFactory

.class public abstract auto ansi sealed beforefieldinit Thrift.Protocol.TProtocolUtil
       extends [mscorlib]System.Object
{
  .method public hidebysig static void  Skip(class Thrift.Protocol.TProtocol prot,
                                             valuetype Thrift.Protocol.TType 'type') cil managed
  {
    // Code size       425 (0x1a9)
    .maxstack  2
    .locals init (valuetype Thrift.Protocol.TField V_0,
             valuetype Thrift.Protocol.TMap V_1,
             int32 V_2,
             valuetype Thrift.Protocol.TSet V_3,
             int32 V_4,
             valuetype Thrift.Protocol.TList V_5,
             int32 V_6)
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance void Thrift.Protocol.TProtocol::IncrementRecursionDepth()
    .try
    {
      IL_0006:  ldarg.1
      IL_0007:  ldc.i4.2
      IL_0008:  sub
      IL_0009:  switch     ( 
                            IL_004b,
                            IL_0057,
                            IL_0087,
                            IL_019c,
                            IL_0063,
                            IL_019c,
                            IL_006f,
                            IL_019c,
                            IL_007b,
                            IL_0093,
                            IL_009f,
                            IL_00e1,
                            IL_0125,
                            IL_0160)
      IL_0046:  br         IL_019c

      IL_004b:  ldarg.0
      IL_004c:  callvirt   instance bool Thrift.Protocol.TProtocol::ReadBool()
      IL_0051:  pop
      IL_0052:  leave      IL_019c

      IL_0057:  ldarg.0
      IL_0058:  callvirt   instance int8 Thrift.Protocol.TProtocol::ReadByte()
      IL_005d:  pop
      IL_005e:  leave      IL_019c

      IL_0063:  ldarg.0
      IL_0064:  callvirt   instance int16 Thrift.Protocol.TProtocol::ReadI16()
      IL_0069:  pop
      IL_006a:  leave      IL_019c

      IL_006f:  ldarg.0
      IL_0070:  callvirt   instance int32 Thrift.Protocol.TProtocol::ReadI32()
      IL_0075:  pop
      IL_0076:  leave      IL_019c

      IL_007b:  ldarg.0
      IL_007c:  callvirt   instance int64 Thrift.Protocol.TProtocol::ReadI64()
      IL_0081:  pop
      IL_0082:  leave      IL_019c

      IL_0087:  ldarg.0
      IL_0088:  callvirt   instance float64 Thrift.Protocol.TProtocol::ReadDouble()
      IL_008d:  pop
      IL_008e:  leave      IL_019c

      IL_0093:  ldarg.0
      IL_0094:  callvirt   instance uint8[] Thrift.Protocol.TProtocol::ReadBinary()
      IL_0099:  pop
      IL_009a:  leave      IL_019c

      IL_009f:  ldarg.0
      IL_00a0:  callvirt   instance valuetype Thrift.Protocol.TStruct Thrift.Protocol.TProtocol::ReadStructBegin()
      IL_00a5:  pop
      IL_00a6:  ldarg.0
      IL_00a7:  callvirt   instance valuetype Thrift.Protocol.TField Thrift.Protocol.TProtocol::ReadFieldBegin()
      IL_00ac:  stloc.0
      IL_00ad:  ldloca.s   V_0
      IL_00af:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
      IL_00b4:  brtrue     IL_00be

      IL_00b9:  leave      IL_00d6

      IL_00be:  ldarg.0
      IL_00bf:  ldloca.s   V_0
      IL_00c1:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
      IL_00c6:  call       void Thrift.Protocol.TProtocolUtil::Skip(class Thrift.Protocol.TProtocol,
                                                                    valuetype Thrift.Protocol.TType)
      IL_00cb:  ldarg.0
      IL_00cc:  callvirt   instance void Thrift.Protocol.TProtocol::ReadFieldEnd()
      IL_00d1:  br         IL_00a6

      IL_00d6:  ldarg.0
      IL_00d7:  callvirt   instance void Thrift.Protocol.TProtocol::ReadStructEnd()
      IL_00dc:  leave      IL_019c

      IL_00e1:  ldarg.0
      IL_00e2:  callvirt   instance valuetype Thrift.Protocol.TMap Thrift.Protocol.TProtocol::ReadMapBegin()
      IL_00e7:  stloc.1
      IL_00e8:  ldc.i4.0
      IL_00e9:  stloc.2
      IL_00ea:  br         IL_010d

      IL_00ef:  ldarg.0
      IL_00f0:  ldloca.s   V_1
      IL_00f2:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::get_KeyType()
      IL_00f7:  call       void Thrift.Protocol.TProtocolUtil::Skip(class Thrift.Protocol.TProtocol,
                                                                    valuetype Thrift.Protocol.TType)
      IL_00fc:  ldarg.0
      IL_00fd:  ldloca.s   V_1
      IL_00ff:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TMap::get_ValueType()
      IL_0104:  call       void Thrift.Protocol.TProtocolUtil::Skip(class Thrift.Protocol.TProtocol,
                                                                    valuetype Thrift.Protocol.TType)
      IL_0109:  ldloc.2
      IL_010a:  ldc.i4.1
      IL_010b:  add
      IL_010c:  stloc.2
      IL_010d:  ldloc.2
      IL_010e:  ldloca.s   V_1
      IL_0110:  call       instance int32 Thrift.Protocol.TMap::get_Count()
      IL_0115:  blt        IL_00ef

      IL_011a:  ldarg.0
      IL_011b:  callvirt   instance void Thrift.Protocol.TProtocol::ReadMapEnd()
      IL_0120:  leave      IL_019c

      IL_0125:  ldarg.0
      IL_0126:  callvirt   instance valuetype Thrift.Protocol.TSet Thrift.Protocol.TProtocol::ReadSetBegin()
      IL_012b:  stloc.3
      IL_012c:  ldc.i4.0
      IL_012d:  stloc.s    V_4
      IL_012f:  br         IL_0147

      IL_0134:  ldarg.0
      IL_0135:  ldloca.s   V_3
      IL_0137:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TSet::get_ElementType()
      IL_013c:  call       void Thrift.Protocol.TProtocolUtil::Skip(class Thrift.Protocol.TProtocol,
                                                                    valuetype Thrift.Protocol.TType)
      IL_0141:  ldloc.s    V_4
      IL_0143:  ldc.i4.1
      IL_0144:  add
      IL_0145:  stloc.s    V_4
      IL_0147:  ldloc.s    V_4
      IL_0149:  ldloca.s   V_3
      IL_014b:  call       instance int32 Thrift.Protocol.TSet::get_Count()
      IL_0150:  blt        IL_0134

      IL_0155:  ldarg.0
      IL_0156:  callvirt   instance void Thrift.Protocol.TProtocol::ReadSetEnd()
      IL_015b:  leave      IL_019c

      IL_0160:  ldarg.0
      IL_0161:  callvirt   instance valuetype Thrift.Protocol.TList Thrift.Protocol.TProtocol::ReadListBegin()
      IL_0166:  stloc.s    V_5
      IL_0168:  ldc.i4.0
      IL_0169:  stloc.s    V_6
      IL_016b:  br         IL_0183

      IL_0170:  ldarg.0
      IL_0171:  ldloca.s   V_5
      IL_0173:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TList::get_ElementType()
      IL_0178:  call       void Thrift.Protocol.TProtocolUtil::Skip(class Thrift.Protocol.TProtocol,
                                                                    valuetype Thrift.Protocol.TType)
      IL_017d:  ldloc.s    V_6
      IL_017f:  ldc.i4.1
      IL_0180:  add
      IL_0181:  stloc.s    V_6
      IL_0183:  ldloc.s    V_6
      IL_0185:  ldloca.s   V_5
      IL_0187:  call       instance int32 Thrift.Protocol.TList::get_Count()
      IL_018c:  blt        IL_0170

      IL_0191:  ldarg.0
      IL_0192:  callvirt   instance void Thrift.Protocol.TProtocol::ReadListEnd()
      IL_0197:  leave      IL_019c

      IL_019c:  leave      IL_01a8

    }  // end .try
    finally
    {
      IL_01a1:  ldarg.0
      IL_01a2:  callvirt   instance void Thrift.Protocol.TProtocol::DecrementRecursionDepth()
      IL_01a7:  endfinally
    }  // end handler
    IL_01a8:  ret
  } // end of method TProtocolUtil::Skip

} // end of class Thrift.Protocol.TProtocolUtil

.class public sequential ansi sealed beforefieldinit Thrift.Protocol.TSet
       extends [mscorlib]System.ValueType
{
  .field private valuetype Thrift.Protocol.TType elementType
  .field private int32 count
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype Thrift.Protocol.TType elementType,
                               int32 count) cil managed
  {
    // Code size       22 (0x16)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  initobj    Thrift.Protocol.TSet
    IL_0007:  ldarg.0
    IL_0008:  ldarg.1
    IL_0009:  stfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TSet::elementType
    IL_000e:  ldarg.0
    IL_000f:  ldarg.2
    IL_0010:  stfld      int32 Thrift.Protocol.TSet::count
    IL_0015:  ret
  } // end of method TSet::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype Thrift.Protocol.TList list) cil managed
  {
    // Code size       21 (0x15)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarga.s   list
    IL_0003:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TList::get_ElementType()
    IL_0008:  ldarga.s   list
    IL_000a:  call       instance int32 Thrift.Protocol.TList::get_Count()
    IL_000f:  call       instance void Thrift.Protocol.TSet::.ctor(valuetype Thrift.Protocol.TType,
                                                                   int32)
    IL_0014:  ret
  } // end of method TSet::.ctor

  .method public hidebysig specialname instance valuetype Thrift.Protocol.TType 
          get_ElementType() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TSet::elementType
    IL_0006:  ret
  } // end of method TSet::get_ElementType

  .method public hidebysig specialname instance void 
          set_ElementType(valuetype Thrift.Protocol.TType 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      valuetype Thrift.Protocol.TType Thrift.Protocol.TSet::elementType
    IL_0007:  ret
  } // end of method TSet::set_ElementType

  .method public hidebysig specialname instance int32 
          get_Count() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      int32 Thrift.Protocol.TSet::count
    IL_0006:  ret
  } // end of method TSet::get_Count

  .method public hidebysig specialname instance void 
          set_Count(int32 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      int32 Thrift.Protocol.TSet::count
    IL_0007:  ret
  } // end of method TSet::set_Count

  .property instance valuetype Thrift.Protocol.TType
          ElementType()
  {
    .get instance valuetype Thrift.Protocol.TType Thrift.Protocol.TSet::get_ElementType()
    .set instance void Thrift.Protocol.TSet::set_ElementType(valuetype Thrift.Protocol.TType)
  } // end of property TSet::ElementType
  .property instance int32 Count()
  {
    .get instance int32 Thrift.Protocol.TSet::get_Count()
    .set instance void Thrift.Protocol.TSet::set_Count(int32)
  } // end of property TSet::Count
} // end of class Thrift.Protocol.TSet

.class public sequential ansi sealed beforefieldinit Thrift.Protocol.TStruct
       extends [mscorlib]System.ValueType
{
  .field private string name
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string name) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  initobj    Thrift.Protocol.TStruct
    IL_0007:  ldarg.0
    IL_0008:  ldarg.1
    IL_0009:  stfld      string Thrift.Protocol.TStruct::name
    IL_000e:  ret
  } // end of method TStruct::.ctor

  .method public hidebysig specialname instance string 
          get_Name() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      string Thrift.Protocol.TStruct::name
    IL_0006:  ret
  } // end of method TStruct::get_Name

  .method public hidebysig specialname instance void 
          set_Name(string 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      string Thrift.Protocol.TStruct::name
    IL_0007:  ret
  } // end of method TStruct::set_Name

  .property instance string Name()
  {
    .get instance string Thrift.Protocol.TStruct::get_Name()
    .set instance void Thrift.Protocol.TStruct::set_Name(string)
  } // end of property TStruct::Name
} // end of class Thrift.Protocol.TStruct

.class public auto ansi sealed Thrift.Protocol.TType
       extends [mscorlib]System.Enum
{
  .field public specialname rtspecialname uint8 value__
  .field public static literal valuetype Thrift.Protocol.TType Stop = uint8(0x00)
  .field public static literal valuetype Thrift.Protocol.TType Void = uint8(0x01)
  .field public static literal valuetype Thrift.Protocol.TType Bool = uint8(0x02)
  .field public static literal valuetype Thrift.Protocol.TType Byte = uint8(0x03)
  .field public static literal valuetype Thrift.Protocol.TType Double = uint8(0x04)
  .field public static literal valuetype Thrift.Protocol.TType I16 = uint8(0x06)
  .field public static literal valuetype Thrift.Protocol.TType I32 = uint8(0x08)
  .field public static literal valuetype Thrift.Protocol.TType I64 = uint8(0x0A)
  .field public static literal valuetype Thrift.Protocol.TType String = uint8(0x0B)
  .field public static literal valuetype Thrift.Protocol.TType Struct = uint8(0x0C)
  .field public static literal valuetype Thrift.Protocol.TType Map = uint8(0x0D)
  .field public static literal valuetype Thrift.Protocol.TType Set = uint8(0x0E)
  .field public static literal valuetype Thrift.Protocol.TType List = uint8(0x0F)
} // end of class Thrift.Protocol.TType

.class public auto ansi beforefieldinit Thrift.Server.TThreadedServer
       extends Thrift.Server.TServer
{
  .field private static literal int32 DEFAULT_MAX_THREADS = int32(0x00000064)
  .field private bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) stop
  .field private initonly int32 maxThreads
  .field private class [System]System.Collections.Generic.Queue`1<class Thrift.Transport.TTransport> clientQueue
  .field private class Thrift.Collections.THashSet`1<class [mscorlib]System.Threading.Thread> clientThreads
  .field private object clientLock
  .field private class [mscorlib]System.Threading.Thread workerThread
  .field private static class Thrift.Server.TServer/LogDelegate '<>f__mg$cache0'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class Thrift.Server.TServer/LogDelegate '<>f__mg$cache1'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method public hidebysig specialname instance int32 
          get_ClientThreadsCount() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Collections.THashSet`1<class [mscorlib]System.Threading.Thread> Thrift.Server.TThreadedServer::clientThreads
    IL_0006:  callvirt   instance int32 class Thrift.Collections.THashSet`1<class [mscorlib]System.Threading.Thread>::get_Count()
    IL_000b:  ret
  } // end of method TThreadedServer::get_ClientThreadsCount

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport) cil managed
  {
    // Code size       60 (0x3c)
    .maxstack  10
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_0008:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_000d:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0012:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0017:  ldc.i4.s   100
    IL_0019:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TThreadedServer::'<>f__mg$cache0'
    IL_001e:  brtrue.s   IL_0031

    IL_0020:  ldnull
    IL_0021:  ldftn      void Thrift.Server.TServer::DefaultLogDelegate(string)
    IL_0027:  newobj     instance void Thrift.Server.TServer/LogDelegate::.ctor(object,
                                                                                native int)
    IL_002c:  stsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TThreadedServer::'<>f__mg$cache0'
    IL_0031:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TThreadedServer::'<>f__mg$cache0'
    IL_0036:  call       instance void Thrift.Server.TThreadedServer::.ctor(class Thrift.TProcessor,
                                                                            class Thrift.Transport.TServerTransport,
                                                                            class Thrift.Transport.TTransportFactory,
                                                                            class Thrift.Transport.TTransportFactory,
                                                                            class Thrift.Protocol.TProtocolFactory,
                                                                            class Thrift.Protocol.TProtocolFactory,
                                                                            int32,
                                                                            class Thrift.Server.TServer/LogDelegate)
    IL_003b:  ret
  } // end of method TThreadedServer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport,
                               class Thrift.Server.TServer/LogDelegate logDelegate) cil managed
  {
    // Code size       32 (0x20)
    .maxstack  9
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_0008:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_000d:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0012:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0017:  ldc.i4.s   100
    IL_0019:  ldarg.3
    IL_001a:  call       instance void Thrift.Server.TThreadedServer::.ctor(class Thrift.TProcessor,
                                                                            class Thrift.Transport.TServerTransport,
                                                                            class Thrift.Transport.TTransportFactory,
                                                                            class Thrift.Transport.TTransportFactory,
                                                                            class Thrift.Protocol.TProtocolFactory,
                                                                            class Thrift.Protocol.TProtocolFactory,
                                                                            int32,
                                                                            class Thrift.Server.TServer/LogDelegate)
    IL_001f:  ret
  } // end of method TThreadedServer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport,
                               class Thrift.Transport.TTransportFactory transportFactory,
                               class Thrift.Protocol.TProtocolFactory protocolFactory) cil managed
  {
    // Code size       46 (0x2e)
    .maxstack  10
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldarg.3
    IL_0004:  ldarg.3
    IL_0005:  ldarg.s    protocolFactory
    IL_0007:  ldarg.s    protocolFactory
    IL_0009:  ldc.i4.s   100
    IL_000b:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TThreadedServer::'<>f__mg$cache1'
    IL_0010:  brtrue.s   IL_0023

    IL_0012:  ldnull
    IL_0013:  ldftn      void Thrift.Server.TServer::DefaultLogDelegate(string)
    IL_0019:  newobj     instance void Thrift.Server.TServer/LogDelegate::.ctor(object,
                                                                                native int)
    IL_001e:  stsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TThreadedServer::'<>f__mg$cache1'
    IL_0023:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TThreadedServer::'<>f__mg$cache1'
    IL_0028:  call       instance void Thrift.Server.TThreadedServer::.ctor(class Thrift.TProcessor,
                                                                            class Thrift.Transport.TServerTransport,
                                                                            class Thrift.Transport.TTransportFactory,
                                                                            class Thrift.Transport.TTransportFactory,
                                                                            class Thrift.Protocol.TProtocolFactory,
                                                                            class Thrift.Protocol.TProtocolFactory,
                                                                            int32,
                                                                            class Thrift.Server.TServer/LogDelegate)
    IL_002d:  ret
  } // end of method TThreadedServer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport,
                               class Thrift.Transport.TTransportFactory inputTransportFactory,
                               class Thrift.Transport.TTransportFactory outputTransportFactory,
                               class Thrift.Protocol.TProtocolFactory inputProtocolFactory,
                               class Thrift.Protocol.TProtocolFactory outputProtocolFactory,
                               int32 maxThreads,
                               class Thrift.Server.TServer/LogDelegate logDel) cil managed
  {
    // Code size       59 (0x3b)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldarg.3
    IL_0004:  ldarg.s    outputTransportFactory
    IL_0006:  ldarg.s    inputProtocolFactory
    IL_0008:  ldarg.s    outputProtocolFactory
    IL_000a:  ldarg.s    logDel
    IL_000c:  call       instance void Thrift.Server.TServer::.ctor(class Thrift.TProcessor,
                                                                    class Thrift.Transport.TServerTransport,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Server.TServer/LogDelegate)
    IL_0011:  ldarg.0
    IL_0012:  ldarg.s    maxThreads
    IL_0014:  stfld      int32 Thrift.Server.TThreadedServer::maxThreads
    IL_0019:  ldarg.0
    IL_001a:  newobj     instance void class [System]System.Collections.Generic.Queue`1<class Thrift.Transport.TTransport>::.ctor()
    IL_001f:  stfld      class [System]System.Collections.Generic.Queue`1<class Thrift.Transport.TTransport> Thrift.Server.TThreadedServer::clientQueue
    IL_0024:  ldarg.0
    IL_0025:  newobj     instance void [mscorlib]System.Object::.ctor()
    IL_002a:  stfld      object Thrift.Server.TThreadedServer::clientLock
    IL_002f:  ldarg.0
    IL_0030:  newobj     instance void class Thrift.Collections.THashSet`1<class [mscorlib]System.Threading.Thread>::.ctor()
    IL_0035:  stfld      class Thrift.Collections.THashSet`1<class [mscorlib]System.Threading.Thread> Thrift.Server.TThreadedServer::clientThreads
    IL_003a:  ret
  } // end of method TThreadedServer::.ctor

  .method public hidebysig virtual instance void 
          Serve() cil managed
  {
    // Code size       314 (0x13a)
    .maxstack  3
    .locals init (class Thrift.Transport.TTransportException V_0,
             int32 V_1,
             class Thrift.Transport.TTransport V_2,
             object V_3,
             class Thrift.Transport.TTransportException V_4,
             class Thrift.Transport.TTransportException V_5)
    .try
    {
      IL_0000:  ldarg.0
      IL_0001:  ldarg.0
      IL_0002:  ldftn      instance void Thrift.Server.TThreadedServer::Execute()
      IL_0008:  newobj     instance void [mscorlib]System.Threading.ThreadStart::.ctor(object,
                                                                                       native int)
      IL_000d:  newobj     instance void [mscorlib]System.Threading.Thread::.ctor(class [mscorlib]System.Threading.ThreadStart)
      IL_0012:  stfld      class [mscorlib]System.Threading.Thread Thrift.Server.TThreadedServer::workerThread
      IL_0017:  ldarg.0
      IL_0018:  ldfld      class [mscorlib]System.Threading.Thread Thrift.Server.TThreadedServer::workerThread
      IL_001d:  callvirt   instance void [mscorlib]System.Threading.Thread::Start()
      IL_0022:  ldarg.0
      IL_0023:  ldfld      class Thrift.Transport.TServerTransport Thrift.Server.TServer::serverTransport
      IL_0028:  callvirt   instance void Thrift.Transport.TServerTransport::Listen()
      IL_002d:  leave      IL_004e

    }  // end .try
    catch Thrift.Transport.TTransportException 
    {
      IL_0032:  stloc.0
      IL_0033:  ldarg.0
      IL_0034:  call       instance class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::get_logDelegate()
      IL_0039:  ldstr      "Error, could not listen on ServerTransport: "
      IL_003e:  ldloc.0
      IL_003f:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_0044:  callvirt   instance void Thrift.Server.TServer/LogDelegate::Invoke(string)
      IL_0049:  leave      IL_0139

    }  // end handler
    IL_004e:  ldarg.0
    IL_004f:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_0054:  brfalse    IL_0064

    IL_0059:  ldarg.0
    IL_005a:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_005f:  callvirt   instance void Thrift.Server.TServerEventHandler::preServe()
    IL_0064:  br         IL_00e3

    IL_0069:  ldc.i4.0
    IL_006a:  stloc.1
    .try
    {
      IL_006b:  ldarg.0
      IL_006c:  ldfld      class Thrift.Transport.TServerTransport Thrift.Server.TServer::serverTransport
      IL_0071:  callvirt   instance class Thrift.Transport.TTransport Thrift.Transport.TServerTransport::Accept()
      IL_0076:  stloc.2
      IL_0077:  ldarg.0
      IL_0078:  ldfld      object Thrift.Server.TThreadedServer::clientLock
      IL_007d:  stloc.3
      IL_007e:  ldloc.3
      IL_007f:  call       void [mscorlib]System.Threading.Monitor::Enter(object)
      .try
      {
        IL_0084:  ldarg.0
        IL_0085:  ldfld      class [System]System.Collections.Generic.Queue`1<class Thrift.Transport.TTransport> Thrift.Server.TThreadedServer::clientQueue
        IL_008a:  ldloc.2
        IL_008b:  callvirt   instance void class [System]System.Collections.Generic.Queue`1<class Thrift.Transport.TTransport>::Enqueue(!0)
        IL_0090:  ldarg.0
        IL_0091:  ldfld      object Thrift.Server.TThreadedServer::clientLock
        IL_0096:  call       void [mscorlib]System.Threading.Monitor::Pulse(object)
        IL_009b:  leave      IL_00a7

      }  // end .try
      finally
      {
        IL_00a0:  ldloc.3
        IL_00a1:  call       void [mscorlib]System.Threading.Monitor::Exit(object)
        IL_00a6:  endfinally
      }  // end handler
      IL_00a7:  leave      IL_00e3

    }  // end .try
    catch Thrift.Transport.TTransportException 
    {
      IL_00ac:  stloc.s    V_4
      IL_00ae:  ldarg.0
      IL_00af:  volatile.
      IL_00b1:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Server.TThreadedServer::stop
      IL_00b6:  brfalse    IL_00c8

      IL_00bb:  ldloc.s    V_4
      IL_00bd:  callvirt   instance valuetype Thrift.Transport.TTransportException/ExceptionType Thrift.Transport.TTransportException::get_Type()
      IL_00c2:  ldc.i4.5
      IL_00c3:  beq        IL_00de

      IL_00c8:  ldloc.1
      IL_00c9:  ldc.i4.1
      IL_00ca:  add
      IL_00cb:  stloc.1
      IL_00cc:  ldarg.0
      IL_00cd:  call       instance class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::get_logDelegate()
      IL_00d2:  ldloc.s    V_4
      IL_00d4:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_00d9:  callvirt   instance void Thrift.Server.TServer/LogDelegate::Invoke(string)
      IL_00de:  leave      IL_00e3

    }  // end handler
    IL_00e3:  ldarg.0
    IL_00e4:  volatile.
    IL_00e6:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Server.TThreadedServer::stop
    IL_00eb:  brfalse    IL_0069

    IL_00f0:  ldarg.0
    IL_00f1:  volatile.
    IL_00f3:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Server.TThreadedServer::stop
    IL_00f8:  brfalse    IL_0139

    .try
    {
      IL_00fd:  ldarg.0
      IL_00fe:  ldfld      class Thrift.Transport.TServerTransport Thrift.Server.TServer::serverTransport
      IL_0103:  callvirt   instance void Thrift.Transport.TServerTransport::Close()
      IL_0108:  leave      IL_0130

    }  // end .try
    catch Thrift.Transport.TTransportException 
    {
      IL_010d:  stloc.s    V_5
      IL_010f:  ldarg.0
      IL_0110:  call       instance class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::get_logDelegate()
      IL_0115:  ldstr      "TServeTransport failed on close: "
      IL_011a:  ldloc.s    V_5
      IL_011c:  callvirt   instance string [mscorlib]System.Exception::get_Message()
      IL_0121:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_0126:  callvirt   instance void Thrift.Server.TServer/LogDelegate::Invoke(string)
      IL_012b:  leave      IL_0130

    }  // end handler
    IL_0130:  ldarg.0
    IL_0131:  ldc.i4.0
    IL_0132:  volatile.
    IL_0134:  stfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Server.TThreadedServer::stop
    IL_0139:  ret
  } // end of method TThreadedServer::Serve

  .method private hidebysig instance void 
          Execute() cil managed
  {
    // Code size       165 (0xa5)
    .maxstack  2
    .locals init (class Thrift.Transport.TTransport V_0,
             class [mscorlib]System.Threading.Thread V_1,
             object V_2)
    IL_0000:  br         IL_0097

    IL_0005:  ldarg.0
    IL_0006:  ldfld      object Thrift.Server.TThreadedServer::clientLock
    IL_000b:  stloc.2
    IL_000c:  ldloc.2
    IL_000d:  call       void [mscorlib]System.Threading.Monitor::Enter(object)
    .try
    {
      IL_0012:  br         IL_0023

      IL_0017:  ldarg.0
      IL_0018:  ldfld      object Thrift.Server.TThreadedServer::clientLock
      IL_001d:  call       bool [mscorlib]System.Threading.Monitor::Wait(object)
      IL_0022:  pop
      IL_0023:  ldarg.0
      IL_0024:  ldfld      class Thrift.Collections.THashSet`1<class [mscorlib]System.Threading.Thread> Thrift.Server.TThreadedServer::clientThreads
      IL_0029:  callvirt   instance int32 class Thrift.Collections.THashSet`1<class [mscorlib]System.Threading.Thread>::get_Count()
      IL_002e:  ldarg.0
      IL_002f:  ldfld      int32 Thrift.Server.TThreadedServer::maxThreads
      IL_0034:  bge        IL_0017

      IL_0039:  br         IL_004a

      IL_003e:  ldarg.0
      IL_003f:  ldfld      object Thrift.Server.TThreadedServer::clientLock
      IL_0044:  call       bool [mscorlib]System.Threading.Monitor::Wait(object)
      IL_0049:  pop
      IL_004a:  ldarg.0
      IL_004b:  ldfld      class [System]System.Collections.Generic.Queue`1<class Thrift.Transport.TTransport> Thrift.Server.TThreadedServer::clientQueue
      IL_0050:  callvirt   instance int32 class [System]System.Collections.Generic.Queue`1<class Thrift.Transport.TTransport>::get_Count()
      IL_0055:  brfalse    IL_003e

      IL_005a:  ldarg.0
      IL_005b:  ldfld      class [System]System.Collections.Generic.Queue`1<class Thrift.Transport.TTransport> Thrift.Server.TThreadedServer::clientQueue
      IL_0060:  callvirt   instance !0 class [System]System.Collections.Generic.Queue`1<class Thrift.Transport.TTransport>::Dequeue()
      IL_0065:  stloc.0
      IL_0066:  ldarg.0
      IL_0067:  ldftn      instance void Thrift.Server.TThreadedServer::ClientWorker(object)
      IL_006d:  newobj     instance void [mscorlib]System.Threading.ParameterizedThreadStart::.ctor(object,
                                                                                                    native int)
      IL_0072:  newobj     instance void [mscorlib]System.Threading.Thread::.ctor(class [mscorlib]System.Threading.ParameterizedThreadStart)
      IL_0077:  stloc.1
      IL_0078:  ldarg.0
      IL_0079:  ldfld      class Thrift.Collections.THashSet`1<class [mscorlib]System.Threading.Thread> Thrift.Server.TThreadedServer::clientThreads
      IL_007e:  ldloc.1
      IL_007f:  callvirt   instance void class Thrift.Collections.THashSet`1<class [mscorlib]System.Threading.Thread>::Add(!0)
      IL_0084:  leave      IL_0090

    }  // end .try
    finally
    {
      IL_0089:  ldloc.2
      IL_008a:  call       void [mscorlib]System.Threading.Monitor::Exit(object)
      IL_008f:  endfinally
    }  // end handler
    IL_0090:  ldloc.1
    IL_0091:  ldloc.0
    IL_0092:  callvirt   instance void [mscorlib]System.Threading.Thread::Start(object)
    IL_0097:  ldarg.0
    IL_0098:  volatile.
    IL_009a:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Server.TThreadedServer::stop
    IL_009f:  brfalse    IL_0005

    IL_00a4:  ret
  } // end of method TThreadedServer::Execute

  .method private hidebysig instance void 
          ClientWorker(object context) cil managed
  {
    // Code size       353 (0x161)
    .maxstack  4
    .locals init (class Thrift.Transport.TTransport V_0,
             class Thrift.Transport.TTransport V_1,
             class Thrift.Transport.TTransport V_2,
             class Thrift.Protocol.TProtocol V_3,
             class Thrift.Protocol.TProtocol V_4,
             object V_5,
             class Thrift.Transport.TTransport V_6,
             class Thrift.Transport.TTransport V_7,
             class [mscorlib]System.Exception V_8,
             object V_9)
    IL_0000:  ldarg.1
    IL_0001:  castclass  Thrift.Transport.TTransport
    IL_0006:  stloc.0
    IL_0007:  ldnull
    IL_0008:  stloc.1
    IL_0009:  ldnull
    IL_000a:  stloc.2
    IL_000b:  ldnull
    IL_000c:  stloc.3
    IL_000d:  ldnull
    IL_000e:  stloc.s    V_4
    IL_0010:  ldnull
    IL_0011:  stloc.s    V_5
    .try
    {
      IL_0013:  ldarg.0
      IL_0014:  ldfld      class Thrift.Transport.TTransportFactory Thrift.Server.TServer::inputTransportFactory
      IL_0019:  ldloc.0
      IL_001a:  callvirt   instance class Thrift.Transport.TTransport Thrift.Transport.TTransportFactory::GetTransport(class Thrift.Transport.TTransport)
      IL_001f:  dup
      IL_0020:  stloc.1
      IL_0021:  stloc.s    V_6
      .try
      {
        IL_0023:  ldarg.0
        IL_0024:  ldfld      class Thrift.Transport.TTransportFactory Thrift.Server.TServer::outputTransportFactory
        IL_0029:  ldloc.0
        IL_002a:  callvirt   instance class Thrift.Transport.TTransport Thrift.Transport.TTransportFactory::GetTransport(class Thrift.Transport.TTransport)
        IL_002f:  dup
        IL_0030:  stloc.2
        IL_0031:  stloc.s    V_7
        .try
        {
          IL_0033:  ldarg.0
          IL_0034:  ldfld      class Thrift.Protocol.TProtocolFactory Thrift.Server.TServer::inputProtocolFactory
          IL_0039:  ldloc.1
          IL_003a:  callvirt   instance class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolFactory::GetProtocol(class Thrift.Transport.TTransport)
          IL_003f:  stloc.3
          IL_0040:  ldarg.0
          IL_0041:  ldfld      class Thrift.Protocol.TProtocolFactory Thrift.Server.TServer::outputProtocolFactory
          IL_0046:  ldloc.2
          IL_0047:  callvirt   instance class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolFactory::GetProtocol(class Thrift.Transport.TTransport)
          IL_004c:  stloc.s    V_4
          IL_004e:  ldarg.0
          IL_004f:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
          IL_0054:  brfalse    IL_0069

          IL_0059:  ldarg.0
          IL_005a:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
          IL_005f:  ldloc.3
          IL_0060:  ldloc.s    V_4
          IL_0062:  callvirt   instance object Thrift.Server.TServerEventHandler::createContext(class Thrift.Protocol.TProtocol,
                                                                                                class Thrift.Protocol.TProtocol)
          IL_0067:  stloc.s    V_5
          IL_0069:  br         IL_00af

          IL_006e:  ldloc.1
          IL_006f:  callvirt   instance bool Thrift.Transport.TTransport::Peek()
          IL_0074:  brtrue     IL_007e

          IL_0079:  leave      IL_00bc

          IL_007e:  ldarg.0
          IL_007f:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
          IL_0084:  brfalse    IL_0097

          IL_0089:  ldarg.0
          IL_008a:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
          IL_008f:  ldloc.s    V_5
          IL_0091:  ldloc.1
          IL_0092:  callvirt   instance void Thrift.Server.TServerEventHandler::processContext(object,
                                                                                               class Thrift.Transport.TTransport)
          IL_0097:  ldarg.0
          IL_0098:  ldfld      class Thrift.TProcessor Thrift.Server.TServer::processor
          IL_009d:  ldloc.3
          IL_009e:  ldloc.s    V_4
          IL_00a0:  callvirt   instance bool Thrift.TProcessor::Process(class Thrift.Protocol.TProtocol,
                                                                        class Thrift.Protocol.TProtocol)
          IL_00a5:  brtrue     IL_00af

          IL_00aa:  leave      IL_00bc

          IL_00af:  ldarg.0
          IL_00b0:  volatile.
          IL_00b2:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Server.TThreadedServer::stop
          IL_00b7:  brfalse    IL_006e

          IL_00bc:  leave      IL_00d0

        }  // end .try
        finally
        {
          IL_00c1:  ldloc.s    V_7
          IL_00c3:  brfalse    IL_00cf

          IL_00c8:  ldloc.s    V_7
          IL_00ca:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
          IL_00cf:  endfinally
        }  // end handler
        IL_00d0:  leave      IL_00e4

      }  // end .try
      finally
      {
        IL_00d5:  ldloc.s    V_6
        IL_00d7:  brfalse    IL_00e3

        IL_00dc:  ldloc.s    V_6
        IL_00de:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_00e3:  endfinally
      }  // end handler
      IL_00e4:  leave      IL_010d

    }  // end .try
    catch Thrift.Transport.TTransportException 
    {
      IL_00e9:  pop
      IL_00ea:  leave      IL_010d

    }  // end handler
    catch [mscorlib]System.Exception 
    {
      IL_00ef:  stloc.s    V_8
      IL_00f1:  ldarg.0
      IL_00f2:  call       instance class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::get_logDelegate()
      IL_00f7:  ldstr      "Error: "
      IL_00fc:  ldloc.s    V_8
      IL_00fe:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_0103:  callvirt   instance void Thrift.Server.TServer/LogDelegate::Invoke(string)
      IL_0108:  leave      IL_010d

    }  // end handler
    IL_010d:  ldarg.0
    IL_010e:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_0113:  brfalse    IL_0128

    IL_0118:  ldarg.0
    IL_0119:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_011e:  ldloc.s    V_5
    IL_0120:  ldloc.3
    IL_0121:  ldloc.s    V_4
    IL_0123:  callvirt   instance void Thrift.Server.TServerEventHandler::deleteContext(object,
                                                                                        class Thrift.Protocol.TProtocol,
                                                                                        class Thrift.Protocol.TProtocol)
    IL_0128:  ldarg.0
    IL_0129:  ldfld      object Thrift.Server.TThreadedServer::clientLock
    IL_012e:  stloc.s    V_9
    IL_0130:  ldloc.s    V_9
    IL_0132:  call       void [mscorlib]System.Threading.Monitor::Enter(object)
    .try
    {
      IL_0137:  ldarg.0
      IL_0138:  ldfld      class Thrift.Collections.THashSet`1<class [mscorlib]System.Threading.Thread> Thrift.Server.TThreadedServer::clientThreads
      IL_013d:  call       class [mscorlib]System.Threading.Thread [mscorlib]System.Threading.Thread::get_CurrentThread()
      IL_0142:  callvirt   instance bool class Thrift.Collections.THashSet`1<class [mscorlib]System.Threading.Thread>::Remove(!0)
      IL_0147:  pop
      IL_0148:  ldarg.0
      IL_0149:  ldfld      object Thrift.Server.TThreadedServer::clientLock
      IL_014e:  call       void [mscorlib]System.Threading.Monitor::Pulse(object)
      IL_0153:  leave      IL_0160

    }  // end .try
    finally
    {
      IL_0158:  ldloc.s    V_9
      IL_015a:  call       void [mscorlib]System.Threading.Monitor::Exit(object)
      IL_015f:  endfinally
    }  // end handler
    IL_0160:  ret
  } // end of method TThreadedServer::ClientWorker

  .method public hidebysig virtual instance void 
          Stop() cil managed
  {
    // Code size       103 (0x67)
    .maxstack  2
    .locals init (class [mscorlib]System.Threading.Thread V_0,
             class [mscorlib]System.Collections.IEnumerator V_1,
             class [mscorlib]System.IDisposable V_2)
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.1
    IL_0002:  volatile.
    IL_0004:  stfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Server.TThreadedServer::stop
    IL_0009:  ldarg.0
    IL_000a:  ldfld      class Thrift.Transport.TServerTransport Thrift.Server.TServer::serverTransport
    IL_000f:  callvirt   instance void Thrift.Transport.TServerTransport::Close()
    IL_0014:  ldarg.0
    IL_0015:  ldfld      class [mscorlib]System.Threading.Thread Thrift.Server.TThreadedServer::workerThread
    IL_001a:  callvirt   instance void [mscorlib]System.Threading.Thread::Abort()
    IL_001f:  ldarg.0
    IL_0020:  ldfld      class Thrift.Collections.THashSet`1<class [mscorlib]System.Threading.Thread> Thrift.Server.TThreadedServer::clientThreads
    IL_0025:  callvirt   instance class [mscorlib]System.Collections.IEnumerator class Thrift.Collections.THashSet`1<class [mscorlib]System.Threading.Thread>::GetEnumerator()
    IL_002a:  stloc.1
    .try
    {
      IL_002b:  br         IL_0042

      IL_0030:  ldloc.1
      IL_0031:  callvirt   instance object [mscorlib]System.Collections.IEnumerator::get_Current()
      IL_0036:  castclass  [mscorlib]System.Threading.Thread
      IL_003b:  stloc.0
      IL_003c:  ldloc.0
      IL_003d:  callvirt   instance void [mscorlib]System.Threading.Thread::Abort()
      IL_0042:  ldloc.1
      IL_0043:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
      IL_0048:  brtrue     IL_0030

      IL_004d:  leave      IL_0066

    }  // end .try
    finally
    {
      IL_0052:  ldloc.1
      IL_0053:  isinst     [mscorlib]System.IDisposable
      IL_0058:  dup
      IL_0059:  stloc.2
      IL_005a:  brfalse    IL_0065

      IL_005f:  ldloc.2
      IL_0060:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_0065:  endfinally
    }  // end handler
    IL_0066:  ret
  } // end of method TThreadedServer::Stop

  .property instance int32 ClientThreadsCount()
  {
    .get instance int32 Thrift.Server.TThreadedServer::get_ClientThreadsCount()
  } // end of property TThreadedServer::ClientThreadsCount
} // end of class Thrift.Server.TThreadedServer

.class public abstract auto ansi beforefieldinit Thrift.Server.TServer
       extends [mscorlib]System.Object
{
  .class auto ansi sealed nested public LogDelegate
         extends [mscorlib]System.MulticastDelegate
  {
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor(object 'object',
                                 native int 'method') runtime managed
    {
    } // end of method LogDelegate::.ctor

    .method public hidebysig newslot virtual 
            instance void  Invoke(string str) runtime managed
    {
    } // end of method LogDelegate::Invoke

    .method public hidebysig newslot virtual 
            instance class [mscorlib]System.IAsyncResult 
            BeginInvoke(string str,
                        class [mscorlib]System.AsyncCallback callback,
                        object 'object') runtime managed
    {
    } // end of method LogDelegate::BeginInvoke

    .method public hidebysig newslot virtual 
            instance void  EndInvoke(class [mscorlib]System.IAsyncResult result) runtime managed
    {
    } // end of method LogDelegate::EndInvoke

  } // end of class LogDelegate

  .field family class Thrift.TProcessor processor
  .field family class Thrift.Transport.TServerTransport serverTransport
  .field family class Thrift.Transport.TTransportFactory inputTransportFactory
  .field family class Thrift.Transport.TTransportFactory outputTransportFactory
  .field family class Thrift.Protocol.TProtocolFactory inputProtocolFactory
  .field family class Thrift.Protocol.TProtocolFactory outputProtocolFactory
  .field family class Thrift.Server.TServerEventHandler serverEventHandler
  .field private class Thrift.Server.TServer/LogDelegate _logDelegate
  .field private static class Thrift.Server.TServer/LogDelegate '<>f__mg$cache0'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class Thrift.Server.TServer/LogDelegate '<>f__mg$cache1'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class Thrift.Server.TServer/LogDelegate '<>f__mg$cache2'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class Thrift.Server.TServer/LogDelegate '<>f__mg$cache3'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class Thrift.Server.TServer/LogDelegate '<>f__mg$cache4'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class Thrift.Server.TServer/LogDelegate '<>f__mg$cache5'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method family hidebysig specialname instance class Thrift.Server.TServer/LogDelegate 
          get_logDelegate() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::_logDelegate
    IL_0006:  ret
  } // end of method TServer::get_logDelegate

  .method family hidebysig specialname instance void 
          set_logDelegate(class Thrift.Server.TServer/LogDelegate 'value') cil managed
  {
    // Code size       48 (0x30)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  brfalse    IL_000d

    IL_0007:  ldarg.1
    IL_0008:  br         IL_002a

    IL_000d:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache0'
    IL_0012:  brtrue.s   IL_0025

    IL_0014:  ldnull
    IL_0015:  ldftn      void Thrift.Server.TServer::DefaultLogDelegate(string)
    IL_001b:  newobj     instance void Thrift.Server.TServer/LogDelegate::.ctor(object,
                                                                                native int)
    IL_0020:  stsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache0'
    IL_0025:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache0'
    IL_002a:  stfld      class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::_logDelegate
    IL_002f:  ret
  } // end of method TServer::set_logDelegate

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport) cil managed
  {
    // Code size       58 (0x3a)
    .maxstack  9
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_0008:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_000d:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0012:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0017:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache1'
    IL_001c:  brtrue.s   IL_002f

    IL_001e:  ldnull
    IL_001f:  ldftn      void Thrift.Server.TServer::DefaultLogDelegate(string)
    IL_0025:  newobj     instance void Thrift.Server.TServer/LogDelegate::.ctor(object,
                                                                                native int)
    IL_002a:  stsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache1'
    IL_002f:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache1'
    IL_0034:  call       instance void Thrift.Server.TServer::.ctor(class Thrift.TProcessor,
                                                                    class Thrift.Transport.TServerTransport,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Server.TServer/LogDelegate)
    IL_0039:  ret
  } // end of method TServer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport,
                               class Thrift.Server.TServer/LogDelegate logDelegate) cil managed
  {
    // Code size       58 (0x3a)
    .maxstack  9
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_0008:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_000d:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0012:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0017:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache2'
    IL_001c:  brtrue.s   IL_002f

    IL_001e:  ldnull
    IL_001f:  ldftn      void Thrift.Server.TServer::DefaultLogDelegate(string)
    IL_0025:  newobj     instance void Thrift.Server.TServer/LogDelegate::.ctor(object,
                                                                                native int)
    IL_002a:  stsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache2'
    IL_002f:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache2'
    IL_0034:  call       instance void Thrift.Server.TServer::.ctor(class Thrift.TProcessor,
                                                                    class Thrift.Transport.TServerTransport,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Server.TServer/LogDelegate)
    IL_0039:  ret
  } // end of method TServer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport,
                               class Thrift.Transport.TTransportFactory transportFactory) cil managed
  {
    // Code size       50 (0x32)
    .maxstack  9
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldarg.3
    IL_0004:  ldarg.3
    IL_0005:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_000a:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_000f:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache3'
    IL_0014:  brtrue.s   IL_0027

    IL_0016:  ldnull
    IL_0017:  ldftn      void Thrift.Server.TServer::DefaultLogDelegate(string)
    IL_001d:  newobj     instance void Thrift.Server.TServer/LogDelegate::.ctor(object,
                                                                                native int)
    IL_0022:  stsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache3'
    IL_0027:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache3'
    IL_002c:  call       instance void Thrift.Server.TServer::.ctor(class Thrift.TProcessor,
                                                                    class Thrift.Transport.TServerTransport,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Server.TServer/LogDelegate)
    IL_0031:  ret
  } // end of method TServer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport,
                               class Thrift.Transport.TTransportFactory transportFactory,
                               class Thrift.Protocol.TProtocolFactory protocolFactory) cil managed
  {
    // Code size       44 (0x2c)
    .maxstack  9
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldarg.3
    IL_0004:  ldarg.3
    IL_0005:  ldarg.s    protocolFactory
    IL_0007:  ldarg.s    protocolFactory
    IL_0009:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache4'
    IL_000e:  brtrue.s   IL_0021

    IL_0010:  ldnull
    IL_0011:  ldftn      void Thrift.Server.TServer::DefaultLogDelegate(string)
    IL_0017:  newobj     instance void Thrift.Server.TServer/LogDelegate::.ctor(object,
                                                                                native int)
    IL_001c:  stsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache4'
    IL_0021:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache4'
    IL_0026:  call       instance void Thrift.Server.TServer::.ctor(class Thrift.TProcessor,
                                                                    class Thrift.Transport.TServerTransport,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Server.TServer/LogDelegate)
    IL_002b:  ret
  } // end of method TServer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport,
                               class Thrift.Transport.TTransportFactory inputTransportFactory,
                               class Thrift.Transport.TTransportFactory outputTransportFactory,
                               class Thrift.Protocol.TProtocolFactory inputProtocolFactory,
                               class Thrift.Protocol.TProtocolFactory outputProtocolFactory,
                               class Thrift.Server.TServer/LogDelegate logDelegate) cil managed
  {
    // Code size       101 (0x65)
    .maxstack  3
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class Thrift.TProcessor Thrift.Server.TServer::processor
    IL_000d:  ldarg.0
    IL_000e:  ldarg.2
    IL_000f:  stfld      class Thrift.Transport.TServerTransport Thrift.Server.TServer::serverTransport
    IL_0014:  ldarg.0
    IL_0015:  ldarg.3
    IL_0016:  stfld      class Thrift.Transport.TTransportFactory Thrift.Server.TServer::inputTransportFactory
    IL_001b:  ldarg.0
    IL_001c:  ldarg.s    outputTransportFactory
    IL_001e:  stfld      class Thrift.Transport.TTransportFactory Thrift.Server.TServer::outputTransportFactory
    IL_0023:  ldarg.0
    IL_0024:  ldarg.s    inputProtocolFactory
    IL_0026:  stfld      class Thrift.Protocol.TProtocolFactory Thrift.Server.TServer::inputProtocolFactory
    IL_002b:  ldarg.0
    IL_002c:  ldarg.s    outputProtocolFactory
    IL_002e:  stfld      class Thrift.Protocol.TProtocolFactory Thrift.Server.TServer::outputProtocolFactory
    IL_0033:  ldarg.0
    IL_0034:  ldarg.s    logDelegate
    IL_0036:  brfalse    IL_0042

    IL_003b:  ldarg.s    logDelegate
    IL_003d:  br         IL_005f

    IL_0042:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache5'
    IL_0047:  brtrue.s   IL_005a

    IL_0049:  ldnull
    IL_004a:  ldftn      void Thrift.Server.TServer::DefaultLogDelegate(string)
    IL_0050:  newobj     instance void Thrift.Server.TServer/LogDelegate::.ctor(object,
                                                                                native int)
    IL_0055:  stsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache5'
    IL_005a:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::'<>f__mg$cache5'
    IL_005f:  call       instance void Thrift.Server.TServer::set_logDelegate(class Thrift.Server.TServer/LogDelegate)
    IL_0064:  ret
  } // end of method TServer::.ctor

  .method public hidebysig instance void 
          setEventHandler(class Thrift.Server.TServerEventHandler seh) cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_0007:  ret
  } // end of method TServer::setEventHandler

  .method public hidebysig instance class Thrift.Server.TServerEventHandler 
          getEventHandler() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_0006:  ret
  } // end of method TServer::getEventHandler

  .method family hidebysig static void  DefaultLogDelegate(string s) cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  call       class [mscorlib]System.IO.TextWriter [mscorlib]System.Console::get_Error()
    IL_0005:  ldarg.0
    IL_0006:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
    IL_000b:  ret
  } // end of method TServer::DefaultLogDelegate

  .method public hidebysig newslot abstract virtual 
          instance void  Serve() cil managed
  {
  } // end of method TServer::Serve

  .method public hidebysig newslot abstract virtual 
          instance void  Stop() cil managed
  {
  } // end of method TServer::Stop

  .property instance class Thrift.Server.TServer/LogDelegate
          logDelegate()
  {
    .get instance class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::get_logDelegate()
    .set instance void Thrift.Server.TServer::set_logDelegate(class Thrift.Server.TServer/LogDelegate)
  } // end of property TServer::logDelegate
} // end of class Thrift.Server.TServer

.class interface public abstract auto ansi Thrift.Server.TServerEventHandler
{
  .method public hidebysig newslot abstract virtual 
          instance void  preServe() cil managed
  {
  } // end of method TServerEventHandler::preServe

  .method public hidebysig newslot abstract virtual 
          instance object  createContext(class Thrift.Protocol.TProtocol input,
                                         class Thrift.Protocol.TProtocol output) cil managed
  {
  } // end of method TServerEventHandler::createContext

  .method public hidebysig newslot abstract virtual 
          instance void  deleteContext(object serverContext,
                                       class Thrift.Protocol.TProtocol input,
                                       class Thrift.Protocol.TProtocol output) cil managed
  {
  } // end of method TServerEventHandler::deleteContext

  .method public hidebysig newslot abstract virtual 
          instance void  processContext(object serverContext,
                                        class Thrift.Transport.TTransport transport) cil managed
  {
  } // end of method TServerEventHandler::processContext

} // end of class Thrift.Server.TServerEventHandler

.class public auto ansi beforefieldinit Thrift.Server.TSimpleServer
       extends Thrift.Server.TServer
{
  .field private bool stop
  .field private static class Thrift.Server.TServer/LogDelegate '<>f__mg$cache0'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class Thrift.Server.TServer/LogDelegate '<>f__mg$cache1'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class Thrift.Server.TServer/LogDelegate '<>f__mg$cache2'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport) cil managed
  {
    // Code size       58 (0x3a)
    .maxstack  9
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_0008:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_000d:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0012:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0017:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TSimpleServer::'<>f__mg$cache0'
    IL_001c:  brtrue.s   IL_002f

    IL_001e:  ldnull
    IL_001f:  ldftn      void Thrift.Server.TServer::DefaultLogDelegate(string)
    IL_0025:  newobj     instance void Thrift.Server.TServer/LogDelegate::.ctor(object,
                                                                                native int)
    IL_002a:  stsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TSimpleServer::'<>f__mg$cache0'
    IL_002f:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TSimpleServer::'<>f__mg$cache0'
    IL_0034:  call       instance void Thrift.Server.TServer::.ctor(class Thrift.TProcessor,
                                                                    class Thrift.Transport.TServerTransport,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Server.TServer/LogDelegate)
    IL_0039:  ret
  } // end of method TSimpleServer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport,
                               class Thrift.Server.TServer/LogDelegate logDel) cil managed
  {
    // Code size       30 (0x1e)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_0008:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_000d:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0012:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0017:  ldarg.3
    IL_0018:  call       instance void Thrift.Server.TServer::.ctor(class Thrift.TProcessor,
                                                                    class Thrift.Transport.TServerTransport,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Server.TServer/LogDelegate)
    IL_001d:  ret
  } // end of method TSimpleServer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport,
                               class Thrift.Transport.TTransportFactory transportFactory) cil managed
  {
    // Code size       50 (0x32)
    .maxstack  9
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldarg.3
    IL_0004:  ldarg.3
    IL_0005:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_000a:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_000f:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TSimpleServer::'<>f__mg$cache1'
    IL_0014:  brtrue.s   IL_0027

    IL_0016:  ldnull
    IL_0017:  ldftn      void Thrift.Server.TServer::DefaultLogDelegate(string)
    IL_001d:  newobj     instance void Thrift.Server.TServer/LogDelegate::.ctor(object,
                                                                                native int)
    IL_0022:  stsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TSimpleServer::'<>f__mg$cache1'
    IL_0027:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TSimpleServer::'<>f__mg$cache1'
    IL_002c:  call       instance void Thrift.Server.TServer::.ctor(class Thrift.TProcessor,
                                                                    class Thrift.Transport.TServerTransport,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Server.TServer/LogDelegate)
    IL_0031:  ret
  } // end of method TSimpleServer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport,
                               class Thrift.Transport.TTransportFactory transportFactory,
                               class Thrift.Protocol.TProtocolFactory protocolFactory) cil managed
  {
    // Code size       44 (0x2c)
    .maxstack  9
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldarg.3
    IL_0004:  ldarg.3
    IL_0005:  ldarg.s    protocolFactory
    IL_0007:  ldarg.s    protocolFactory
    IL_0009:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TSimpleServer::'<>f__mg$cache2'
    IL_000e:  brtrue.s   IL_0021

    IL_0010:  ldnull
    IL_0011:  ldftn      void Thrift.Server.TServer::DefaultLogDelegate(string)
    IL_0017:  newobj     instance void Thrift.Server.TServer/LogDelegate::.ctor(object,
                                                                                native int)
    IL_001c:  stsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TSimpleServer::'<>f__mg$cache2'
    IL_0021:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TSimpleServer::'<>f__mg$cache2'
    IL_0026:  call       instance void Thrift.Server.TServer::.ctor(class Thrift.TProcessor,
                                                                    class Thrift.Transport.TServerTransport,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Server.TServer/LogDelegate)
    IL_002b:  ret
  } // end of method TSimpleServer::.ctor

  .method public hidebysig virtual instance void 
          Serve() cil managed
  {
    // Code size       451 (0x1c3)
    .maxstack  4
    .locals init (class Thrift.Transport.TTransportException V_0,
             class Thrift.Transport.TTransport V_1,
             class Thrift.Transport.TTransport V_2,
             class Thrift.Transport.TTransport V_3,
             class Thrift.Protocol.TProtocol V_4,
             class Thrift.Protocol.TProtocol V_5,
             object V_6,
             class Thrift.Transport.TTransport V_7,
             class Thrift.Transport.TTransport V_8,
             class Thrift.Transport.TTransport V_9,
             class Thrift.Transport.TTransportException V_10,
             class [mscorlib]System.Exception V_11)
    .try
    {
      IL_0000:  ldarg.0
      IL_0001:  ldfld      class Thrift.Transport.TServerTransport Thrift.Server.TServer::serverTransport
      IL_0006:  callvirt   instance void Thrift.Transport.TServerTransport::Listen()
      IL_000b:  leave      IL_0027

    }  // end .try
    catch Thrift.Transport.TTransportException 
    {
      IL_0010:  stloc.0
      IL_0011:  ldarg.0
      IL_0012:  call       instance class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::get_logDelegate()
      IL_0017:  ldloc.0
      IL_0018:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_001d:  callvirt   instance void Thrift.Server.TServer/LogDelegate::Invoke(string)
      IL_0022:  leave      IL_01c2

    }  // end handler
    IL_0027:  ldarg.0
    IL_0028:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_002d:  brfalse    IL_003d

    IL_0032:  ldarg.0
    IL_0033:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_0038:  callvirt   instance void Thrift.Server.TServerEventHandler::preServe()
    IL_003d:  br         IL_01b7

    IL_0042:  ldnull
    IL_0043:  stloc.1
    IL_0044:  ldnull
    IL_0045:  stloc.2
    IL_0046:  ldnull
    IL_0047:  stloc.3
    IL_0048:  ldnull
    IL_0049:  stloc.s    V_4
    IL_004b:  ldnull
    IL_004c:  stloc.s    V_5
    IL_004e:  ldnull
    IL_004f:  stloc.s    V_6
    .try
    {
      IL_0051:  ldarg.0
      IL_0052:  ldfld      class Thrift.Transport.TServerTransport Thrift.Server.TServer::serverTransport
      IL_0057:  callvirt   instance class Thrift.Transport.TTransport Thrift.Transport.TServerTransport::Accept()
      IL_005c:  dup
      IL_005d:  stloc.1
      IL_005e:  stloc.s    V_7
      .try
      {
        IL_0060:  ldloc.1
        IL_0061:  brfalse    IL_0138

        IL_0066:  ldarg.0
        IL_0067:  ldfld      class Thrift.Transport.TTransportFactory Thrift.Server.TServer::inputTransportFactory
        IL_006c:  ldloc.1
        IL_006d:  callvirt   instance class Thrift.Transport.TTransport Thrift.Transport.TTransportFactory::GetTransport(class Thrift.Transport.TTransport)
        IL_0072:  dup
        IL_0073:  stloc.2
        IL_0074:  stloc.s    V_8
        .try
        {
          IL_0076:  ldarg.0
          IL_0077:  ldfld      class Thrift.Transport.TTransportFactory Thrift.Server.TServer::outputTransportFactory
          IL_007c:  ldloc.1
          IL_007d:  callvirt   instance class Thrift.Transport.TTransport Thrift.Transport.TTransportFactory::GetTransport(class Thrift.Transport.TTransport)
          IL_0082:  dup
          IL_0083:  stloc.3
          IL_0084:  stloc.s    V_9
          .try
          {
            IL_0086:  ldarg.0
            IL_0087:  ldfld      class Thrift.Protocol.TProtocolFactory Thrift.Server.TServer::inputProtocolFactory
            IL_008c:  ldloc.2
            IL_008d:  callvirt   instance class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolFactory::GetProtocol(class Thrift.Transport.TTransport)
            IL_0092:  stloc.s    V_4
            IL_0094:  ldarg.0
            IL_0095:  ldfld      class Thrift.Protocol.TProtocolFactory Thrift.Server.TServer::outputProtocolFactory
            IL_009a:  ldloc.3
            IL_009b:  callvirt   instance class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolFactory::GetProtocol(class Thrift.Transport.TTransport)
            IL_00a0:  stloc.s    V_5
            IL_00a2:  ldarg.0
            IL_00a3:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
            IL_00a8:  brfalse    IL_00be

            IL_00ad:  ldarg.0
            IL_00ae:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
            IL_00b3:  ldloc.s    V_4
            IL_00b5:  ldloc.s    V_5
            IL_00b7:  callvirt   instance object Thrift.Server.TServerEventHandler::createContext(class Thrift.Protocol.TProtocol,
                                                                                                  class Thrift.Protocol.TProtocol)
            IL_00bc:  stloc.s    V_6
            IL_00be:  br         IL_0105

            IL_00c3:  ldloc.2
            IL_00c4:  callvirt   instance bool Thrift.Transport.TTransport::Peek()
            IL_00c9:  brtrue     IL_00d3

            IL_00ce:  leave      IL_0110

            IL_00d3:  ldarg.0
            IL_00d4:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
            IL_00d9:  brfalse    IL_00ec

            IL_00de:  ldarg.0
            IL_00df:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
            IL_00e4:  ldloc.s    V_6
            IL_00e6:  ldloc.2
            IL_00e7:  callvirt   instance void Thrift.Server.TServerEventHandler::processContext(object,
                                                                                                 class Thrift.Transport.TTransport)
            IL_00ec:  ldarg.0
            IL_00ed:  ldfld      class Thrift.TProcessor Thrift.Server.TServer::processor
            IL_00f2:  ldloc.s    V_4
            IL_00f4:  ldloc.s    V_5
            IL_00f6:  callvirt   instance bool Thrift.TProcessor::Process(class Thrift.Protocol.TProtocol,
                                                                          class Thrift.Protocol.TProtocol)
            IL_00fb:  brtrue     IL_0105

            IL_0100:  leave      IL_0110

            IL_0105:  ldarg.0
            IL_0106:  ldfld      bool Thrift.Server.TSimpleServer::stop
            IL_010b:  brfalse    IL_00c3

            IL_0110:  leave      IL_0124

          }  // end .try
          finally
          {
            IL_0115:  ldloc.s    V_9
            IL_0117:  brfalse    IL_0123

            IL_011c:  ldloc.s    V_9
            IL_011e:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
            IL_0123:  endfinally
          }  // end handler
          IL_0124:  leave      IL_0138

        }  // end .try
        finally
        {
          IL_0129:  ldloc.s    V_8
          IL_012b:  brfalse    IL_0137

          IL_0130:  ldloc.s    V_8
          IL_0132:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
          IL_0137:  endfinally
        }  // end handler
        IL_0138:  leave      IL_014c

      }  // end .try
      finally
      {
        IL_013d:  ldloc.s    V_7
        IL_013f:  brfalse    IL_014b

        IL_0144:  ldloc.s    V_7
        IL_0146:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_014b:  endfinally
      }  // end handler
      IL_014c:  leave      IL_019b

    }  // end .try
    catch Thrift.Transport.TTransportException 
    {
      IL_0151:  stloc.s    V_10
      IL_0153:  ldarg.0
      IL_0154:  ldfld      bool Thrift.Server.TSimpleServer::stop
      IL_0159:  brfalse    IL_016b

      IL_015e:  ldloc.s    V_10
      IL_0160:  callvirt   instance valuetype Thrift.Transport.TTransportException/ExceptionType Thrift.Transport.TTransportException::get_Type()
      IL_0165:  ldc.i4.5
      IL_0166:  beq        IL_017d

      IL_016b:  ldarg.0
      IL_016c:  call       instance class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::get_logDelegate()
      IL_0171:  ldloc.s    V_10
      IL_0173:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_0178:  callvirt   instance void Thrift.Server.TServer/LogDelegate::Invoke(string)
      IL_017d:  leave      IL_019b

    }  // end handler
    catch [mscorlib]System.Exception 
    {
      IL_0182:  stloc.s    V_11
      IL_0184:  ldarg.0
      IL_0185:  call       instance class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::get_logDelegate()
      IL_018a:  ldloc.s    V_11
      IL_018c:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_0191:  callvirt   instance void Thrift.Server.TServer/LogDelegate::Invoke(string)
      IL_0196:  leave      IL_019b

    }  // end handler
    IL_019b:  ldarg.0
    IL_019c:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_01a1:  brfalse    IL_01b7

    IL_01a6:  ldarg.0
    IL_01a7:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_01ac:  ldloc.s    V_6
    IL_01ae:  ldloc.s    V_4
    IL_01b0:  ldloc.s    V_5
    IL_01b2:  callvirt   instance void Thrift.Server.TServerEventHandler::deleteContext(object,
                                                                                        class Thrift.Protocol.TProtocol,
                                                                                        class Thrift.Protocol.TProtocol)
    IL_01b7:  ldarg.0
    IL_01b8:  ldfld      bool Thrift.Server.TSimpleServer::stop
    IL_01bd:  brfalse    IL_0042

    IL_01c2:  ret
  } // end of method TSimpleServer::Serve

  .method public hidebysig virtual instance void 
          Stop() cil managed
  {
    // Code size       19 (0x13)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.1
    IL_0002:  stfld      bool Thrift.Server.TSimpleServer::stop
    IL_0007:  ldarg.0
    IL_0008:  ldfld      class Thrift.Transport.TServerTransport Thrift.Server.TServer::serverTransport
    IL_000d:  callvirt   instance void Thrift.Transport.TServerTransport::Close()
    IL_0012:  ret
  } // end of method TSimpleServer::Stop

} // end of class Thrift.Server.TSimpleServer

.class public auto ansi beforefieldinit Thrift.Server.TThreadPoolServer
       extends Thrift.Server.TServer
{
  .field private static literal int32 DEFAULT_MIN_THREADS = int32(0x0000000A)
  .field private static literal int32 DEFAULT_MAX_THREADS = int32(0x00000064)
  .field private bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) stop
  .field private static class Thrift.Server.TServer/LogDelegate '<>f__mg$cache0'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class Thrift.Server.TServer/LogDelegate '<>f__mg$cache1'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport) cil managed
  {
    // Code size       62 (0x3e)
    .maxstack  11
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_0008:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_000d:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0012:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0017:  ldc.i4.s   10
    IL_0019:  ldc.i4.s   100
    IL_001b:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TThreadPoolServer::'<>f__mg$cache0'
    IL_0020:  brtrue.s   IL_0033

    IL_0022:  ldnull
    IL_0023:  ldftn      void Thrift.Server.TServer::DefaultLogDelegate(string)
    IL_0029:  newobj     instance void Thrift.Server.TServer/LogDelegate::.ctor(object,
                                                                                native int)
    IL_002e:  stsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TThreadPoolServer::'<>f__mg$cache0'
    IL_0033:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TThreadPoolServer::'<>f__mg$cache0'
    IL_0038:  call       instance void Thrift.Server.TThreadPoolServer::.ctor(class Thrift.TProcessor,
                                                                              class Thrift.Transport.TServerTransport,
                                                                              class Thrift.Transport.TTransportFactory,
                                                                              class Thrift.Transport.TTransportFactory,
                                                                              class Thrift.Protocol.TProtocolFactory,
                                                                              class Thrift.Protocol.TProtocolFactory,
                                                                              int32,
                                                                              int32,
                                                                              class Thrift.Server.TServer/LogDelegate)
    IL_003d:  ret
  } // end of method TThreadPoolServer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport,
                               class Thrift.Server.TServer/LogDelegate logDelegate) cil managed
  {
    // Code size       34 (0x22)
    .maxstack  10
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_0008:  newobj     instance void Thrift.Transport.TTransportFactory::.ctor()
    IL_000d:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0012:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0017:  ldc.i4.s   10
    IL_0019:  ldc.i4.s   100
    IL_001b:  ldarg.3
    IL_001c:  call       instance void Thrift.Server.TThreadPoolServer::.ctor(class Thrift.TProcessor,
                                                                              class Thrift.Transport.TServerTransport,
                                                                              class Thrift.Transport.TTransportFactory,
                                                                              class Thrift.Transport.TTransportFactory,
                                                                              class Thrift.Protocol.TProtocolFactory,
                                                                              class Thrift.Protocol.TProtocolFactory,
                                                                              int32,
                                                                              int32,
                                                                              class Thrift.Server.TServer/LogDelegate)
    IL_0021:  ret
  } // end of method TThreadPoolServer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport,
                               class Thrift.Transport.TTransportFactory transportFactory,
                               class Thrift.Protocol.TProtocolFactory protocolFactory) cil managed
  {
    // Code size       48 (0x30)
    .maxstack  11
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldarg.3
    IL_0004:  ldarg.3
    IL_0005:  ldarg.s    protocolFactory
    IL_0007:  ldarg.s    protocolFactory
    IL_0009:  ldc.i4.s   10
    IL_000b:  ldc.i4.s   100
    IL_000d:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TThreadPoolServer::'<>f__mg$cache1'
    IL_0012:  brtrue.s   IL_0025

    IL_0014:  ldnull
    IL_0015:  ldftn      void Thrift.Server.TServer::DefaultLogDelegate(string)
    IL_001b:  newobj     instance void Thrift.Server.TServer/LogDelegate::.ctor(object,
                                                                                native int)
    IL_0020:  stsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TThreadPoolServer::'<>f__mg$cache1'
    IL_0025:  ldsfld     class Thrift.Server.TServer/LogDelegate Thrift.Server.TThreadPoolServer::'<>f__mg$cache1'
    IL_002a:  call       instance void Thrift.Server.TThreadPoolServer::.ctor(class Thrift.TProcessor,
                                                                              class Thrift.Transport.TServerTransport,
                                                                              class Thrift.Transport.TTransportFactory,
                                                                              class Thrift.Transport.TTransportFactory,
                                                                              class Thrift.Protocol.TProtocolFactory,
                                                                              class Thrift.Protocol.TProtocolFactory,
                                                                              int32,
                                                                              int32,
                                                                              class Thrift.Server.TServer/LogDelegate)
    IL_002f:  ret
  } // end of method TThreadPoolServer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Transport.TServerTransport serverTransport,
                               class Thrift.Transport.TTransportFactory inputTransportFactory,
                               class Thrift.Transport.TTransportFactory outputTransportFactory,
                               class Thrift.Protocol.TProtocolFactory inputProtocolFactory,
                               class Thrift.Protocol.TProtocolFactory outputProtocolFactory,
                               int32 minThreadPoolThreads,
                               int32 maxThreadPoolThreads,
                               class Thrift.Server.TServer/LogDelegate logDel) cil managed
  {
    // Code size       97 (0x61)
    .maxstack  8
    .locals init (object V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldarg.3
    IL_0004:  ldarg.s    outputTransportFactory
    IL_0006:  ldarg.s    inputProtocolFactory
    IL_0008:  ldarg.s    outputProtocolFactory
    IL_000a:  ldarg.s    logDel
    IL_000c:  call       instance void Thrift.Server.TServer::.ctor(class Thrift.TProcessor,
                                                                    class Thrift.Transport.TServerTransport,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Transport.TTransportFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Protocol.TProtocolFactory,
                                                                    class Thrift.Server.TServer/LogDelegate)
    IL_0011:  ldtoken    Thrift.Server.TThreadPoolServer
    IL_0016:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
    IL_001b:  stloc.0
    IL_001c:  ldloc.0
    IL_001d:  call       void [mscorlib]System.Threading.Monitor::Enter(object)
    .try
    {
      IL_0022:  ldarg.s    maxThreadPoolThreads
      IL_0024:  ldarg.s    maxThreadPoolThreads
      IL_0026:  call       bool [mscorlib]System.Threading.ThreadPool::SetMaxThreads(int32,
                                                                                     int32)
      IL_002b:  brtrue     IL_003b

      IL_0030:  ldstr      "Error: could not SetMaxThreads in ThreadPool"
      IL_0035:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_003a:  throw

      IL_003b:  ldarg.s    minThreadPoolThreads
      IL_003d:  ldarg.s    minThreadPoolThreads
      IL_003f:  call       bool [mscorlib]System.Threading.ThreadPool::SetMinThreads(int32,
                                                                                     int32)
      IL_0044:  brtrue     IL_0054

      IL_0049:  ldstr      "Error: could not SetMinThreads in ThreadPool"
      IL_004e:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_0053:  throw

      IL_0054:  leave      IL_0060

    }  // end .try
    finally
    {
      IL_0059:  ldloc.0
      IL_005a:  call       void [mscorlib]System.Threading.Monitor::Exit(object)
      IL_005f:  endfinally
    }  // end handler
    IL_0060:  ret
  } // end of method TThreadPoolServer::.ctor

  .method public hidebysig virtual instance void 
          Serve() cil managed
  {
    // Code size       248 (0xf8)
    .maxstack  3
    .locals init (class Thrift.Transport.TTransportException V_0,
             int32 V_1,
             class Thrift.Transport.TTransport V_2,
             class Thrift.Transport.TTransportException V_3,
             class Thrift.Transport.TTransportException V_4)
    .try
    {
      IL_0000:  ldarg.0
      IL_0001:  ldfld      class Thrift.Transport.TServerTransport Thrift.Server.TServer::serverTransport
      IL_0006:  callvirt   instance void Thrift.Transport.TServerTransport::Listen()
      IL_000b:  leave      IL_002c

    }  // end .try
    catch Thrift.Transport.TTransportException 
    {
      IL_0010:  stloc.0
      IL_0011:  ldarg.0
      IL_0012:  call       instance class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::get_logDelegate()
      IL_0017:  ldstr      "Error, could not listen on ServerTransport: "
      IL_001c:  ldloc.0
      IL_001d:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_0022:  callvirt   instance void Thrift.Server.TServer/LogDelegate::Invoke(string)
      IL_0027:  leave      IL_00f7

    }  // end handler
    IL_002c:  ldarg.0
    IL_002d:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_0032:  brfalse    IL_0042

    IL_0037:  ldarg.0
    IL_0038:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_003d:  callvirt   instance void Thrift.Server.TServerEventHandler::preServe()
    IL_0042:  br         IL_00a1

    IL_0047:  ldc.i4.0
    IL_0048:  stloc.1
    .try
    {
      IL_0049:  ldarg.0
      IL_004a:  ldfld      class Thrift.Transport.TServerTransport Thrift.Server.TServer::serverTransport
      IL_004f:  callvirt   instance class Thrift.Transport.TTransport Thrift.Transport.TServerTransport::Accept()
      IL_0054:  stloc.2
      IL_0055:  ldarg.0
      IL_0056:  ldftn      instance void Thrift.Server.TThreadPoolServer::Execute(object)
      IL_005c:  newobj     instance void [mscorlib]System.Threading.WaitCallback::.ctor(object,
                                                                                        native int)
      IL_0061:  ldloc.2
      IL_0062:  call       bool [mscorlib]System.Threading.ThreadPool::QueueUserWorkItem(class [mscorlib]System.Threading.WaitCallback,
                                                                                         object)
      IL_0067:  pop
      IL_0068:  leave      IL_00a1

    }  // end .try
    catch Thrift.Transport.TTransportException 
    {
      IL_006d:  stloc.3
      IL_006e:  ldarg.0
      IL_006f:  volatile.
      IL_0071:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Server.TThreadPoolServer::stop
      IL_0076:  brfalse    IL_0087

      IL_007b:  ldloc.3
      IL_007c:  callvirt   instance valuetype Thrift.Transport.TTransportException/ExceptionType Thrift.Transport.TTransportException::get_Type()
      IL_0081:  ldc.i4.5
      IL_0082:  beq        IL_009c

      IL_0087:  ldloc.1
      IL_0088:  ldc.i4.1
      IL_0089:  add
      IL_008a:  stloc.1
      IL_008b:  ldarg.0
      IL_008c:  call       instance class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::get_logDelegate()
      IL_0091:  ldloc.3
      IL_0092:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_0097:  callvirt   instance void Thrift.Server.TServer/LogDelegate::Invoke(string)
      IL_009c:  leave      IL_00a1

    }  // end handler
    IL_00a1:  ldarg.0
    IL_00a2:  volatile.
    IL_00a4:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Server.TThreadPoolServer::stop
    IL_00a9:  brfalse    IL_0047

    IL_00ae:  ldarg.0
    IL_00af:  volatile.
    IL_00b1:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Server.TThreadPoolServer::stop
    IL_00b6:  brfalse    IL_00f7

    .try
    {
      IL_00bb:  ldarg.0
      IL_00bc:  ldfld      class Thrift.Transport.TServerTransport Thrift.Server.TServer::serverTransport
      IL_00c1:  callvirt   instance void Thrift.Transport.TServerTransport::Close()
      IL_00c6:  leave      IL_00ee

    }  // end .try
    catch Thrift.Transport.TTransportException 
    {
      IL_00cb:  stloc.s    V_4
      IL_00cd:  ldarg.0
      IL_00ce:  call       instance class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::get_logDelegate()
      IL_00d3:  ldstr      "TServerTransport failed on close: "
      IL_00d8:  ldloc.s    V_4
      IL_00da:  callvirt   instance string [mscorlib]System.Exception::get_Message()
      IL_00df:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_00e4:  callvirt   instance void Thrift.Server.TServer/LogDelegate::Invoke(string)
      IL_00e9:  leave      IL_00ee

    }  // end handler
    IL_00ee:  ldarg.0
    IL_00ef:  ldc.i4.0
    IL_00f0:  volatile.
    IL_00f2:  stfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Server.TThreadPoolServer::stop
    IL_00f7:  ret
  } // end of method TThreadPoolServer::Serve

  .method private hidebysig instance void 
          Execute(object threadContext) cil managed
  {
    // Code size       275 (0x113)
    .maxstack  4
    .locals init (class Thrift.Transport.TTransport V_0,
             class Thrift.Transport.TTransport V_1,
             class Thrift.Transport.TTransport V_2,
             class Thrift.Protocol.TProtocol V_3,
             class Thrift.Protocol.TProtocol V_4,
             object V_5,
             class [mscorlib]System.Exception V_6)
    IL_0000:  ldarg.1
    IL_0001:  castclass  Thrift.Transport.TTransport
    IL_0006:  stloc.0
    IL_0007:  ldnull
    IL_0008:  stloc.1
    IL_0009:  ldnull
    IL_000a:  stloc.2
    IL_000b:  ldnull
    IL_000c:  stloc.3
    IL_000d:  ldnull
    IL_000e:  stloc.s    V_4
    IL_0010:  ldnull
    IL_0011:  stloc.s    V_5
    .try
    {
      IL_0013:  ldarg.0
      IL_0014:  ldfld      class Thrift.Transport.TTransportFactory Thrift.Server.TServer::inputTransportFactory
      IL_0019:  ldloc.0
      IL_001a:  callvirt   instance class Thrift.Transport.TTransport Thrift.Transport.TTransportFactory::GetTransport(class Thrift.Transport.TTransport)
      IL_001f:  stloc.1
      IL_0020:  ldarg.0
      IL_0021:  ldfld      class Thrift.Transport.TTransportFactory Thrift.Server.TServer::outputTransportFactory
      IL_0026:  ldloc.0
      IL_0027:  callvirt   instance class Thrift.Transport.TTransport Thrift.Transport.TTransportFactory::GetTransport(class Thrift.Transport.TTransport)
      IL_002c:  stloc.2
      IL_002d:  ldarg.0
      IL_002e:  ldfld      class Thrift.Protocol.TProtocolFactory Thrift.Server.TServer::inputProtocolFactory
      IL_0033:  ldloc.1
      IL_0034:  callvirt   instance class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolFactory::GetProtocol(class Thrift.Transport.TTransport)
      IL_0039:  stloc.3
      IL_003a:  ldarg.0
      IL_003b:  ldfld      class Thrift.Protocol.TProtocolFactory Thrift.Server.TServer::outputProtocolFactory
      IL_0040:  ldloc.2
      IL_0041:  callvirt   instance class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolFactory::GetProtocol(class Thrift.Transport.TTransport)
      IL_0046:  stloc.s    V_4
      IL_0048:  ldarg.0
      IL_0049:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
      IL_004e:  brfalse    IL_0063

      IL_0053:  ldarg.0
      IL_0054:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
      IL_0059:  ldloc.3
      IL_005a:  ldloc.s    V_4
      IL_005c:  callvirt   instance object Thrift.Server.TServerEventHandler::createContext(class Thrift.Protocol.TProtocol,
                                                                                            class Thrift.Protocol.TProtocol)
      IL_0061:  stloc.s    V_5
      IL_0063:  br         IL_00a9

      IL_0068:  ldloc.1
      IL_0069:  callvirt   instance bool Thrift.Transport.TTransport::Peek()
      IL_006e:  brtrue     IL_0078

      IL_0073:  leave      IL_00b6

      IL_0078:  ldarg.0
      IL_0079:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
      IL_007e:  brfalse    IL_0091

      IL_0083:  ldarg.0
      IL_0084:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
      IL_0089:  ldloc.s    V_5
      IL_008b:  ldloc.1
      IL_008c:  callvirt   instance void Thrift.Server.TServerEventHandler::processContext(object,
                                                                                           class Thrift.Transport.TTransport)
      IL_0091:  ldarg.0
      IL_0092:  ldfld      class Thrift.TProcessor Thrift.Server.TServer::processor
      IL_0097:  ldloc.3
      IL_0098:  ldloc.s    V_4
      IL_009a:  callvirt   instance bool Thrift.TProcessor::Process(class Thrift.Protocol.TProtocol,
                                                                    class Thrift.Protocol.TProtocol)
      IL_009f:  brtrue     IL_00a9

      IL_00a4:  leave      IL_00b6

      IL_00a9:  ldarg.0
      IL_00aa:  volatile.
      IL_00ac:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Server.TThreadPoolServer::stop
      IL_00b1:  brfalse    IL_0068

      IL_00b6:  leave      IL_00df

    }  // end .try
    catch Thrift.Transport.TTransportException 
    {
      IL_00bb:  pop
      IL_00bc:  leave      IL_00df

    }  // end handler
    catch [mscorlib]System.Exception 
    {
      IL_00c1:  stloc.s    V_6
      IL_00c3:  ldarg.0
      IL_00c4:  call       instance class Thrift.Server.TServer/LogDelegate Thrift.Server.TServer::get_logDelegate()
      IL_00c9:  ldstr      "Error: "
      IL_00ce:  ldloc.s    V_6
      IL_00d0:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_00d5:  callvirt   instance void Thrift.Server.TServer/LogDelegate::Invoke(string)
      IL_00da:  leave      IL_00df

    }  // end handler
    IL_00df:  ldarg.0
    IL_00e0:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_00e5:  brfalse    IL_00fa

    IL_00ea:  ldarg.0
    IL_00eb:  ldfld      class Thrift.Server.TServerEventHandler Thrift.Server.TServer::serverEventHandler
    IL_00f0:  ldloc.s    V_5
    IL_00f2:  ldloc.3
    IL_00f3:  ldloc.s    V_4
    IL_00f5:  callvirt   instance void Thrift.Server.TServerEventHandler::deleteContext(object,
                                                                                        class Thrift.Protocol.TProtocol,
                                                                                        class Thrift.Protocol.TProtocol)
    IL_00fa:  ldloc.1
    IL_00fb:  brfalse    IL_0106

    IL_0100:  ldloc.1
    IL_0101:  callvirt   instance void Thrift.Transport.TTransport::Close()
    IL_0106:  ldloc.2
    IL_0107:  brfalse    IL_0112

    IL_010c:  ldloc.2
    IL_010d:  callvirt   instance void Thrift.Transport.TTransport::Close()
    IL_0112:  ret
  } // end of method TThreadPoolServer::Execute

  .method public hidebysig virtual instance void 
          Stop() cil managed
  {
    // Code size       21 (0x15)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.1
    IL_0002:  volatile.
    IL_0004:  stfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Server.TThreadPoolServer::stop
    IL_0009:  ldarg.0
    IL_000a:  ldfld      class Thrift.Transport.TServerTransport Thrift.Server.TServer::serverTransport
    IL_000f:  callvirt   instance void Thrift.Transport.TServerTransport::Close()
    IL_0014:  ret
  } // end of method TThreadPoolServer::Stop

} // end of class Thrift.Server.TThreadPoolServer

.class public auto ansi beforefieldinit Thrift.TException
       extends [mscorlib]System.Exception
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Exception::.ctor()
    IL_0006:  ret
  } // end of method TException::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string message) cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  call       instance void [mscorlib]System.Exception::.ctor(string)
    IL_0007:  ret
  } // end of method TException::.ctor

} // end of class Thrift.TException

.class public auto ansi beforefieldinit Thrift.TApplicationException
       extends Thrift.TException
{
  .class auto ansi sealed nested public ExceptionType
         extends [mscorlib]System.Enum
  {
    .field public specialname rtspecialname int32 value__
    .field public static literal valuetype Thrift.TApplicationException/ExceptionType Unknown = int32(0x00000000)
    .field public static literal valuetype Thrift.TApplicationException/ExceptionType UnknownMethod = int32(0x00000001)
    .field public static literal valuetype Thrift.TApplicationException/ExceptionType InvalidMessageType = int32(0x00000002)
    .field public static literal valuetype Thrift.TApplicationException/ExceptionType WrongMethodName = int32(0x00000003)
    .field public static literal valuetype Thrift.TApplicationException/ExceptionType BadSequenceID = int32(0x00000004)
    .field public static literal valuetype Thrift.TApplicationException/ExceptionType MissingResult = int32(0x00000005)
    .field public static literal valuetype Thrift.TApplicationException/ExceptionType InternalError = int32(0x00000006)
    .field public static literal valuetype Thrift.TApplicationException/ExceptionType ProtocolError = int32(0x00000007)
    .field public static literal valuetype Thrift.TApplicationException/ExceptionType InvalidTransform = int32(0x00000008)
    .field public static literal valuetype Thrift.TApplicationException/ExceptionType InvalidProtocol = int32(0x00000009)
    .field public static literal valuetype Thrift.TApplicationException/ExceptionType UnsupportedClientType = int32(0x0000000A)
  } // end of class ExceptionType

  .field family valuetype Thrift.TApplicationException/ExceptionType 'type'
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.TException::.ctor()
    IL_0006:  ret
  } // end of method TApplicationException::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype Thrift.TApplicationException/ExceptionType 'type') cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.TException::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      valuetype Thrift.TApplicationException/ExceptionType Thrift.TApplicationException::'type'
    IL_000d:  ret
  } // end of method TApplicationException::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype Thrift.TApplicationException/ExceptionType 'type',
                               string message) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.2
    IL_0002:  call       instance void Thrift.TException::.ctor(string)
    IL_0007:  ldarg.0
    IL_0008:  ldarg.1
    IL_0009:  stfld      valuetype Thrift.TApplicationException/ExceptionType Thrift.TApplicationException::'type'
    IL_000e:  ret
  } // end of method TApplicationException::.ctor

  .method public hidebysig static class Thrift.TApplicationException 
          Read(class Thrift.Protocol.TProtocol iprot) cil managed
  {
    // Code size       192 (0xc0)
    .maxstack  2
    .locals init (valuetype Thrift.Protocol.TField V_0,
             string V_1,
             valuetype Thrift.TApplicationException/ExceptionType V_2,
             int16 V_3)
    IL_0000:  ldnull
    IL_0001:  stloc.1
    IL_0002:  ldc.i4.0
    IL_0003:  stloc.2
    IL_0004:  ldarg.0
    IL_0005:  callvirt   instance valuetype Thrift.Protocol.TStruct Thrift.Protocol.TProtocol::ReadStructBegin()
    IL_000a:  pop
    IL_000b:  ldarg.0
    IL_000c:  callvirt   instance valuetype Thrift.Protocol.TField Thrift.Protocol.TProtocol::ReadFieldBegin()
    IL_0011:  stloc.0
    IL_0012:  ldloca.s   V_0
    IL_0014:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
    IL_0019:  brtrue     IL_0023

    IL_001e:  br         IL_00b2

    IL_0023:  ldloca.s   V_0
    IL_0025:  call       instance int16 Thrift.Protocol.TField::get_ID()
    IL_002a:  stloc.3
    IL_002b:  ldloc.3
    IL_002c:  ldc.i4.1
    IL_002d:  beq        IL_003e

    IL_0032:  ldloc.3
    IL_0033:  ldc.i4.2
    IL_0034:  beq        IL_006a

    IL_0039:  br         IL_0095

    IL_003e:  ldloca.s   V_0
    IL_0040:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
    IL_0045:  ldc.i4.s   11
    IL_0047:  bne.un     IL_0058

    IL_004c:  ldarg.0
    IL_004d:  callvirt   instance string Thrift.Protocol.TProtocol::ReadString()
    IL_0052:  stloc.1
    IL_0053:  br         IL_0065

    IL_0058:  ldarg.0
    IL_0059:  ldloca.s   V_0
    IL_005b:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
    IL_0060:  call       void Thrift.Protocol.TProtocolUtil::Skip(class Thrift.Protocol.TProtocol,
                                                                  valuetype Thrift.Protocol.TType)
    IL_0065:  br         IL_00a7

    IL_006a:  ldloca.s   V_0
    IL_006c:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
    IL_0071:  ldc.i4.8
    IL_0072:  bne.un     IL_0083

    IL_0077:  ldarg.0
    IL_0078:  callvirt   instance int32 Thrift.Protocol.TProtocol::ReadI32()
    IL_007d:  stloc.2
    IL_007e:  br         IL_0090

    IL_0083:  ldarg.0
    IL_0084:  ldloca.s   V_0
    IL_0086:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
    IL_008b:  call       void Thrift.Protocol.TProtocolUtil::Skip(class Thrift.Protocol.TProtocol,
                                                                  valuetype Thrift.Protocol.TType)
    IL_0090:  br         IL_00a7

    IL_0095:  ldarg.0
    IL_0096:  ldloca.s   V_0
    IL_0098:  call       instance valuetype Thrift.Protocol.TType Thrift.Protocol.TField::get_Type()
    IL_009d:  call       void Thrift.Protocol.TProtocolUtil::Skip(class Thrift.Protocol.TProtocol,
                                                                  valuetype Thrift.Protocol.TType)
    IL_00a2:  br         IL_00a7

    IL_00a7:  ldarg.0
    IL_00a8:  callvirt   instance void Thrift.Protocol.TProtocol::ReadFieldEnd()
    IL_00ad:  br         IL_000b

    IL_00b2:  ldarg.0
    IL_00b3:  callvirt   instance void Thrift.Protocol.TProtocol::ReadStructEnd()
    IL_00b8:  ldloc.2
    IL_00b9:  ldloc.1
    IL_00ba:  newobj     instance void Thrift.TApplicationException::.ctor(valuetype Thrift.TApplicationException/ExceptionType,
                                                                           string)
    IL_00bf:  ret
  } // end of method TApplicationException::Read

  .method public hidebysig instance void 
          Write(class Thrift.Protocol.TProtocol oprot) cil managed
  {
    // Code size       163 (0xa3)
    .maxstack  2
    .locals init (valuetype Thrift.Protocol.TStruct V_0,
             valuetype Thrift.Protocol.TField V_1)
    IL_0000:  ldloca.s   V_0
    IL_0002:  ldstr      "TApplicationException"
    IL_0007:  call       instance void Thrift.Protocol.TStruct::.ctor(string)
    IL_000c:  ldloca.s   V_1
    IL_000e:  initobj    Thrift.Protocol.TField
    IL_0014:  ldarg.1
    IL_0015:  ldloc.0
    IL_0016:  callvirt   instance void Thrift.Protocol.TProtocol::WriteStructBegin(valuetype Thrift.Protocol.TStruct)
    IL_001b:  ldarg.0
    IL_001c:  callvirt   instance string [mscorlib]System.Exception::get_Message()
    IL_0021:  call       bool [mscorlib]System.String::IsNullOrEmpty(string)
    IL_0026:  brtrue     IL_0061

    IL_002b:  ldloca.s   V_1
    IL_002d:  ldstr      "message"
    IL_0032:  call       instance void Thrift.Protocol.TField::set_Name(string)
    IL_0037:  ldloca.s   V_1
    IL_0039:  ldc.i4.s   11
    IL_003b:  call       instance void Thrift.Protocol.TField::set_Type(valuetype Thrift.Protocol.TType)
    IL_0040:  ldloca.s   V_1
    IL_0042:  ldc.i4.1
    IL_0043:  call       instance void Thrift.Protocol.TField::set_ID(int16)
    IL_0048:  ldarg.1
    IL_0049:  ldloc.1
    IL_004a:  callvirt   instance void Thrift.Protocol.TProtocol::WriteFieldBegin(valuetype Thrift.Protocol.TField)
    IL_004f:  ldarg.1
    IL_0050:  ldarg.0
    IL_0051:  callvirt   instance string [mscorlib]System.Exception::get_Message()
    IL_0056:  callvirt   instance void Thrift.Protocol.TProtocol::WriteString(string)
    IL_005b:  ldarg.1
    IL_005c:  callvirt   instance void Thrift.Protocol.TProtocol::WriteFieldEnd()
    IL_0061:  ldloca.s   V_1
    IL_0063:  ldstr      "type"
    IL_0068:  call       instance void Thrift.Protocol.TField::set_Name(string)
    IL_006d:  ldloca.s   V_1
    IL_006f:  ldc.i4.8
    IL_0070:  call       instance void Thrift.Protocol.TField::set_Type(valuetype Thrift.Protocol.TType)
    IL_0075:  ldloca.s   V_1
    IL_0077:  ldc.i4.2
    IL_0078:  call       instance void Thrift.Protocol.TField::set_ID(int16)
    IL_007d:  ldarg.1
    IL_007e:  ldloc.1
    IL_007f:  callvirt   instance void Thrift.Protocol.TProtocol::WriteFieldBegin(valuetype Thrift.Protocol.TField)
    IL_0084:  ldarg.1
    IL_0085:  ldarg.0
    IL_0086:  ldfld      valuetype Thrift.TApplicationException/ExceptionType Thrift.TApplicationException::'type'
    IL_008b:  callvirt   instance void Thrift.Protocol.TProtocol::WriteI32(int32)
    IL_0090:  ldarg.1
    IL_0091:  callvirt   instance void Thrift.Protocol.TProtocol::WriteFieldEnd()
    IL_0096:  ldarg.1
    IL_0097:  callvirt   instance void Thrift.Protocol.TProtocol::WriteFieldStop()
    IL_009c:  ldarg.1
    IL_009d:  callvirt   instance void Thrift.Protocol.TProtocol::WriteStructEnd()
    IL_00a2:  ret
  } // end of method TApplicationException::Write

} // end of class Thrift.TApplicationException

.class interface public abstract auto ansi Thrift.TProcessor
{
  .method public hidebysig newslot abstract virtual 
          instance bool  Process(class Thrift.Protocol.TProtocol iprot,
                                 class Thrift.Protocol.TProtocol oprot) cil managed
  {
  } // end of method TProcessor::Process

} // end of class Thrift.TProcessor

.class public auto ansi beforefieldinit Thrift.Transport.TBufferedTransport
       extends Thrift.Transport.TTransport
       implements [mscorlib]System.IDisposable
{
  .field private class [mscorlib]System.IO.BufferedStream inputBuffer
  .field private class [mscorlib]System.IO.BufferedStream outputBuffer
  .field private int32 bufSize
  .field private class Thrift.Transport.TStreamTransport transport
  .field private bool _IsDisposed
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.Transport.TStreamTransport transport) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldc.i4     0x400
    IL_0007:  call       instance void Thrift.Transport.TBufferedTransport::.ctor(class Thrift.Transport.TStreamTransport,
                                                                                  int32)
    IL_000c:  ret
  } // end of method TBufferedTransport::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.Transport.TStreamTransport transport,
                               int32 bufSize) cil managed
  {
    // Code size       27 (0x1b)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.2
    IL_0008:  stfld      int32 Thrift.Transport.TBufferedTransport::bufSize
    IL_000d:  ldarg.0
    IL_000e:  ldarg.1
    IL_000f:  stfld      class Thrift.Transport.TStreamTransport Thrift.Transport.TBufferedTransport::transport
    IL_0014:  ldarg.0
    IL_0015:  call       instance void Thrift.Transport.TBufferedTransport::InitBuffers()
    IL_001a:  ret
  } // end of method TBufferedTransport::.ctor

  .method public hidebysig specialname instance class Thrift.Transport.TTransport 
          get_UnderlyingTransport() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Transport.TStreamTransport Thrift.Transport.TBufferedTransport::transport
    IL_0006:  ret
  } // end of method TBufferedTransport::get_UnderlyingTransport

  .method public hidebysig specialname virtual 
          instance bool  get_IsOpen() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Transport.TStreamTransport Thrift.Transport.TBufferedTransport::transport
    IL_0006:  callvirt   instance bool Thrift.Transport.TTransport::get_IsOpen()
    IL_000b:  ret
  } // end of method TBufferedTransport::get_IsOpen

  .method private hidebysig instance void 
          InitBuffers() cil managed
  {
    // Code size       89 (0x59)
    .maxstack  3
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Transport.TStreamTransport Thrift.Transport.TBufferedTransport::transport
    IL_0006:  callvirt   instance class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::get_InputStream()
    IL_000b:  brfalse    IL_002c

    IL_0010:  ldarg.0
    IL_0011:  ldarg.0
    IL_0012:  ldfld      class Thrift.Transport.TStreamTransport Thrift.Transport.TBufferedTransport::transport
    IL_0017:  callvirt   instance class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::get_InputStream()
    IL_001c:  ldarg.0
    IL_001d:  ldfld      int32 Thrift.Transport.TBufferedTransport::bufSize
    IL_0022:  newobj     instance void [mscorlib]System.IO.BufferedStream::.ctor(class [mscorlib]System.IO.Stream,
                                                                                 int32)
    IL_0027:  stfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::inputBuffer
    IL_002c:  ldarg.0
    IL_002d:  ldfld      class Thrift.Transport.TStreamTransport Thrift.Transport.TBufferedTransport::transport
    IL_0032:  callvirt   instance class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::get_OutputStream()
    IL_0037:  brfalse    IL_0058

    IL_003c:  ldarg.0
    IL_003d:  ldarg.0
    IL_003e:  ldfld      class Thrift.Transport.TStreamTransport Thrift.Transport.TBufferedTransport::transport
    IL_0043:  callvirt   instance class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::get_OutputStream()
    IL_0048:  ldarg.0
    IL_0049:  ldfld      int32 Thrift.Transport.TBufferedTransport::bufSize
    IL_004e:  newobj     instance void [mscorlib]System.IO.BufferedStream::.ctor(class [mscorlib]System.IO.Stream,
                                                                                 int32)
    IL_0053:  stfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::outputBuffer
    IL_0058:  ret
  } // end of method TBufferedTransport::InitBuffers

  .method private hidebysig instance void 
          CloseBuffers() cil managed
  {
    // Code size       77 (0x4d)
    .maxstack  1
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::inputBuffer
    IL_0006:  brfalse    IL_0026

    IL_000b:  ldarg.0
    IL_000c:  ldfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::inputBuffer
    IL_0011:  callvirt   instance bool [mscorlib]System.IO.Stream::get_CanRead()
    IL_0016:  brfalse    IL_0026

    IL_001b:  ldarg.0
    IL_001c:  ldfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::inputBuffer
    IL_0021:  callvirt   instance void [mscorlib]System.IO.Stream::Close()
    IL_0026:  ldarg.0
    IL_0027:  ldfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::outputBuffer
    IL_002c:  brfalse    IL_004c

    IL_0031:  ldarg.0
    IL_0032:  ldfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::outputBuffer
    IL_0037:  callvirt   instance bool [mscorlib]System.IO.Stream::get_CanWrite()
    IL_003c:  brfalse    IL_004c

    IL_0041:  ldarg.0
    IL_0042:  ldfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::outputBuffer
    IL_0047:  callvirt   instance void [mscorlib]System.IO.Stream::Close()
    IL_004c:  ret
  } // end of method TBufferedTransport::CloseBuffers

  .method public hidebysig virtual instance void 
          Open() cil managed
  {
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Transport.TStreamTransport Thrift.Transport.TBufferedTransport::transport
    IL_0006:  callvirt   instance void Thrift.Transport.TTransport::Open()
    IL_000b:  ldarg.0
    IL_000c:  call       instance void Thrift.Transport.TBufferedTransport::InitBuffers()
    IL_0011:  ret
  } // end of method TBufferedTransport::Open

  .method public hidebysig virtual instance void 
          Close() cil managed
  {
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TBufferedTransport::CloseBuffers()
    IL_0006:  ldarg.0
    IL_0007:  ldfld      class Thrift.Transport.TStreamTransport Thrift.Transport.TBufferedTransport::transport
    IL_000c:  callvirt   instance void Thrift.Transport.TTransport::Close()
    IL_0011:  ret
  } // end of method TBufferedTransport::Close

  .method public hidebysig virtual instance int32 
          Read(uint8[] buf,
               int32 'off',
               int32 len) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::inputBuffer
    IL_0006:  ldarg.1
    IL_0007:  ldarg.2
    IL_0008:  ldarg.3
    IL_0009:  callvirt   instance int32 [mscorlib]System.IO.Stream::Read(uint8[],
                                                                         int32,
                                                                         int32)
    IL_000e:  ret
  } // end of method TBufferedTransport::Read

  .method public hidebysig virtual instance void 
          Write(uint8[] buf,
                int32 'off',
                int32 len) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::outputBuffer
    IL_0006:  ldarg.1
    IL_0007:  ldarg.2
    IL_0008:  ldarg.3
    IL_0009:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                         int32,
                                                                         int32)
    IL_000e:  ret
  } // end of method TBufferedTransport::Write

  .method public hidebysig virtual instance void 
          Flush() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::outputBuffer
    IL_0006:  callvirt   instance void [mscorlib]System.IO.Stream::Flush()
    IL_000b:  ret
  } // end of method TBufferedTransport::Flush

  .method family hidebysig virtual instance void 
          Dispose(bool disposing) cil managed
  {
    // Code size       69 (0x45)
    .maxstack  2
    IL_0000:  ldarg.0
    IL_0001:  ldfld      bool Thrift.Transport.TBufferedTransport::_IsDisposed
    IL_0006:  brtrue     IL_003d

    IL_000b:  ldarg.1
    IL_000c:  brfalse    IL_003d

    IL_0011:  ldarg.0
    IL_0012:  ldfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::inputBuffer
    IL_0017:  brfalse    IL_0027

    IL_001c:  ldarg.0
    IL_001d:  ldfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::inputBuffer
    IL_0022:  callvirt   instance void [mscorlib]System.IO.Stream::Dispose()
    IL_0027:  ldarg.0
    IL_0028:  ldfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::outputBuffer
    IL_002d:  brfalse    IL_003d

    IL_0032:  ldarg.0
    IL_0033:  ldfld      class [mscorlib]System.IO.BufferedStream Thrift.Transport.TBufferedTransport::outputBuffer
    IL_0038:  callvirt   instance void [mscorlib]System.IO.Stream::Dispose()
    IL_003d:  ldarg.0
    IL_003e:  ldc.i4.1
    IL_003f:  stfld      bool Thrift.Transport.TBufferedTransport::_IsDisposed
    IL_0044:  ret
  } // end of method TBufferedTransport::Dispose

  .property instance class Thrift.Transport.TTransport
          UnderlyingTransport()
  {
    .get instance class Thrift.Transport.TTransport Thrift.Transport.TBufferedTransport::get_UnderlyingTransport()
  } // end of property TBufferedTransport::UnderlyingTransport
  .property instance bool IsOpen()
  {
    .get instance bool Thrift.Transport.TBufferedTransport::get_IsOpen()
  } // end of property TBufferedTransport::IsOpen
} // end of class Thrift.Transport.TBufferedTransport

.class public auto ansi beforefieldinit Thrift.Transport.TFramedTransport
       extends Thrift.Transport.TTransport
       implements [mscorlib]System.IDisposable
{
  .class auto ansi nested public beforefieldinit Factory
         extends Thrift.Transport.TTransportFactory
  {
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void Thrift.Transport.TTransportFactory::.ctor()
      IL_0006:  ret
    } // end of method Factory::.ctor

    .method public hidebysig virtual instance class Thrift.Transport.TTransport 
            GetTransport(class Thrift.Transport.TTransport trans) cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  newobj     instance void Thrift.Transport.TFramedTransport::.ctor(class Thrift.Transport.TTransport)
      IL_0006:  ret
    } // end of method Factory::GetTransport

  } // end of class Factory

  .field family class Thrift.Transport.TTransport transport
  .field family class [mscorlib]System.IO.MemoryStream writeBuffer
  .field family class [mscorlib]System.IO.MemoryStream readBuffer
  .field private static literal int32 header_size = int32(0x00000004)
  .field private static uint8[] header_dummy
  .field private bool _IsDisposed
  .method family hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  call       instance void Thrift.Transport.TFramedTransport::InitWriteBuffer()
    IL_000c:  ret
  } // end of method TFramedTransport::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.Transport.TTransport transport) cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TFramedTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class Thrift.Transport.TTransport Thrift.Transport.TFramedTransport::transport
    IL_000d:  ret
  } // end of method TFramedTransport::.ctor

  .method public hidebysig specialname virtual 
          instance bool  get_IsOpen() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Transport.TTransport Thrift.Transport.TFramedTransport::transport
    IL_0006:  callvirt   instance bool Thrift.Transport.TTransport::get_IsOpen()
    IL_000b:  ret
  } // end of method TFramedTransport::get_IsOpen

  .method public hidebysig virtual instance void 
          Open() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Transport.TTransport Thrift.Transport.TFramedTransport::transport
    IL_0006:  callvirt   instance void Thrift.Transport.TTransport::Open()
    IL_000b:  ret
  } // end of method TFramedTransport::Open

  .method public hidebysig virtual instance void 
          Close() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Thrift.Transport.TTransport Thrift.Transport.TFramedTransport::transport
    IL_0006:  callvirt   instance void Thrift.Transport.TTransport::Close()
    IL_000b:  ret
  } // end of method TFramedTransport::Close

  .method public hidebysig virtual instance int32 
          Read(uint8[] buf,
               int32 'off',
               int32 len) cil managed
  {
    // Code size       56 (0x38)
    .maxstack  4
    .locals init (int32 V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TFramedTransport::readBuffer
    IL_0006:  brfalse    IL_0023

    IL_000b:  ldarg.0
    IL_000c:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TFramedTransport::readBuffer
    IL_0011:  ldarg.1
    IL_0012:  ldarg.2
    IL_0013:  ldarg.3
    IL_0014:  callvirt   instance int32 [mscorlib]System.IO.Stream::Read(uint8[],
                                                                         int32,
                                                                         int32)
    IL_0019:  stloc.0
    IL_001a:  ldloc.0
    IL_001b:  ldc.i4.0
    IL_001c:  ble        IL_0023

    IL_0021:  ldloc.0
    IL_0022:  ret

    IL_0023:  ldarg.0
    IL_0024:  call       instance void Thrift.Transport.TFramedTransport::ReadFrame()
    IL_0029:  ldarg.0
    IL_002a:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TFramedTransport::readBuffer
    IL_002f:  ldarg.1
    IL_0030:  ldarg.2
    IL_0031:  ldarg.3
    IL_0032:  callvirt   instance int32 [mscorlib]System.IO.Stream::Read(uint8[],
                                                                         int32,
                                                                         int32)
    IL_0037:  ret
  } // end of method TFramedTransport::Read

  .method private hidebysig instance void 
          ReadFrame() cil managed
  {
    // Code size       64 (0x40)
    .maxstack  4
    .locals init (uint8[] V_0,
             int32 V_1,
             uint8[] V_2)
    IL_0000:  ldc.i4.4
    IL_0001:  newarr     [mscorlib]System.Byte
    IL_0006:  stloc.0
    IL_0007:  ldarg.0
    IL_0008:  ldfld      class Thrift.Transport.TTransport Thrift.Transport.TFramedTransport::transport
    IL_000d:  ldloc.0
    IL_000e:  ldc.i4.0
    IL_000f:  ldc.i4.4
    IL_0010:  callvirt   instance int32 Thrift.Transport.TTransport::ReadAll(uint8[],
                                                                             int32,
                                                                             int32)
    IL_0015:  pop
    IL_0016:  ldloc.0
    IL_0017:  call       int32 Thrift.Transport.TFramedTransport::DecodeFrameSize(uint8[])
    IL_001c:  stloc.1
    IL_001d:  ldloc.1
    IL_001e:  newarr     [mscorlib]System.Byte
    IL_0023:  stloc.2
    IL_0024:  ldarg.0
    IL_0025:  ldfld      class Thrift.Transport.TTransport Thrift.Transport.TFramedTransport::transport
    IL_002a:  ldloc.2
    IL_002b:  ldc.i4.0
    IL_002c:  ldloc.1
    IL_002d:  callvirt   instance int32 Thrift.Transport.TTransport::ReadAll(uint8[],
                                                                             int32,
                                                                             int32)
    IL_0032:  pop
    IL_0033:  ldarg.0
    IL_0034:  ldloc.2
    IL_0035:  newobj     instance void [mscorlib]System.IO.MemoryStream::.ctor(uint8[])
    IL_003a:  stfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TFramedTransport::readBuffer
    IL_003f:  ret
  } // end of method TFramedTransport::ReadFrame

  .method public hidebysig virtual instance void 
          Write(uint8[] buf,
                int32 'off',
                int32 len) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TFramedTransport::writeBuffer
    IL_0006:  ldarg.1
    IL_0007:  ldarg.2
    IL_0008:  ldarg.3
    IL_0009:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                         int32,
                                                                         int32)
    IL_000e:  ret
  } // end of method TFramedTransport::Write

  .method public hidebysig virtual instance void 
          Flush() cil managed
  {
    // Code size       82 (0x52)
    .maxstack  4
    .locals init (uint8[] V_0,
             int32 V_1,
             int32 V_2)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TFramedTransport::writeBuffer
    IL_0006:  callvirt   instance uint8[] [mscorlib]System.IO.MemoryStream::GetBuffer()
    IL_000b:  stloc.0
    IL_000c:  ldarg.0
    IL_000d:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TFramedTransport::writeBuffer
    IL_0012:  callvirt   instance int64 [mscorlib]System.IO.Stream::get_Length()
    IL_0017:  conv.i4
    IL_0018:  stloc.1
    IL_0019:  ldloc.1
    IL_001a:  ldc.i4.4
    IL_001b:  sub
    IL_001c:  stloc.2
    IL_001d:  ldloc.2
    IL_001e:  ldc.i4.0
    IL_001f:  bge        IL_002a

    IL_0024:  newobj     instance void [mscorlib]System.InvalidOperationException::.ctor()
    IL_0029:  throw

    IL_002a:  ldarg.0
    IL_002b:  call       instance void Thrift.Transport.TFramedTransport::InitWriteBuffer()
    IL_0030:  ldloc.2
    IL_0031:  ldloca.s   V_0
    IL_0033:  call       void Thrift.Transport.TFramedTransport::EncodeFrameSize(int32,
                                                                                 uint8[]&)
    IL_0038:  ldarg.0
    IL_0039:  ldfld      class Thrift.Transport.TTransport Thrift.Transport.TFramedTransport::transport
    IL_003e:  ldloc.0
    IL_003f:  ldc.i4.0
    IL_0040:  ldloc.1
    IL_0041:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_0046:  ldarg.0
    IL_0047:  ldfld      class Thrift.Transport.TTransport Thrift.Transport.TFramedTransport::transport
    IL_004c:  callvirt   instance void Thrift.Transport.TTransport::Flush()
    IL_0051:  ret
  } // end of method TFramedTransport::Flush

  .method private hidebysig instance void 
          InitWriteBuffer() cil managed
  {
    // Code size       35 (0x23)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4     0x400
    IL_0006:  newobj     instance void [mscorlib]System.IO.MemoryStream::.ctor(int32)
    IL_000b:  stfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TFramedTransport::writeBuffer
    IL_0010:  ldarg.0
    IL_0011:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TFramedTransport::writeBuffer
    IL_0016:  ldsfld     uint8[] Thrift.Transport.TFramedTransport::header_dummy
    IL_001b:  ldc.i4.0
    IL_001c:  ldc.i4.4
    IL_001d:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                         int32,
                                                                         int32)
    IL_0022:  ret
  } // end of method TFramedTransport::InitWriteBuffer

  .method private hidebysig static void  EncodeFrameSize(int32 frameSize,
                                                         uint8[]& buf) cil managed
  {
    // Code size       57 (0x39)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  ldind.ref
    IL_0002:  ldc.i4.0
    IL_0003:  ldc.i4     0xff
    IL_0008:  ldarg.0
    IL_0009:  ldc.i4.s   24
    IL_000b:  shr
    IL_000c:  and
    IL_000d:  conv.u1
    IL_000e:  stelem.i1
    IL_000f:  ldarg.1
    IL_0010:  ldind.ref
    IL_0011:  ldc.i4.1
    IL_0012:  ldc.i4     0xff
    IL_0017:  ldarg.0
    IL_0018:  ldc.i4.s   16
    IL_001a:  shr
    IL_001b:  and
    IL_001c:  conv.u1
    IL_001d:  stelem.i1
    IL_001e:  ldarg.1
    IL_001f:  ldind.ref
    IL_0020:  ldc.i4.2
    IL_0021:  ldc.i4     0xff
    IL_0026:  ldarg.0
    IL_0027:  ldc.i4.8
    IL_0028:  shr
    IL_0029:  and
    IL_002a:  conv.u1
    IL_002b:  stelem.i1
    IL_002c:  ldarg.1
    IL_002d:  ldind.ref
    IL_002e:  ldc.i4.3
    IL_002f:  ldc.i4     0xff
    IL_0034:  ldarg.0
    IL_0035:  and
    IL_0036:  conv.u1
    IL_0037:  stelem.i1
    IL_0038:  ret
  } // end of method TFramedTransport::EncodeFrameSize

  .method private hidebysig static int32 
          DecodeFrameSize(uint8[] buf) cil managed
  {
    // Code size       48 (0x30)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.0
    IL_0002:  ldelem.u1
    IL_0003:  ldc.i4     0xff
    IL_0008:  and
    IL_0009:  ldc.i4.s   24
    IL_000b:  shl
    IL_000c:  ldarg.0
    IL_000d:  ldc.i4.1
    IL_000e:  ldelem.u1
    IL_000f:  ldc.i4     0xff
    IL_0014:  and
    IL_0015:  ldc.i4.s   16
    IL_0017:  shl
    IL_0018:  or
    IL_0019:  ldarg.0
    IL_001a:  ldc.i4.2
    IL_001b:  ldelem.u1
    IL_001c:  ldc.i4     0xff
    IL_0021:  and
    IL_0022:  ldc.i4.8
    IL_0023:  shl
    IL_0024:  or
    IL_0025:  ldarg.0
    IL_0026:  ldc.i4.3
    IL_0027:  ldelem.u1
    IL_0028:  ldc.i4     0xff
    IL_002d:  and
    IL_002e:  or
    IL_002f:  ret
  } // end of method TFramedTransport::DecodeFrameSize

  .method family hidebysig virtual instance void 
          Dispose(bool disposing) cil managed
  {
    // Code size       47 (0x2f)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      bool Thrift.Transport.TFramedTransport::_IsDisposed
    IL_0006:  brtrue     IL_0027

    IL_000b:  ldarg.1
    IL_000c:  brfalse    IL_0027

    IL_0011:  ldarg.0
    IL_0012:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TFramedTransport::readBuffer
    IL_0017:  brfalse    IL_0027

    IL_001c:  ldarg.0
    IL_001d:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TFramedTransport::readBuffer
    IL_0022:  callvirt   instance void [mscorlib]System.IO.Stream::Dispose()
    IL_0027:  ldarg.0
    IL_0028:  ldc.i4.1
    IL_0029:  stfld      bool Thrift.Transport.TFramedTransport::_IsDisposed
    IL_002e:  ret
  } // end of method TFramedTransport::Dispose

  .method private hidebysig specialname rtspecialname static 
          void  .cctor() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldc.i4.4
    IL_0001:  newarr     [mscorlib]System.Byte
    IL_0006:  stsfld     uint8[] Thrift.Transport.TFramedTransport::header_dummy
    IL_000b:  ret
  } // end of method TFramedTransport::.cctor

  .property instance bool IsOpen()
  {
    .get instance bool Thrift.Transport.TFramedTransport::get_IsOpen()
  } // end of property TFramedTransport::IsOpen
} // end of class Thrift.Transport.TFramedTransport

.class public auto ansi beforefieldinit Thrift.Transport.THttpClient
       extends Thrift.Transport.TTransport
       implements [mscorlib]System.IDisposable
{
  .class auto ansi nested private beforefieldinit FlushAsyncResult
         extends [mscorlib]System.Object
         implements [mscorlib]System.IAsyncResult
  {
    .field private bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) _isCompleted
    .field private class [mscorlib]System.Threading.ManualResetEvent _evt
    .field private initonly class [mscorlib]System.AsyncCallback _cbMethod
    .field private initonly object _state
    .field private uint8[] '<Data>k__BackingField'
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field private class [System]System.Net.HttpWebRequest '<Connection>k__BackingField'
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field private class Thrift.Transport.TTransportException '<AsyncException>k__BackingField'
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field private initonly object _locker
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor(class [mscorlib]System.AsyncCallback cbMethod,
                                 object state) cil managed
    {
      // Code size       32 (0x20)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  newobj     instance void [mscorlib]System.Object::.ctor()
      IL_0006:  stfld      object Thrift.Transport.THttpClient/FlushAsyncResult::_locker
      IL_000b:  ldarg.0
      IL_000c:  call       instance void [mscorlib]System.Object::.ctor()
      IL_0011:  ldarg.0
      IL_0012:  ldarg.1
      IL_0013:  stfld      class [mscorlib]System.AsyncCallback Thrift.Transport.THttpClient/FlushAsyncResult::_cbMethod
      IL_0018:  ldarg.0
      IL_0019:  ldarg.2
      IL_001a:  stfld      object Thrift.Transport.THttpClient/FlushAsyncResult::_state
      IL_001f:  ret
    } // end of method FlushAsyncResult::.ctor

    .method assembly hidebysig specialname 
            instance uint8[]  get_Data() cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      uint8[] Thrift.Transport.THttpClient/FlushAsyncResult::'<Data>k__BackingField'
      IL_0006:  ret
    } // end of method FlushAsyncResult::get_Data

    .method assembly hidebysig specialname 
            instance void  set_Data(uint8[] 'value') cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
      // Code size       8 (0x8)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldarg.1
      IL_0002:  stfld      uint8[] Thrift.Transport.THttpClient/FlushAsyncResult::'<Data>k__BackingField'
      IL_0007:  ret
    } // end of method FlushAsyncResult::set_Data

    .method assembly hidebysig specialname 
            instance class [System]System.Net.HttpWebRequest 
            get_Connection() cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      class [System]System.Net.HttpWebRequest Thrift.Transport.THttpClient/FlushAsyncResult::'<Connection>k__BackingField'
      IL_0006:  ret
    } // end of method FlushAsyncResult::get_Connection

    .method assembly hidebysig specialname 
            instance void  set_Connection(class [System]System.Net.HttpWebRequest 'value') cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
      // Code size       8 (0x8)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldarg.1
      IL_0002:  stfld      class [System]System.Net.HttpWebRequest Thrift.Transport.THttpClient/FlushAsyncResult::'<Connection>k__BackingField'
      IL_0007:  ret
    } // end of method FlushAsyncResult::set_Connection

    .method assembly hidebysig specialname 
            instance class Thrift.Transport.TTransportException 
            get_AsyncException() cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      class Thrift.Transport.TTransportException Thrift.Transport.THttpClient/FlushAsyncResult::'<AsyncException>k__BackingField'
      IL_0006:  ret
    } // end of method FlushAsyncResult::get_AsyncException

    .method assembly hidebysig specialname 
            instance void  set_AsyncException(class Thrift.Transport.TTransportException 'value') cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
      // Code size       8 (0x8)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldarg.1
      IL_0002:  stfld      class Thrift.Transport.TTransportException Thrift.Transport.THttpClient/FlushAsyncResult::'<AsyncException>k__BackingField'
      IL_0007:  ret
    } // end of method FlushAsyncResult::set_AsyncException

    .method public hidebysig newslot specialname virtual final 
            instance object  get_AsyncState() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      object Thrift.Transport.THttpClient/FlushAsyncResult::_state
      IL_0006:  ret
    } // end of method FlushAsyncResult::get_AsyncState

    .method public hidebysig newslot specialname virtual final 
            instance class [mscorlib]System.Threading.WaitHandle 
            get_AsyncWaitHandle() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.THttpClient/FlushAsyncResult::GetEvtHandle()
      IL_0006:  ret
    } // end of method FlushAsyncResult::get_AsyncWaitHandle

    .method public hidebysig newslot specialname virtual final 
            instance bool  get_CompletedSynchronously() cil managed
    {
      // Code size       2 (0x2)
      .maxstack  8
      IL_0000:  ldc.i4.0
      IL_0001:  ret
    } // end of method FlushAsyncResult::get_CompletedSynchronously

    .method public hidebysig newslot specialname virtual final 
            instance bool  get_IsCompleted() cil managed
    {
      // Code size       9 (0x9)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  volatile.
      IL_0003:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Transport.THttpClient/FlushAsyncResult::_isCompleted
      IL_0008:  ret
    } // end of method FlushAsyncResult::get_IsCompleted

    .method private hidebysig instance class [mscorlib]System.Threading.ManualResetEvent 
            GetEvtHandle() cil managed
    {
      // Code size       80 (0x50)
      .maxstack  2
      .locals init (object V_0)
      IL_0000:  ldarg.0
      IL_0001:  ldfld      object Thrift.Transport.THttpClient/FlushAsyncResult::_locker
      IL_0006:  stloc.0
      IL_0007:  ldloc.0
      IL_0008:  call       void [mscorlib]System.Threading.Monitor::Enter(object)
      .try
      {
        IL_000d:  ldarg.0
        IL_000e:  ldfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.THttpClient/FlushAsyncResult::_evt
        IL_0013:  brtrue     IL_0024

        IL_0018:  ldarg.0
        IL_0019:  ldc.i4.0
        IL_001a:  newobj     instance void [mscorlib]System.Threading.ManualResetEvent::.ctor(bool)
        IL_001f:  stfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.THttpClient/FlushAsyncResult::_evt
        IL_0024:  ldarg.0
        IL_0025:  volatile.
        IL_0027:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Transport.THttpClient/FlushAsyncResult::_isCompleted
        IL_002c:  brfalse    IL_003d

        IL_0031:  ldarg.0
        IL_0032:  ldfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.THttpClient/FlushAsyncResult::_evt
        IL_0037:  callvirt   instance bool [mscorlib]System.Threading.EventWaitHandle::Set()
        IL_003c:  pop
        IL_003d:  leave      IL_0049

      }  // end .try
      finally
      {
        IL_0042:  ldloc.0
        IL_0043:  call       void [mscorlib]System.Threading.Monitor::Exit(object)
        IL_0048:  endfinally
      }  // end handler
      IL_0049:  ldarg.0
      IL_004a:  ldfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.THttpClient/FlushAsyncResult::_evt
      IL_004f:  ret
    } // end of method FlushAsyncResult::GetEvtHandle

    .method assembly hidebysig instance void 
            UpdateStatusToComplete() cil managed
    {
      // Code size       58 (0x3a)
      .maxstack  2
      .locals init (object V_0)
      IL_0000:  ldarg.0
      IL_0001:  ldc.i4.1
      IL_0002:  volatile.
      IL_0004:  stfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Thrift.Transport.THttpClient/FlushAsyncResult::_isCompleted
      IL_0009:  ldarg.0
      IL_000a:  ldfld      object Thrift.Transport.THttpClient/FlushAsyncResult::_locker
      IL_000f:  stloc.0
      IL_0010:  ldloc.0
      IL_0011:  call       void [mscorlib]System.Threading.Monitor::Enter(object)
      .try
      {
        IL_0016:  ldarg.0
        IL_0017:  ldfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.THttpClient/FlushAsyncResult::_evt
        IL_001c:  brfalse    IL_002d

        IL_0021:  ldarg.0
        IL_0022:  ldfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.THttpClient/FlushAsyncResult::_evt
        IL_0027:  callvirt   instance bool [mscorlib]System.Threading.EventWaitHandle::Set()
        IL_002c:  pop
        IL_002d:  leave      IL_0039

      }  // end .try
      finally
      {
        IL_0032:  ldloc.0
        IL_0033:  call       void [mscorlib]System.Threading.Monitor::Exit(object)
        IL_0038:  endfinally
      }  // end handler
      IL_0039:  ret
    } // end of method FlushAsyncResult::UpdateStatusToComplete

    .method assembly hidebysig instance void 
            NotifyCallbackWhenAvailable() cil managed
    {
      // Code size       24 (0x18)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      class [mscorlib]System.AsyncCallback Thrift.Transport.THttpClient/FlushAsyncResult::_cbMethod
      IL_0006:  brfalse    IL_0017

      IL_000b:  ldarg.0
      IL_000c:  ldfld      class [mscorlib]System.AsyncCallback Thrift.Transport.THttpClient/FlushAsyncResult::_cbMethod
      IL_0011:  ldarg.0
      IL_0012:  callvirt   instance void [mscorlib]System.AsyncCallback::Invoke(class [mscorlib]System.IAsyncResult)
      IL_0017:  ret
    } // end of method FlushAsyncResult::NotifyCallbackWhenAvailable

    .property instance uint8[] Data()
    {
      .get instance uint8[] Thrift.Transport.THttpClient/FlushAsyncResult::get_Data()
      .set instance void Thrift.Transport.THttpClient/FlushAsyncResult::set_Data(uint8[])
    } // end of property FlushAsyncResult::Data
    .property instance class [System]System.Net.HttpWebRequest
            Connection()
    {
      .get instance class [System]System.Net.HttpWebRequest Thrift.Transport.THttpClient/FlushAsyncResult::get_Connection()
      .set instance void Thrift.Transport.THttpClient/FlushAsyncResult::set_Connection(class [System]System.Net.HttpWebRequest)
    } // end of property FlushAsyncResult::Connection
    .property instance class Thrift.Transport.TTransportException
            AsyncException()
    {
      .get instance class Thrift.Transport.TTransportException Thrift.Transport.THttpClient/FlushAsyncResult::get_AsyncException()
      .set instance void Thrift.Transport.THttpClient/FlushAsyncResult::set_AsyncException(class Thrift.Transport.TTransportException)
    } // end of property FlushAsyncResult::AsyncException
    .property instance object AsyncState()
    {
      .get instance object Thrift.Transport.THttpClient/FlushAsyncResult::get_AsyncState()
    } // end of property FlushAsyncResult::AsyncState
    .property instance class [mscorlib]System.Threading.WaitHandle
            AsyncWaitHandle()
    {
      .get instance class [mscorlib]System.Threading.WaitHandle Thrift.Transport.THttpClient/FlushAsyncResult::get_AsyncWaitHandle()
    } // end of property FlushAsyncResult::AsyncWaitHandle
    .property instance bool CompletedSynchronously()
    {
      .get instance bool Thrift.Transport.THttpClient/FlushAsyncResult::get_CompletedSynchronously()
    } // end of property FlushAsyncResult::CompletedSynchronously
    .property instance bool IsCompleted()
    {
      .get instance bool Thrift.Transport.THttpClient/FlushAsyncResult::get_IsCompleted()
    } // end of property FlushAsyncResult::IsCompleted
  } // end of class FlushAsyncResult

  .field private initonly class [System]System.Uri uri
  .field private initonly class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate[] certificates
  .field private class [mscorlib]System.IO.Stream inputStream
  .field private class [mscorlib]System.IO.MemoryStream outputStream
  .field private int32 connectTimeout
  .field private int32 readTimeout
  .field private class [mscorlib]System.Collections.Generic.IDictionary`2<string,string> customHeaders
  .field private class [System]System.Net.IWebProxy proxy
  .field private bool _IsDisposed
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class [System]System.Uri u) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0> [System.Core]System.Linq.Enumerable::Empty<class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate>()
    IL_0007:  call       instance void Thrift.Transport.THttpClient::.ctor(class [System]System.Uri,
                                                                           class [mscorlib]System.Collections.Generic.IEnumerable`1<class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate>)
    IL_000c:  ret
  } // end of method THttpClient::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class [System]System.Uri u,
                               class [mscorlib]System.Collections.Generic.IEnumerable`1<class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate> certificates) cil managed
  {
    // Code size       93 (0x5d)
    .maxstack  3
    IL_0000:  ldarg.0
    IL_0001:  newobj     instance void [mscorlib]System.IO.MemoryStream::.ctor()
    IL_0006:  stfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.THttpClient::outputStream
    IL_000b:  ldarg.0
    IL_000c:  ldc.i4     0x7530
    IL_0011:  stfld      int32 Thrift.Transport.THttpClient::connectTimeout
    IL_0016:  ldarg.0
    IL_0017:  ldc.i4     0x7530
    IL_001c:  stfld      int32 Thrift.Transport.THttpClient::readTimeout
    IL_0021:  ldarg.0
    IL_0022:  newobj     instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::.ctor()
    IL_0027:  stfld      class [mscorlib]System.Collections.Generic.IDictionary`2<string,string> Thrift.Transport.THttpClient::customHeaders
    IL_002c:  ldarg.0
    IL_002d:  call       class [System]System.Net.IWebProxy [System]System.Net.WebRequest::get_DefaultWebProxy()
    IL_0032:  stfld      class [System]System.Net.IWebProxy Thrift.Transport.THttpClient::proxy
    IL_0037:  ldarg.0
    IL_0038:  call       instance void Thrift.Transport.TTransport::.ctor()
    IL_003d:  ldarg.0
    IL_003e:  ldarg.1
    IL_003f:  stfld      class [System]System.Uri Thrift.Transport.THttpClient::uri
    IL_0044:  ldarg.0
    IL_0045:  ldarg.2
    IL_0046:  dup
    IL_0047:  brtrue     IL_0052

    IL_004c:  pop
    IL_004d:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0> [System.Core]System.Linq.Enumerable::Empty<class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate>()
    IL_0052:  call       !!0[] [System.Core]System.Linq.Enumerable::ToArray<class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_0057:  stfld      class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate[] Thrift.Transport.THttpClient::certificates
    IL_005c:  ret
  } // end of method THttpClient::.ctor

  .method public hidebysig specialname instance void 
          set_ConnectTimeout(int32 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      int32 Thrift.Transport.THttpClient::connectTimeout
    IL_0007:  ret
  } // end of method THttpClient::set_ConnectTimeout

  .method public hidebysig specialname instance void 
          set_ReadTimeout(int32 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      int32 Thrift.Transport.THttpClient::readTimeout
    IL_0007:  ret
  } // end of method THttpClient::set_ReadTimeout

  .method public hidebysig specialname instance class [mscorlib]System.Collections.Generic.IDictionary`2<string,string> 
          get_CustomHeaders() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.Collections.Generic.IDictionary`2<string,string> Thrift.Transport.THttpClient::customHeaders
    IL_0006:  ret
  } // end of method THttpClient::get_CustomHeaders

  .method public hidebysig specialname instance void 
          set_Proxy(class [System]System.Net.IWebProxy 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      class [System]System.Net.IWebProxy Thrift.Transport.THttpClient::proxy
    IL_0007:  ret
  } // end of method THttpClient::set_Proxy

  .method public hidebysig specialname virtual 
          instance bool  get_IsOpen() cil managed
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldc.i4.1
    IL_0001:  ret
  } // end of method THttpClient::get_IsOpen

  .method public hidebysig virtual instance void 
          Open() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method THttpClient::Open

  .method public hidebysig virtual instance void 
          Close() cil managed
  {
    // Code size       59 (0x3b)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.THttpClient::inputStream
    IL_0006:  brfalse    IL_001d

    IL_000b:  ldarg.0
    IL_000c:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.THttpClient::inputStream
    IL_0011:  callvirt   instance void [mscorlib]System.IO.Stream::Close()
    IL_0016:  ldarg.0
    IL_0017:  ldnull
    IL_0018:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.THttpClient::inputStream
    IL_001d:  ldarg.0
    IL_001e:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.THttpClient::outputStream
    IL_0023:  brfalse    IL_003a

    IL_0028:  ldarg.0
    IL_0029:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.THttpClient::outputStream
    IL_002e:  callvirt   instance void [mscorlib]System.IO.Stream::Close()
    IL_0033:  ldarg.0
    IL_0034:  ldnull
    IL_0035:  stfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.THttpClient::outputStream
    IL_003a:  ret
  } // end of method THttpClient::Close

  .method public hidebysig virtual instance int32 
          Read(uint8[] buf,
               int32 'off',
               int32 len) cil managed
  {
    // Code size       80 (0x50)
    .maxstack  4
    .locals init (int32 V_0,
             int32 V_1,
             class [mscorlib]System.IO.IOException V_2)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.THttpClient::inputStream
    IL_0006:  brtrue     IL_0017

    IL_000b:  ldc.i4.1
    IL_000c:  ldstr      "No request has been sent"
    IL_0011:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_0016:  throw

    .try
    {
      IL_0017:  ldarg.0
      IL_0018:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.THttpClient::inputStream
      IL_001d:  ldarg.1
      IL_001e:  ldarg.2
      IL_001f:  ldarg.3
      IL_0020:  callvirt   instance int32 [mscorlib]System.IO.Stream::Read(uint8[],
                                                                           int32,
                                                                           int32)
      IL_0025:  stloc.0
      IL_0026:  ldloc.0
      IL_0027:  ldc.i4.m1
      IL_0028:  bne.un     IL_0039

      IL_002d:  ldc.i4.4
      IL_002e:  ldstr      "No more data available"
      IL_0033:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                     string)
      IL_0038:  throw

      IL_0039:  ldloc.0
      IL_003a:  stloc.1
      IL_003b:  leave      IL_004e

    }  // end .try
    catch [mscorlib]System.IO.IOException 
    {
      IL_0040:  stloc.2
      IL_0041:  ldc.i4.0
      IL_0042:  ldloc.2
      IL_0043:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_0048:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                     string)
      IL_004d:  throw

    }  // end handler
    IL_004e:  ldloc.1
    IL_004f:  ret
  } // end of method THttpClient::Read

  .method public hidebysig virtual instance void 
          Write(uint8[] buf,
                int32 'off',
                int32 len) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.THttpClient::outputStream
    IL_0006:  ldarg.1
    IL_0007:  ldarg.2
    IL_0008:  ldarg.3
    IL_0009:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                         int32,
                                                                         int32)
    IL_000e:  ret
  } // end of method THttpClient::Write

  .method public hidebysig virtual instance void 
          Flush() cil managed
  {
    // Code size       24 (0x18)
    .maxstack  2
    .try
    {
      IL_0000:  ldarg.0
      IL_0001:  call       instance void Thrift.Transport.THttpClient::SendRequest()
      IL_0006:  leave      IL_0017

    }  // end .try
    finally
    {
      IL_000b:  ldarg.0
      IL_000c:  newobj     instance void [mscorlib]System.IO.MemoryStream::.ctor()
      IL_0011:  stfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.THttpClient::outputStream
      IL_0016:  endfinally
    }  // end handler
    IL_0017:  ret
  } // end of method THttpClient::Flush

  .method private hidebysig instance void 
          SendRequest() cil managed
  {
    // Code size       243 (0xf3)
    .maxstack  4
    .locals init (class [System]System.Net.HttpWebRequest V_0,
             uint8[] V_1,
             class [mscorlib]System.IO.Stream V_2,
             class [System]System.Net.WebResponse V_3,
             class [mscorlib]System.IO.Stream V_4,
             uint8[] V_5,
             int32 V_6,
             class [mscorlib]System.IO.IOException V_7,
             class [System]System.Net.WebException V_8)
    .try
    {
      IL_0000:  ldarg.0
      IL_0001:  call       instance class [System]System.Net.HttpWebRequest Thrift.Transport.THttpClient::CreateRequest()
      IL_0006:  stloc.0
      IL_0007:  ldarg.0
      IL_0008:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.THttpClient::outputStream
      IL_000d:  callvirt   instance uint8[] [mscorlib]System.IO.MemoryStream::ToArray()
      IL_0012:  stloc.1
      IL_0013:  ldloc.0
      IL_0014:  ldloc.1
      IL_0015:  ldlen
      IL_0016:  conv.i4
      IL_0017:  conv.i8
      IL_0018:  callvirt   instance void [System]System.Net.WebRequest::set_ContentLength(int64)
      IL_001d:  ldloc.0
      IL_001e:  callvirt   instance class [mscorlib]System.IO.Stream [System]System.Net.WebRequest::GetRequestStream()
      IL_0023:  stloc.2
      .try
      {
        IL_0024:  ldloc.2
        IL_0025:  ldloc.1
        IL_0026:  ldc.i4.0
        IL_0027:  ldloc.1
        IL_0028:  ldlen
        IL_0029:  conv.i4
        IL_002a:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                             int32,
                                                                             int32)
        IL_002f:  ldloc.0
        IL_0030:  callvirt   instance class [System]System.Net.WebResponse [System]System.Net.WebRequest::GetResponse()
        IL_0035:  stloc.3
        .try
        {
          IL_0036:  ldloc.3
          IL_0037:  callvirt   instance class [mscorlib]System.IO.Stream [System]System.Net.WebResponse::GetResponseStream()
          IL_003c:  stloc.s    V_4
          .try
          {
            IL_003e:  ldarg.0
            IL_003f:  newobj     instance void [mscorlib]System.IO.MemoryStream::.ctor()
            IL_0044:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.THttpClient::inputStream
            IL_0049:  ldc.i4     0x1fa0
            IL_004e:  newarr     [mscorlib]System.Byte
            IL_0053:  stloc.s    V_5
            IL_0055:  br         IL_006a

            IL_005a:  ldarg.0
            IL_005b:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.THttpClient::inputStream
            IL_0060:  ldloc.s    V_5
            IL_0062:  ldc.i4.0
            IL_0063:  ldloc.s    V_6
            IL_0065:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                                 int32,
                                                                                 int32)
            IL_006a:  ldloc.s    V_4
            IL_006c:  ldloc.s    V_5
            IL_006e:  ldc.i4.0
            IL_006f:  ldloc.s    V_5
            IL_0071:  ldlen
            IL_0072:  conv.i4
            IL_0073:  callvirt   instance int32 [mscorlib]System.IO.Stream::Read(uint8[],
                                                                                 int32,
                                                                                 int32)
            IL_0078:  dup
            IL_0079:  stloc.s    V_6
            IL_007b:  ldc.i4.0
            IL_007c:  bgt        IL_005a

            IL_0081:  ldarg.0
            IL_0082:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.THttpClient::inputStream
            IL_0087:  ldc.i4.0
            IL_0088:  conv.i8
            IL_0089:  ldc.i4.0
            IL_008a:  callvirt   instance int64 [mscorlib]System.IO.Stream::Seek(int64,
                                                                                 valuetype [mscorlib]System.IO.SeekOrigin)
            IL_008f:  pop
            IL_0090:  leave      IL_00a4

          }  // end .try
          finally
          {
            IL_0095:  ldloc.s    V_4
            IL_0097:  brfalse    IL_00a3

            IL_009c:  ldloc.s    V_4
            IL_009e:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
            IL_00a3:  endfinally
          }  // end handler
          IL_00a4:  leave      IL_00b6

        }  // end .try
        finally
        {
          IL_00a9:  ldloc.3
          IL_00aa:  brfalse    IL_00b5

          IL_00af:  ldloc.3
          IL_00b0:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
          IL_00b5:  endfinally
        }  // end handler
        IL_00b6:  leave      IL_00c8

      }  // end .try
      finally
      {
        IL_00bb:  ldloc.2
        IL_00bc:  brfalse    IL_00c7

        IL_00c1:  ldloc.2
        IL_00c2:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_00c7:  endfinally
      }  // end handler
      IL_00c8:  leave      IL_00f2

    }  // end .try
    catch [mscorlib]System.IO.IOException 
    {
      IL_00cd:  stloc.s    V_7
      IL_00cf:  ldc.i4.0
      IL_00d0:  ldloc.s    V_7
      IL_00d2:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_00d7:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                     string)
      IL_00dc:  throw

    }  // end handler
    catch [System]System.Net.WebException 
    {
      IL_00dd:  stloc.s    V_8
      IL_00df:  ldc.i4.0
      IL_00e0:  ldstr      "Couldn't connect to server: "
      IL_00e5:  ldloc.s    V_8
      IL_00e7:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_00ec:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                     string)
      IL_00f1:  throw

    }  // end handler
    IL_00f2:  ret
  } // end of method THttpClient::SendRequest

  .method private hidebysig instance class [System]System.Net.HttpWebRequest 
          CreateRequest() cil managed
  {
    // Code size       229 (0xe5)
    .maxstack  3
    .locals init (class [System]System.Net.HttpWebRequest V_0,
             valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string> V_1,
             class [mscorlib]System.Collections.Generic.IEnumerator`1<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>> V_2)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Uri Thrift.Transport.THttpClient::uri
    IL_0006:  call       class [System]System.Net.WebRequest [System]System.Net.WebRequest::Create(class [System]System.Uri)
    IL_000b:  castclass  [System]System.Net.HttpWebRequest
    IL_0010:  stloc.0
    IL_0011:  ldloc.0
    IL_0012:  callvirt   instance class [System]System.Security.Cryptography.X509Certificates.X509CertificateCollection [System]System.Net.HttpWebRequest::get_ClientCertificates()
    IL_0017:  ldarg.0
    IL_0018:  ldfld      class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate[] Thrift.Transport.THttpClient::certificates
    IL_001d:  callvirt   instance void [System]System.Security.Cryptography.X509Certificates.X509CertificateCollection::AddRange(class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate[])
    IL_0022:  ldarg.0
    IL_0023:  ldfld      int32 Thrift.Transport.THttpClient::connectTimeout
    IL_0028:  ldc.i4.0
    IL_0029:  ble        IL_003a

    IL_002e:  ldloc.0
    IL_002f:  ldarg.0
    IL_0030:  ldfld      int32 Thrift.Transport.THttpClient::connectTimeout
    IL_0035:  callvirt   instance void [System]System.Net.WebRequest::set_Timeout(int32)
    IL_003a:  ldarg.0
    IL_003b:  ldfld      int32 Thrift.Transport.THttpClient::readTimeout
    IL_0040:  ldc.i4.0
    IL_0041:  ble        IL_0052

    IL_0046:  ldloc.0
    IL_0047:  ldarg.0
    IL_0048:  ldfld      int32 Thrift.Transport.THttpClient::readTimeout
    IL_004d:  callvirt   instance void [System]System.Net.HttpWebRequest::set_ReadWriteTimeout(int32)
    IL_0052:  ldloc.0
    IL_0053:  ldstr      "application/x-thrift"
    IL_0058:  callvirt   instance void [System]System.Net.WebRequest::set_ContentType(string)
    IL_005d:  ldloc.0
    IL_005e:  ldstr      "application/x-thrift"
    IL_0063:  callvirt   instance void [System]System.Net.HttpWebRequest::set_Accept(string)
    IL_0068:  ldloc.0
    IL_0069:  ldstr      "C#/THttpClient"
    IL_006e:  callvirt   instance void [System]System.Net.HttpWebRequest::set_UserAgent(string)
    IL_0073:  ldloc.0
    IL_0074:  ldstr      "POST"
    IL_0079:  callvirt   instance void [System]System.Net.WebRequest::set_Method(string)
    IL_007e:  ldloc.0
    IL_007f:  ldsfld     class [mscorlib]System.Version [System]System.Net.HttpVersion::Version10
    IL_0084:  callvirt   instance void [System]System.Net.HttpWebRequest::set_ProtocolVersion(class [mscorlib]System.Version)
    IL_0089:  ldarg.0
    IL_008a:  ldfld      class [mscorlib]System.Collections.Generic.IDictionary`2<string,string> Thrift.Transport.THttpClient::customHeaders
    IL_008f:  callvirt   instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>>::GetEnumerator()
    IL_0094:  stloc.2
    .try
    {
      IL_0095:  br         IL_00ba

      IL_009a:  ldloc.2
      IL_009b:  callvirt   instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>>::get_Current()
      IL_00a0:  stloc.1
      IL_00a1:  ldloc.0
      IL_00a2:  callvirt   instance class [System]System.Net.WebHeaderCollection [System]System.Net.WebRequest::get_Headers()
      IL_00a7:  ldloca.s   V_1
      IL_00a9:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
      IL_00ae:  ldloca.s   V_1
      IL_00b0:  call       instance !1 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Value()
      IL_00b5:  callvirt   instance void [System]System.Collections.Specialized.NameValueCollection::Add(string,
                                                                                                         string)
      IL_00ba:  ldloc.2
      IL_00bb:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
      IL_00c0:  brtrue     IL_009a

      IL_00c5:  leave      IL_00d7

    }  // end .try
    finally
    {
      IL_00ca:  ldloc.2
      IL_00cb:  brfalse    IL_00d6

      IL_00d0:  ldloc.2
      IL_00d1:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_00d6:  endfinally
    }  // end handler
    IL_00d7:  ldloc.0
    IL_00d8:  ldarg.0
    IL_00d9:  ldfld      class [System]System.Net.IWebProxy Thrift.Transport.THttpClient::proxy
    IL_00de:  callvirt   instance void [System]System.Net.WebRequest::set_Proxy(class [System]System.Net.IWebProxy)
    IL_00e3:  ldloc.0
    IL_00e4:  ret
  } // end of method THttpClient::CreateRequest

  .method public hidebysig virtual instance class [mscorlib]System.IAsyncResult 
          BeginFlush(class [mscorlib]System.AsyncCallback callback,
                     object state) cil managed
  {
    // Code size       86 (0x56)
    .maxstack  3
    .locals init (uint8[] V_0,
             class Thrift.Transport.THttpClient/FlushAsyncResult V_1,
             class [mscorlib]System.IAsyncResult V_2,
             class [mscorlib]System.IO.IOException V_3)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.THttpClient::outputStream
    IL_0006:  callvirt   instance uint8[] [mscorlib]System.IO.MemoryStream::ToArray()
    IL_000b:  stloc.0
    .try
    {
      IL_000c:  ldarg.1
      IL_000d:  ldarg.2
      IL_000e:  newobj     instance void Thrift.Transport.THttpClient/FlushAsyncResult::.ctor(class [mscorlib]System.AsyncCallback,
                                                                                              object)
      IL_0013:  stloc.1
      IL_0014:  ldloc.1
      IL_0015:  ldarg.0
      IL_0016:  call       instance class [System]System.Net.HttpWebRequest Thrift.Transport.THttpClient::CreateRequest()
      IL_001b:  callvirt   instance void Thrift.Transport.THttpClient/FlushAsyncResult::set_Connection(class [System]System.Net.HttpWebRequest)
      IL_0020:  ldloc.1
      IL_0021:  ldloc.0
      IL_0022:  callvirt   instance void Thrift.Transport.THttpClient/FlushAsyncResult::set_Data(uint8[])
      IL_0027:  ldloc.1
      IL_0028:  callvirt   instance class [System]System.Net.HttpWebRequest Thrift.Transport.THttpClient/FlushAsyncResult::get_Connection()
      IL_002d:  ldarg.0
      IL_002e:  ldftn      instance void Thrift.Transport.THttpClient::GetRequestStreamCallback(class [mscorlib]System.IAsyncResult)
      IL_0034:  newobj     instance void [mscorlib]System.AsyncCallback::.ctor(object,
                                                                               native int)
      IL_0039:  ldloc.1
      IL_003a:  callvirt   instance class [mscorlib]System.IAsyncResult [System]System.Net.WebRequest::BeginGetRequestStream(class [mscorlib]System.AsyncCallback,
                                                                                                                             object)
      IL_003f:  pop
      IL_0040:  ldloc.1
      IL_0041:  stloc.2
      IL_0042:  leave      IL_0054

    }  // end .try
    catch [mscorlib]System.IO.IOException 
    {
      IL_0047:  stloc.3
      IL_0048:  ldloc.3
      IL_0049:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_004e:  newobj     instance void Thrift.Transport.TTransportException::.ctor(string)
      IL_0053:  throw

    }  // end handler
    IL_0054:  ldloc.2
    IL_0055:  ret
  } // end of method THttpClient::BeginFlush

  .method public hidebysig virtual instance void 
          EndFlush(class [mscorlib]System.IAsyncResult asyncResult) cil managed
  {
    // Code size       74 (0x4a)
    .maxstack  2
    .locals init (class Thrift.Transport.THttpClient/FlushAsyncResult V_0,
             class [mscorlib]System.Threading.WaitHandle V_1)
    .try
    {
      IL_0000:  ldarg.1
      IL_0001:  castclass  Thrift.Transport.THttpClient/FlushAsyncResult
      IL_0006:  stloc.0
      IL_0007:  ldloc.0
      IL_0008:  callvirt   instance bool Thrift.Transport.THttpClient/FlushAsyncResult::get_IsCompleted()
      IL_000d:  brtrue     IL_0026

      IL_0012:  ldloc.0
      IL_0013:  callvirt   instance class [mscorlib]System.Threading.WaitHandle Thrift.Transport.THttpClient/FlushAsyncResult::get_AsyncWaitHandle()
      IL_0018:  stloc.1
      IL_0019:  ldloc.1
      IL_001a:  callvirt   instance bool [mscorlib]System.Threading.WaitHandle::WaitOne()
      IL_001f:  pop
      IL_0020:  ldloc.1
      IL_0021:  callvirt   instance void [mscorlib]System.Threading.WaitHandle::Close()
      IL_0026:  ldloc.0
      IL_0027:  callvirt   instance class Thrift.Transport.TTransportException Thrift.Transport.THttpClient/FlushAsyncResult::get_AsyncException()
      IL_002c:  brfalse    IL_0038

      IL_0031:  ldloc.0
      IL_0032:  callvirt   instance class Thrift.Transport.TTransportException Thrift.Transport.THttpClient/FlushAsyncResult::get_AsyncException()
      IL_0037:  throw

      IL_0038:  leave      IL_0049

    }  // end .try
    finally
    {
      IL_003d:  ldarg.0
      IL_003e:  newobj     instance void [mscorlib]System.IO.MemoryStream::.ctor()
      IL_0043:  stfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.THttpClient::outputStream
      IL_0048:  endfinally
    }  // end handler
    IL_0049:  ret
  } // end of method THttpClient::EndFlush

  .method private hidebysig instance void 
          GetRequestStreamCallback(class [mscorlib]System.IAsyncResult asynchronousResult) cil managed
  {
    // Code size       124 (0x7c)
    .maxstack  4
    .locals init (class Thrift.Transport.THttpClient/FlushAsyncResult V_0,
             class [mscorlib]System.IO.Stream V_1,
             class [mscorlib]System.Exception V_2)
    IL_0000:  ldarg.1
    IL_0001:  callvirt   instance object [mscorlib]System.IAsyncResult::get_AsyncState()
    IL_0006:  castclass  Thrift.Transport.THttpClient/FlushAsyncResult
    IL_000b:  stloc.0
    .try
    {
      IL_000c:  ldloc.0
      IL_000d:  callvirt   instance class [System]System.Net.HttpWebRequest Thrift.Transport.THttpClient/FlushAsyncResult::get_Connection()
      IL_0012:  ldarg.1
      IL_0013:  callvirt   instance class [mscorlib]System.IO.Stream [System]System.Net.WebRequest::EndGetRequestStream(class [mscorlib]System.IAsyncResult)
      IL_0018:  stloc.1
      IL_0019:  ldloc.1
      IL_001a:  ldloc.0
      IL_001b:  callvirt   instance uint8[] Thrift.Transport.THttpClient/FlushAsyncResult::get_Data()
      IL_0020:  ldc.i4.0
      IL_0021:  ldloc.0
      IL_0022:  callvirt   instance uint8[] Thrift.Transport.THttpClient/FlushAsyncResult::get_Data()
      IL_0027:  ldlen
      IL_0028:  conv.i4
      IL_0029:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                           int32,
                                                                           int32)
      IL_002e:  ldloc.1
      IL_002f:  callvirt   instance void [mscorlib]System.IO.Stream::Flush()
      IL_0034:  ldloc.1
      IL_0035:  callvirt   instance void [mscorlib]System.IO.Stream::Close()
      IL_003a:  ldloc.0
      IL_003b:  callvirt   instance class [System]System.Net.HttpWebRequest Thrift.Transport.THttpClient/FlushAsyncResult::get_Connection()
      IL_0040:  ldarg.0
      IL_0041:  ldftn      instance void Thrift.Transport.THttpClient::GetResponseCallback(class [mscorlib]System.IAsyncResult)
      IL_0047:  newobj     instance void [mscorlib]System.AsyncCallback::.ctor(object,
                                                                               native int)
      IL_004c:  ldloc.0
      IL_004d:  callvirt   instance class [mscorlib]System.IAsyncResult [System]System.Net.WebRequest::BeginGetResponse(class [mscorlib]System.AsyncCallback,
                                                                                                                        object)
      IL_0052:  pop
      IL_0053:  leave      IL_007b

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_0058:  stloc.2
      IL_0059:  ldloc.0
      IL_005a:  ldloc.2
      IL_005b:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_0060:  newobj     instance void Thrift.Transport.TTransportException::.ctor(string)
      IL_0065:  callvirt   instance void Thrift.Transport.THttpClient/FlushAsyncResult::set_AsyncException(class Thrift.Transport.TTransportException)
      IL_006a:  ldloc.0
      IL_006b:  callvirt   instance void Thrift.Transport.THttpClient/FlushAsyncResult::UpdateStatusToComplete()
      IL_0070:  ldloc.0
      IL_0071:  callvirt   instance void Thrift.Transport.THttpClient/FlushAsyncResult::NotifyCallbackWhenAvailable()
      IL_0076:  leave      IL_007b

    }  // end handler
    IL_007b:  ret
  } // end of method THttpClient::GetRequestStreamCallback

  .method private hidebysig instance void 
          GetResponseCallback(class [mscorlib]System.IAsyncResult asynchronousResult) cil managed
  {
    // Code size       76 (0x4c)
    .maxstack  3
    .locals init (class Thrift.Transport.THttpClient/FlushAsyncResult V_0,
             class [mscorlib]System.Exception V_1)
    IL_0000:  ldarg.1
    IL_0001:  callvirt   instance object [mscorlib]System.IAsyncResult::get_AsyncState()
    IL_0006:  castclass  Thrift.Transport.THttpClient/FlushAsyncResult
    IL_000b:  stloc.0
    .try
    {
      IL_000c:  ldarg.0
      IL_000d:  ldloc.0
      IL_000e:  callvirt   instance class [System]System.Net.HttpWebRequest Thrift.Transport.THttpClient/FlushAsyncResult::get_Connection()
      IL_0013:  ldarg.1
      IL_0014:  callvirt   instance class [System]System.Net.WebResponse [System]System.Net.WebRequest::EndGetResponse(class [mscorlib]System.IAsyncResult)
      IL_0019:  callvirt   instance class [mscorlib]System.IO.Stream [System]System.Net.WebResponse::GetResponseStream()
      IL_001e:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.THttpClient::inputStream
      IL_0023:  leave      IL_003f

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_0028:  stloc.1
      IL_0029:  ldloc.0
      IL_002a:  ldloc.1
      IL_002b:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_0030:  newobj     instance void Thrift.Transport.TTransportException::.ctor(string)
      IL_0035:  callvirt   instance void Thrift.Transport.THttpClient/FlushAsyncResult::set_AsyncException(class Thrift.Transport.TTransportException)
      IL_003a:  leave      IL_003f

    }  // end handler
    IL_003f:  ldloc.0
    IL_0040:  callvirt   instance void Thrift.Transport.THttpClient/FlushAsyncResult::UpdateStatusToComplete()
    IL_0045:  ldloc.0
    IL_0046:  callvirt   instance void Thrift.Transport.THttpClient/FlushAsyncResult::NotifyCallbackWhenAvailable()
    IL_004b:  ret
  } // end of method THttpClient::GetResponseCallback

  .method family hidebysig virtual instance void 
          Dispose(bool disposing) cil managed
  {
    // Code size       69 (0x45)
    .maxstack  2
    IL_0000:  ldarg.0
    IL_0001:  ldfld      bool Thrift.Transport.THttpClient::_IsDisposed
    IL_0006:  brtrue     IL_003d

    IL_000b:  ldarg.1
    IL_000c:  brfalse    IL_003d

    IL_0011:  ldarg.0
    IL_0012:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.THttpClient::inputStream
    IL_0017:  brfalse    IL_0027

    IL_001c:  ldarg.0
    IL_001d:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.THttpClient::inputStream
    IL_0022:  callvirt   instance void [mscorlib]System.IO.Stream::Dispose()
    IL_0027:  ldarg.0
    IL_0028:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.THttpClient::outputStream
    IL_002d:  brfalse    IL_003d

    IL_0032:  ldarg.0
    IL_0033:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.THttpClient::outputStream
    IL_0038:  callvirt   instance void [mscorlib]System.IO.Stream::Dispose()
    IL_003d:  ldarg.0
    IL_003e:  ldc.i4.1
    IL_003f:  stfld      bool Thrift.Transport.THttpClient::_IsDisposed
    IL_0044:  ret
  } // end of method THttpClient::Dispose

  .property instance int32 ConnectTimeout()
  {
    .set instance void Thrift.Transport.THttpClient::set_ConnectTimeout(int32)
  } // end of property THttpClient::ConnectTimeout
  .property instance int32 ReadTimeout()
  {
    .set instance void Thrift.Transport.THttpClient::set_ReadTimeout(int32)
  } // end of property THttpClient::ReadTimeout
  .property instance class [mscorlib]System.Collections.Generic.IDictionary`2<string,string>
          CustomHeaders()
  {
    .get instance class [mscorlib]System.Collections.Generic.IDictionary`2<string,string> Thrift.Transport.THttpClient::get_CustomHeaders()
  } // end of property THttpClient::CustomHeaders
  .property instance class [System]System.Net.IWebProxy
          Proxy()
  {
    .set instance void Thrift.Transport.THttpClient::set_Proxy(class [System]System.Net.IWebProxy)
  } // end of property THttpClient::Proxy
  .property instance bool IsOpen()
  {
    .get instance bool Thrift.Transport.THttpClient::get_IsOpen()
  } // end of property THttpClient::IsOpen
} // end of class Thrift.Transport.THttpClient

.class public auto ansi beforefieldinit Thrift.Transport.THttpHandler
       extends [mscorlib]System.Object
       implements [System.Web]System.Web.IHttpHandler
{
  .field family class Thrift.TProcessor processor
  .field family class Thrift.Protocol.TProtocolFactory inputProtocolFactory
  .field family class Thrift.Protocol.TProtocolFactory outputProtocolFactory
  .field static family literal string contentType = "application/x-thrift"
  .field family class [mscorlib]System.Text.Encoding encoding
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  newobj     instance void Thrift.Protocol.TBinaryProtocol/Factory::.ctor()
    IL_0007:  call       instance void Thrift.Transport.THttpHandler::.ctor(class Thrift.TProcessor,
                                                                            class Thrift.Protocol.TProtocolFactory)
    IL_000c:  ret
  } // end of method THttpHandler::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Protocol.TProtocolFactory protocolFactory) cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldarg.2
    IL_0004:  call       instance void Thrift.Transport.THttpHandler::.ctor(class Thrift.TProcessor,
                                                                            class Thrift.Protocol.TProtocolFactory,
                                                                            class Thrift.Protocol.TProtocolFactory)
    IL_0009:  ret
  } // end of method THttpHandler::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Thrift.TProcessor processor,
                               class Thrift.Protocol.TProtocolFactory inputProtocolFactory,
                               class Thrift.Protocol.TProtocolFactory outputProtocolFactory) cil managed
  {
    // Code size       39 (0x27)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       class [mscorlib]System.Text.Encoding [mscorlib]System.Text.Encoding::get_UTF8()
    IL_0006:  stfld      class [mscorlib]System.Text.Encoding Thrift.Transport.THttpHandler::encoding
    IL_000b:  ldarg.0
    IL_000c:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0011:  ldarg.0
    IL_0012:  ldarg.1
    IL_0013:  stfld      class Thrift.TProcessor Thrift.Transport.THttpHandler::processor
    IL_0018:  ldarg.0
    IL_0019:  ldarg.2
    IL_001a:  stfld      class Thrift.Protocol.TProtocolFactory Thrift.Transport.THttpHandler::inputProtocolFactory
    IL_001f:  ldarg.0
    IL_0020:  ldarg.3
    IL_0021:  stfld      class Thrift.Protocol.TProtocolFactory Thrift.Transport.THttpHandler::outputProtocolFactory
    IL_0026:  ret
  } // end of method THttpHandler::.ctor

  .method public hidebysig newslot specialname virtual final 
          instance bool  get_IsReusable() cil managed
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldc.i4.1
    IL_0001:  ret
  } // end of method THttpHandler::get_IsReusable

  .method public hidebysig instance void 
          ProcessRequest(class [System]System.Net.HttpListenerContext context) cil managed
  {
    // Code size       62 (0x3e)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  callvirt   instance class [System]System.Net.HttpListenerResponse [System]System.Net.HttpListenerContext::get_Response()
    IL_0006:  ldstr      "application/x-thrift"
    IL_000b:  callvirt   instance void [System]System.Net.HttpListenerResponse::set_ContentType(string)
    IL_0010:  ldarg.1
    IL_0011:  callvirt   instance class [System]System.Net.HttpListenerResponse [System]System.Net.HttpListenerContext::get_Response()
    IL_0016:  ldarg.0
    IL_0017:  ldfld      class [mscorlib]System.Text.Encoding Thrift.Transport.THttpHandler::encoding
    IL_001c:  callvirt   instance void [System]System.Net.HttpListenerResponse::set_ContentEncoding(class [mscorlib]System.Text.Encoding)
    IL_0021:  ldarg.0
    IL_0022:  ldarg.1
    IL_0023:  callvirt   instance class [System]System.Net.HttpListenerRequest [System]System.Net.HttpListenerContext::get_Request()
    IL_0028:  callvirt   instance class [mscorlib]System.IO.Stream [System]System.Net.HttpListenerRequest::get_InputStream()
    IL_002d:  ldarg.1
    IL_002e:  callvirt   instance class [System]System.Net.HttpListenerResponse [System]System.Net.HttpListenerContext::get_Response()
    IL_0033:  callvirt   instance class [mscorlib]System.IO.Stream [System]System.Net.HttpListenerResponse::get_OutputStream()
    IL_0038:  call       instance void Thrift.Transport.THttpHandler::ProcessRequest(class [mscorlib]System.IO.Stream,
                                                                                     class [mscorlib]System.IO.Stream)
    IL_003d:  ret
  } // end of method THttpHandler::ProcessRequest

  .method public hidebysig newslot virtual final 
          instance void  ProcessRequest(class [System.Web]System.Web.HttpContext context) cil managed
  {
    // Code size       62 (0x3e)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  callvirt   instance class [System.Web]System.Web.HttpResponse [System.Web]System.Web.HttpContext::get_Response()
    IL_0006:  ldstr      "application/x-thrift"
    IL_000b:  callvirt   instance void [System.Web]System.Web.HttpResponse::set_ContentType(string)
    IL_0010:  ldarg.1
    IL_0011:  callvirt   instance class [System.Web]System.Web.HttpResponse [System.Web]System.Web.HttpContext::get_Response()
    IL_0016:  ldarg.0
    IL_0017:  ldfld      class [mscorlib]System.Text.Encoding Thrift.Transport.THttpHandler::encoding
    IL_001c:  callvirt   instance void [System.Web]System.Web.HttpResponse::set_ContentEncoding(class [mscorlib]System.Text.Encoding)
    IL_0021:  ldarg.0
    IL_0022:  ldarg.1
    IL_0023:  callvirt   instance class [System.Web]System.Web.HttpRequest [System.Web]System.Web.HttpContext::get_Request()
    IL_0028:  callvirt   instance class [mscorlib]System.IO.Stream [System.Web]System.Web.HttpRequest::get_InputStream()
    IL_002d:  ldarg.1
    IL_002e:  callvirt   instance class [System.Web]System.Web.HttpResponse [System.Web]System.Web.HttpContext::get_Response()
    IL_0033:  callvirt   instance class [mscorlib]System.IO.Stream [System.Web]System.Web.HttpResponse::get_OutputStream()
    IL_0038:  call       instance void Thrift.Transport.THttpHandler::ProcessRequest(class [mscorlib]System.IO.Stream,
                                                                                     class [mscorlib]System.IO.Stream)
    IL_003d:  ret
  } // end of method THttpHandler::ProcessRequest

  .method public hidebysig instance void 
          ProcessRequest(class [mscorlib]System.IO.Stream input,
                         class [mscorlib]System.IO.Stream output) cil managed
  {
    // Code size       76 (0x4c)
    .maxstack  3
    .locals init (class Thrift.Transport.TTransport V_0,
             class Thrift.Protocol.TProtocol V_1,
             class Thrift.Protocol.TProtocol V_2)
    IL_0000:  ldarg.1
    IL_0001:  ldarg.2
    IL_0002:  newobj     instance void Thrift.Transport.TStreamTransport::.ctor(class [mscorlib]System.IO.Stream,
                                                                                class [mscorlib]System.IO.Stream)
    IL_0007:  stloc.0
    .try
    {
      .try
      {
        IL_0008:  ldarg.0
        IL_0009:  ldfld      class Thrift.Protocol.TProtocolFactory Thrift.Transport.THttpHandler::inputProtocolFactory
        IL_000e:  ldloc.0
        IL_000f:  callvirt   instance class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolFactory::GetProtocol(class Thrift.Transport.TTransport)
        IL_0014:  stloc.1
        IL_0015:  ldarg.0
        IL_0016:  ldfld      class Thrift.Protocol.TProtocolFactory Thrift.Transport.THttpHandler::outputProtocolFactory
        IL_001b:  ldloc.0
        IL_001c:  callvirt   instance class Thrift.Protocol.TProtocol Thrift.Protocol.TProtocolFactory::GetProtocol(class Thrift.Transport.TTransport)
        IL_0021:  stloc.2
        IL_0022:  br         IL_0027

        IL_0027:  ldarg.0
        IL_0028:  ldfld      class Thrift.TProcessor Thrift.Transport.THttpHandler::processor
        IL_002d:  ldloc.1
        IL_002e:  ldloc.2
        IL_002f:  callvirt   instance bool Thrift.TProcessor::Process(class Thrift.Protocol.TProtocol,
                                                                      class Thrift.Protocol.TProtocol)
        IL_0034:  brtrue     IL_0027

        IL_0039:  leave      IL_004b

      }  // end .try
      catch Thrift.Transport.TTransportException 
      {
        IL_003e:  pop
        IL_003f:  leave      IL_004b

      }  // end handler
    }  // end .try
    finally
    {
      IL_0044:  ldloc.0
      IL_0045:  callvirt   instance void Thrift.Transport.TTransport::Close()
      IL_004a:  endfinally
    }  // end handler
    IL_004b:  ret
  } // end of method THttpHandler::ProcessRequest

  .property instance bool IsReusable()
  {
    .get instance bool Thrift.Transport.THttpHandler::get_IsReusable()
  } // end of property THttpHandler::IsReusable
} // end of class Thrift.Transport.THttpHandler

.class public auto ansi beforefieldinit Thrift.Transport.TNamedPipeClientTransport
       extends Thrift.Transport.TTransport
{
  .field private class [System.Core]System.IO.Pipes.NamedPipeClientStream client
  .field private string ServerName
  .field private string PipeName
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string pipe) cil managed
  {
    // Code size       25 (0x19)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldstr      "."
    IL_000c:  stfld      string Thrift.Transport.TNamedPipeClientTransport::ServerName
    IL_0011:  ldarg.0
    IL_0012:  ldarg.1
    IL_0013:  stfld      string Thrift.Transport.TNamedPipeClientTransport::PipeName
    IL_0018:  ret
  } // end of method TNamedPipeClientTransport::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string server,
                               string pipe) cil managed
  {
    // Code size       47 (0x2f)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  ldsfld     string [mscorlib]System.String::Empty
    IL_000d:  call       bool [mscorlib]System.String::op_Inequality(string,
                                                                     string)
    IL_0012:  brfalse    IL_001d

    IL_0017:  ldarg.1
    IL_0018:  br         IL_0022

    IL_001d:  ldstr      "."
    IL_0022:  stfld      string Thrift.Transport.TNamedPipeClientTransport::ServerName
    IL_0027:  ldarg.0
    IL_0028:  ldarg.2
    IL_0029:  stfld      string Thrift.Transport.TNamedPipeClientTransport::PipeName
    IL_002e:  ret
  } // end of method TNamedPipeClientTransport::.ctor

  .method public hidebysig specialname virtual 
          instance bool  get_IsOpen() cil managed
  {
    // Code size       26 (0x1a)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeClientStream Thrift.Transport.TNamedPipeClientTransport::client
    IL_0006:  brfalse    IL_0018

    IL_000b:  ldarg.0
    IL_000c:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeClientStream Thrift.Transport.TNamedPipeClientTransport::client
    IL_0011:  callvirt   instance bool [System.Core]System.IO.Pipes.PipeStream::get_IsConnected()
    IL_0016:  br.s       IL_0019

    IL_0018:  ldc.i4.0
    IL_0019:  ret
  } // end of method TNamedPipeClientTransport::get_IsOpen

  .method public hidebysig virtual instance void 
          Open() cil managed
  {
    // Code size       55 (0x37)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance bool Thrift.Transport.TTransport::get_IsOpen()
    IL_0006:  brfalse    IL_0012

    IL_000b:  ldc.i4.2
    IL_000c:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType)
    IL_0011:  throw

    IL_0012:  ldarg.0
    IL_0013:  ldarg.0
    IL_0014:  ldfld      string Thrift.Transport.TNamedPipeClientTransport::ServerName
    IL_0019:  ldarg.0
    IL_001a:  ldfld      string Thrift.Transport.TNamedPipeClientTransport::PipeName
    IL_001f:  ldc.i4.3
    IL_0020:  ldc.i4.0
    IL_0021:  newobj     instance void [System.Core]System.IO.Pipes.NamedPipeClientStream::.ctor(string,
                                                                                                 string,
                                                                                                 valuetype [System.Core]System.IO.Pipes.PipeDirection,
                                                                                                 valuetype [System.Core]System.IO.Pipes.PipeOptions)
    IL_0026:  stfld      class [System.Core]System.IO.Pipes.NamedPipeClientStream Thrift.Transport.TNamedPipeClientTransport::client
    IL_002b:  ldarg.0
    IL_002c:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeClientStream Thrift.Transport.TNamedPipeClientTransport::client
    IL_0031:  callvirt   instance void [System.Core]System.IO.Pipes.NamedPipeClientStream::Connect()
    IL_0036:  ret
  } // end of method TNamedPipeClientTransport::Open

  .method public hidebysig virtual instance void 
          Close() cil managed
  {
    // Code size       30 (0x1e)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeClientStream Thrift.Transport.TNamedPipeClientTransport::client
    IL_0006:  brfalse    IL_001d

    IL_000b:  ldarg.0
    IL_000c:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeClientStream Thrift.Transport.TNamedPipeClientTransport::client
    IL_0011:  callvirt   instance void [mscorlib]System.IO.Stream::Close()
    IL_0016:  ldarg.0
    IL_0017:  ldnull
    IL_0018:  stfld      class [System.Core]System.IO.Pipes.NamedPipeClientStream Thrift.Transport.TNamedPipeClientTransport::client
    IL_001d:  ret
  } // end of method TNamedPipeClientTransport::Close

  .method public hidebysig virtual instance int32 
          Read(uint8[] buf,
               int32 'off',
               int32 len) cil managed
  {
    // Code size       33 (0x21)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeClientStream Thrift.Transport.TNamedPipeClientTransport::client
    IL_0006:  brtrue     IL_0012

    IL_000b:  ldc.i4.1
    IL_000c:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType)
    IL_0011:  throw

    IL_0012:  ldarg.0
    IL_0013:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeClientStream Thrift.Transport.TNamedPipeClientTransport::client
    IL_0018:  ldarg.1
    IL_0019:  ldarg.2
    IL_001a:  ldarg.3
    IL_001b:  callvirt   instance int32 [mscorlib]System.IO.Stream::Read(uint8[],
                                                                         int32,
                                                                         int32)
    IL_0020:  ret
  } // end of method TNamedPipeClientTransport::Read

  .method public hidebysig virtual instance void 
          Write(uint8[] buf,
                int32 'off',
                int32 len) cil managed
  {
    // Code size       33 (0x21)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeClientStream Thrift.Transport.TNamedPipeClientTransport::client
    IL_0006:  brtrue     IL_0012

    IL_000b:  ldc.i4.1
    IL_000c:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType)
    IL_0011:  throw

    IL_0012:  ldarg.0
    IL_0013:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeClientStream Thrift.Transport.TNamedPipeClientTransport::client
    IL_0018:  ldarg.1
    IL_0019:  ldarg.2
    IL_001a:  ldarg.3
    IL_001b:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                         int32,
                                                                         int32)
    IL_0020:  ret
  } // end of method TNamedPipeClientTransport::Write

  .method family hidebysig virtual instance void 
          Dispose(bool disposing) cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeClientStream Thrift.Transport.TNamedPipeClientTransport::client
    IL_0006:  callvirt   instance void [mscorlib]System.IO.Stream::Dispose()
    IL_000b:  ret
  } // end of method TNamedPipeClientTransport::Dispose

  .property instance bool IsOpen()
  {
    .get instance bool Thrift.Transport.TNamedPipeClientTransport::get_IsOpen()
  } // end of property TNamedPipeClientTransport::IsOpen
} // end of class Thrift.Transport.TNamedPipeClientTransport

.class public auto ansi beforefieldinit Thrift.Transport.TNamedPipeServerTransport
       extends Thrift.Transport.TServerTransport
{
  .class auto ansi nested private beforefieldinit ServerTransport
         extends Thrift.Transport.TTransport
  {
    .class auto ansi sealed nested private beforefieldinit '<Read>c__AnonStorey0'
           extends [mscorlib]System.Object
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
      .field assembly int32 retval
      .field assembly class [mscorlib]System.Exception eOuter
      .field assembly class [mscorlib]System.Threading.ManualResetEvent evt
      .field assembly class Thrift.Transport.TNamedPipeServerTransport/ServerTransport $this
      .method public hidebysig specialname rtspecialname 
              instance void  .ctor() cil managed
      {
        // Code size       7 (0x7)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
        IL_0006:  ret
      } // end of method '<Read>c__AnonStorey0'::.ctor

      .method assembly hidebysig instance void 
              '<>m__0'(class [mscorlib]System.IAsyncResult asyncResult) cil managed
      {
        // Code size       126 (0x7e)
        .maxstack  3
        .locals init (class [mscorlib]System.Exception V_0)
        .try
        {
          IL_0000:  ldarg.0
          IL_0001:  ldfld      class Thrift.Transport.TNamedPipeServerTransport/ServerTransport Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::$this
          IL_0006:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
          IL_000b:  brfalse    IL_002c

          IL_0010:  ldarg.0
          IL_0011:  ldarg.0
          IL_0012:  ldfld      class Thrift.Transport.TNamedPipeServerTransport/ServerTransport Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::$this
          IL_0017:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
          IL_001c:  ldarg.1
          IL_001d:  callvirt   instance int32 [mscorlib]System.IO.Stream::EndRead(class [mscorlib]System.IAsyncResult)
          IL_0022:  stfld      int32 Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::retval
          IL_0027:  br         IL_0038

          IL_002c:  ldarg.0
          IL_002d:  ldc.i4.5
          IL_002e:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType)
          IL_0033:  stfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::eOuter
          IL_0038:  leave      IL_0071

        }  // end .try
        catch [mscorlib]System.Exception 
        {
          IL_003d:  stloc.0
          IL_003e:  ldarg.0
          IL_003f:  ldfld      class Thrift.Transport.TNamedPipeServerTransport/ServerTransport Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::$this
          IL_0044:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
          IL_0049:  brfalse    IL_005a

          IL_004e:  ldarg.0
          IL_004f:  ldloc.0
          IL_0050:  stfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::eOuter
          IL_0055:  br         IL_006c

          IL_005a:  ldarg.0
          IL_005b:  ldc.i4.5
          IL_005c:  ldloc.0
          IL_005d:  callvirt   instance string [mscorlib]System.Exception::get_Message()
          IL_0062:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                         string)
          IL_0067:  stfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::eOuter
          IL_006c:  leave      IL_0071

        }  // end handler
        IL_0071:  ldarg.0
        IL_0072:  ldfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::evt
        IL_0077:  callvirt   instance bool [mscorlib]System.Threading.EventWaitHandle::Set()
        IL_007c:  pop
        IL_007d:  ret
      } // end of method '<Read>c__AnonStorey0'::'<>m__0'

    } // end of class '<Read>c__AnonStorey0'

    .class auto ansi sealed nested private beforefieldinit '<Write>c__AnonStorey1'
           extends [mscorlib]System.Object
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
      .field assembly class [mscorlib]System.Exception eOuter
      .field assembly class [mscorlib]System.Threading.ManualResetEvent evt
      .field assembly class Thrift.Transport.TNamedPipeServerTransport/ServerTransport $this
      .method public hidebysig specialname rtspecialname 
              instance void  .ctor() cil managed
      {
        // Code size       7 (0x7)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
        IL_0006:  ret
      } // end of method '<Write>c__AnonStorey1'::.ctor

      .method assembly hidebysig instance void 
              '<>m__0'(class [mscorlib]System.IAsyncResult asyncResult) cil managed
      {
        // Code size       120 (0x78)
        .maxstack  3
        .locals init (class [mscorlib]System.Exception V_0)
        .try
        {
          IL_0000:  ldarg.0
          IL_0001:  ldfld      class Thrift.Transport.TNamedPipeServerTransport/ServerTransport Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::$this
          IL_0006:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
          IL_000b:  brfalse    IL_0026

          IL_0010:  ldarg.0
          IL_0011:  ldfld      class Thrift.Transport.TNamedPipeServerTransport/ServerTransport Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::$this
          IL_0016:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
          IL_001b:  ldarg.1
          IL_001c:  callvirt   instance void [mscorlib]System.IO.Stream::EndWrite(class [mscorlib]System.IAsyncResult)
          IL_0021:  br         IL_0032

          IL_0026:  ldarg.0
          IL_0027:  ldc.i4.5
          IL_0028:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType)
          IL_002d:  stfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::eOuter
          IL_0032:  leave      IL_006b

        }  // end .try
        catch [mscorlib]System.Exception 
        {
          IL_0037:  stloc.0
          IL_0038:  ldarg.0
          IL_0039:  ldfld      class Thrift.Transport.TNamedPipeServerTransport/ServerTransport Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::$this
          IL_003e:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
          IL_0043:  brfalse    IL_0054

          IL_0048:  ldarg.0
          IL_0049:  ldloc.0
          IL_004a:  stfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::eOuter
          IL_004f:  br         IL_0066

          IL_0054:  ldarg.0
          IL_0055:  ldc.i4.5
          IL_0056:  ldloc.0
          IL_0057:  callvirt   instance string [mscorlib]System.Exception::get_Message()
          IL_005c:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                         string)
          IL_0061:  stfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::eOuter
          IL_0066:  leave      IL_006b

        }  // end handler
        IL_006b:  ldarg.0
        IL_006c:  ldfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::evt
        IL_0071:  callvirt   instance bool [mscorlib]System.Threading.EventWaitHandle::Set()
        IL_0076:  pop
        IL_0077:  ret
      } // end of method '<Write>c__AnonStorey1'::'<>m__0'

    } // end of class '<Write>c__AnonStorey1'

    .field private class [System.Core]System.IO.Pipes.NamedPipeServerStream 'stream'
    .field private bool asyncMode
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor(class [System.Core]System.IO.Pipes.NamedPipeServerStream 'stream',
                                 bool asyncMode) cil managed
    {
      // Code size       21 (0x15)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void Thrift.Transport.TTransport::.ctor()
      IL_0006:  ldarg.0
      IL_0007:  ldarg.1
      IL_0008:  stfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
      IL_000d:  ldarg.0
      IL_000e:  ldarg.2
      IL_000f:  stfld      bool Thrift.Transport.TNamedPipeServerTransport/ServerTransport::asyncMode
      IL_0014:  ret
    } // end of method ServerTransport::.ctor

    .method public hidebysig specialname virtual 
            instance bool  get_IsOpen() cil managed
    {
      // Code size       26 (0x1a)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
      IL_0006:  brfalse    IL_0018

      IL_000b:  ldarg.0
      IL_000c:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
      IL_0011:  callvirt   instance bool [System.Core]System.IO.Pipes.PipeStream::get_IsConnected()
      IL_0016:  br.s       IL_0019

      IL_0018:  ldc.i4.0
      IL_0019:  ret
    } // end of method ServerTransport::get_IsOpen

    .method public hidebysig virtual instance void 
            Open() cil managed
    {
      // Code size       1 (0x1)
      .maxstack  8
      IL_0000:  ret
    } // end of method ServerTransport::Open

    .method public hidebysig virtual instance void 
            Close() cil managed
    {
      // Code size       23 (0x17)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
      IL_0006:  brfalse    IL_0016

      IL_000b:  ldarg.0
      IL_000c:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
      IL_0011:  callvirt   instance void [mscorlib]System.IO.Stream::Close()
      IL_0016:  ret
    } // end of method ServerTransport::Close

    .method public hidebysig virtual instance int32 
            Read(uint8[] buf,
                 int32 'off',
                 int32 len) cil managed
    {
      // Code size       148 (0x94)
      .maxstack  6
      .locals init (class Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0' V_0)
      IL_0000:  ldarg.0
      IL_0001:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
      IL_0006:  brtrue     IL_0012

      IL_000b:  ldc.i4.1
      IL_000c:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType)
      IL_0011:  throw

      IL_0012:  ldarg.0
      IL_0013:  ldfld      bool Thrift.Transport.TNamedPipeServerTransport/ServerTransport::asyncMode
      IL_0018:  brfalse    IL_0085

      IL_001d:  newobj     instance void Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::.ctor()
      IL_0022:  stloc.0
      IL_0023:  ldloc.0
      IL_0024:  ldarg.0
      IL_0025:  stfld      class Thrift.Transport.TNamedPipeServerTransport/ServerTransport Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::$this
      IL_002a:  ldloc.0
      IL_002b:  ldnull
      IL_002c:  stfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::eOuter
      IL_0031:  ldloc.0
      IL_0032:  ldc.i4.0
      IL_0033:  newobj     instance void [mscorlib]System.Threading.ManualResetEvent::.ctor(bool)
      IL_0038:  stfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::evt
      IL_003d:  ldloc.0
      IL_003e:  ldc.i4.0
      IL_003f:  stfld      int32 Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::retval
      IL_0044:  ldarg.0
      IL_0045:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
      IL_004a:  ldarg.1
      IL_004b:  ldarg.2
      IL_004c:  ldarg.3
      IL_004d:  ldloc.0
      IL_004e:  ldftn      instance void Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::'<>m__0'(class [mscorlib]System.IAsyncResult)
      IL_0054:  newobj     instance void [mscorlib]System.AsyncCallback::.ctor(object,
                                                                               native int)
      IL_0059:  ldnull
      IL_005a:  callvirt   instance class [mscorlib]System.IAsyncResult [mscorlib]System.IO.Stream::BeginRead(uint8[],
                                                                                                              int32,
                                                                                                              int32,
                                                                                                              class [mscorlib]System.AsyncCallback,
                                                                                                              object)
      IL_005f:  pop
      IL_0060:  ldloc.0
      IL_0061:  ldfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::evt
      IL_0066:  callvirt   instance bool [mscorlib]System.Threading.WaitHandle::WaitOne()
      IL_006b:  pop
      IL_006c:  ldloc.0
      IL_006d:  ldfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::eOuter
      IL_0072:  brfalse    IL_007e

      IL_0077:  ldloc.0
      IL_0078:  ldfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::eOuter
      IL_007d:  throw

      IL_007e:  ldloc.0
      IL_007f:  ldfld      int32 Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Read>c__AnonStorey0'::retval
      IL_0084:  ret

      IL_0085:  ldarg.0
      IL_0086:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
      IL_008b:  ldarg.1
      IL_008c:  ldarg.2
      IL_008d:  ldarg.3
      IL_008e:  callvirt   instance int32 [mscorlib]System.IO.Stream::Read(uint8[],
                                                                           int32,
                                                                           int32)
      IL_0093:  ret
    } // end of method ServerTransport::Read

    .method public hidebysig virtual instance void 
            Write(uint8[] buf,
                  int32 'off',
                  int32 len) cil managed
    {
      // Code size       139 (0x8b)
      .maxstack  6
      .locals init (class Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1' V_0)
      IL_0000:  ldarg.0
      IL_0001:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
      IL_0006:  brtrue     IL_0012

      IL_000b:  ldc.i4.1
      IL_000c:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType)
      IL_0011:  throw

      IL_0012:  ldarg.0
      IL_0013:  ldfld      bool Thrift.Transport.TNamedPipeServerTransport/ServerTransport::asyncMode
      IL_0018:  brfalse    IL_007c

      IL_001d:  newobj     instance void Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::.ctor()
      IL_0022:  stloc.0
      IL_0023:  ldloc.0
      IL_0024:  ldarg.0
      IL_0025:  stfld      class Thrift.Transport.TNamedPipeServerTransport/ServerTransport Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::$this
      IL_002a:  ldloc.0
      IL_002b:  ldnull
      IL_002c:  stfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::eOuter
      IL_0031:  ldloc.0
      IL_0032:  ldc.i4.0
      IL_0033:  newobj     instance void [mscorlib]System.Threading.ManualResetEvent::.ctor(bool)
      IL_0038:  stfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::evt
      IL_003d:  ldarg.0
      IL_003e:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
      IL_0043:  ldarg.1
      IL_0044:  ldarg.2
      IL_0045:  ldarg.3
      IL_0046:  ldloc.0
      IL_0047:  ldftn      instance void Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::'<>m__0'(class [mscorlib]System.IAsyncResult)
      IL_004d:  newobj     instance void [mscorlib]System.AsyncCallback::.ctor(object,
                                                                               native int)
      IL_0052:  ldnull
      IL_0053:  callvirt   instance class [mscorlib]System.IAsyncResult [mscorlib]System.IO.Stream::BeginWrite(uint8[],
                                                                                                               int32,
                                                                                                               int32,
                                                                                                               class [mscorlib]System.AsyncCallback,
                                                                                                               object)
      IL_0058:  pop
      IL_0059:  ldloc.0
      IL_005a:  ldfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::evt
      IL_005f:  callvirt   instance bool [mscorlib]System.Threading.WaitHandle::WaitOne()
      IL_0064:  pop
      IL_0065:  ldloc.0
      IL_0066:  ldfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::eOuter
      IL_006b:  brfalse    IL_0077

      IL_0070:  ldloc.0
      IL_0071:  ldfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/ServerTransport/'<Write>c__AnonStorey1'::eOuter
      IL_0076:  throw

      IL_0077:  br         IL_008a

      IL_007c:  ldarg.0
      IL_007d:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
      IL_0082:  ldarg.1
      IL_0083:  ldarg.2
      IL_0084:  ldarg.3
      IL_0085:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                           int32,
                                                                           int32)
      IL_008a:  ret
    } // end of method ServerTransport::Write

    .method family hidebysig virtual instance void 
            Dispose(bool disposing) cil managed
    {
      // Code size       23 (0x17)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
      IL_0006:  brfalse    IL_0016

      IL_000b:  ldarg.0
      IL_000c:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport/ServerTransport::'stream'
      IL_0011:  callvirt   instance void [mscorlib]System.IO.Stream::Dispose()
      IL_0016:  ret
    } // end of method ServerTransport::Dispose

    .property instance bool IsOpen()
    {
      .get instance bool Thrift.Transport.TNamedPipeServerTransport/ServerTransport::get_IsOpen()
    } // end of property ServerTransport::IsOpen
  } // end of class ServerTransport

  .class auto ansi sealed nested private beforefieldinit '<AcceptImpl>c__AnonStorey0'
         extends [mscorlib]System.Object
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field assembly class [mscorlib]System.Exception eOuter
    .field assembly class [mscorlib]System.Threading.ManualResetEvent evt
    .field assembly class Thrift.Transport.TNamedPipeServerTransport $this
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<AcceptImpl>c__AnonStorey0'::.ctor

    .method assembly hidebysig instance void 
            '<>m__0'(class [mscorlib]System.IAsyncResult asyncResult) cil managed
    {
      // Code size       120 (0x78)
      .maxstack  3
      .locals init (class [mscorlib]System.Exception V_0)
      .try
      {
        IL_0000:  ldarg.0
        IL_0001:  ldfld      class Thrift.Transport.TNamedPipeServerTransport Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::$this
        IL_0006:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
        IL_000b:  brfalse    IL_0026

        IL_0010:  ldarg.0
        IL_0011:  ldfld      class Thrift.Transport.TNamedPipeServerTransport Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::$this
        IL_0016:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
        IL_001b:  ldarg.1
        IL_001c:  callvirt   instance void [System.Core]System.IO.Pipes.NamedPipeServerStream::EndWaitForConnection(class [mscorlib]System.IAsyncResult)
        IL_0021:  br         IL_0032

        IL_0026:  ldarg.0
        IL_0027:  ldc.i4.5
        IL_0028:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType)
        IL_002d:  stfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::eOuter
        IL_0032:  leave      IL_006b

      }  // end .try
      catch [mscorlib]System.Exception 
      {
        IL_0037:  stloc.0
        IL_0038:  ldarg.0
        IL_0039:  ldfld      class Thrift.Transport.TNamedPipeServerTransport Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::$this
        IL_003e:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
        IL_0043:  brfalse    IL_0054

        IL_0048:  ldarg.0
        IL_0049:  ldloc.0
        IL_004a:  stfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::eOuter
        IL_004f:  br         IL_0066

        IL_0054:  ldarg.0
        IL_0055:  ldc.i4.5
        IL_0056:  ldloc.0
        IL_0057:  callvirt   instance string [mscorlib]System.Exception::get_Message()
        IL_005c:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                       string)
        IL_0061:  stfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::eOuter
        IL_0066:  leave      IL_006b

      }  // end handler
      IL_006b:  ldarg.0
      IL_006c:  ldfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::evt
      IL_0071:  callvirt   instance bool [mscorlib]System.Threading.EventWaitHandle::Set()
      IL_0076:  pop
      IL_0077:  ret
    } // end of method '<AcceptImpl>c__AnonStorey0'::'<>m__0'

  } // end of class '<AcceptImpl>c__AnonStorey0'

  .field private initonly string pipeAddress
  .field private class [System.Core]System.IO.Pipes.NamedPipeServerStream 'stream'
  .field private bool asyncMode
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string pipeAddress) cil managed
  {
    // Code size       21 (0x15)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.1
    IL_0002:  stfld      bool Thrift.Transport.TNamedPipeServerTransport::asyncMode
    IL_0007:  ldarg.0
    IL_0008:  call       instance void Thrift.Transport.TServerTransport::.ctor()
    IL_000d:  ldarg.0
    IL_000e:  ldarg.1
    IL_000f:  stfld      string Thrift.Transport.TNamedPipeServerTransport::pipeAddress
    IL_0014:  ret
  } // end of method TNamedPipeServerTransport::.ctor

  .method public hidebysig virtual instance void 
          Listen() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TNamedPipeServerTransport::Listen

  .method public hidebysig virtual instance void 
          Close() cil managed
  {
    // Code size       47 (0x2f)
    .maxstack  2
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
    IL_0006:  brfalse    IL_002e

    .try
    {
      IL_000b:  ldarg.0
      IL_000c:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
      IL_0011:  callvirt   instance void [mscorlib]System.IO.Stream::Close()
      IL_0016:  ldarg.0
      IL_0017:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
      IL_001c:  callvirt   instance void [mscorlib]System.IO.Stream::Dispose()
      IL_0021:  leave      IL_002e

    }  // end .try
    finally
    {
      IL_0026:  ldarg.0
      IL_0027:  ldnull
      IL_0028:  stfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
      IL_002d:  endfinally
    }  // end handler
    IL_002e:  ret
  } // end of method TNamedPipeServerTransport::Close

  .method private hidebysig instance void 
          EnsurePipeInstance() cil managed
  {
    // Code size       153 (0x99)
    .maxstack  8
    .locals init (valuetype [System.Core]System.IO.Pipes.PipeDirection V_0,
             int32 V_1,
             valuetype [System.Core]System.IO.Pipes.PipeTransmissionMode V_2,
             valuetype [System.Core]System.IO.Pipes.PipeOptions V_3,
             int32 V_4,
             int32 V_5)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
    IL_0006:  brtrue     IL_0098

    IL_000b:  ldc.i4.3
    IL_000c:  stloc.0
    IL_000d:  ldc.i4     0xfe
    IL_0012:  stloc.1
    IL_0013:  ldc.i4.0
    IL_0014:  stloc.2
    IL_0015:  ldarg.0
    IL_0016:  ldfld      bool Thrift.Transport.TNamedPipeServerTransport::asyncMode
    IL_001b:  brfalse    IL_002a

    IL_0020:  ldc.i4     0x40000000
    IL_0025:  br         IL_002b

    IL_002a:  ldc.i4.0
    IL_002b:  stloc.3
    IL_002c:  ldc.i4     0x1000
    IL_0031:  stloc.s    V_4
    IL_0033:  ldc.i4     0x1000
    IL_0038:  stloc.s    V_5
    .try
    {
      IL_003a:  ldarg.0
      IL_003b:  ldarg.0
      IL_003c:  ldfld      string Thrift.Transport.TNamedPipeServerTransport::pipeAddress
      IL_0041:  ldloc.0
      IL_0042:  ldloc.1
      IL_0043:  ldloc.2
      IL_0044:  ldloc.3
      IL_0045:  ldloc.s    V_4
      IL_0047:  ldloc.s    V_5
      IL_0049:  newobj     instance void [System.Core]System.IO.Pipes.NamedPipeServerStream::.ctor(string,
                                                                                                   valuetype [System.Core]System.IO.Pipes.PipeDirection,
                                                                                                   int32,
                                                                                                   valuetype [System.Core]System.IO.Pipes.PipeTransmissionMode,
                                                                                                   valuetype [System.Core]System.IO.Pipes.PipeOptions,
                                                                                                   int32,
                                                                                                   int32)
      IL_004e:  stfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
      IL_0053:  leave      IL_0098

    }  // end .try
    catch [mscorlib]System.NotImplementedException 
    {
      IL_0058:  pop
      IL_0059:  ldarg.0
      IL_005a:  ldfld      bool Thrift.Transport.TNamedPipeServerTransport::asyncMode
      IL_005f:  brfalse    IL_0091

      IL_0064:  ldloc.3
      IL_0065:  ldc.i4     0xbfffffff
      IL_006a:  and
      IL_006b:  stloc.3
      IL_006c:  ldarg.0
      IL_006d:  ldarg.0
      IL_006e:  ldfld      string Thrift.Transport.TNamedPipeServerTransport::pipeAddress
      IL_0073:  ldloc.0
      IL_0074:  ldloc.1
      IL_0075:  ldloc.2
      IL_0076:  ldloc.3
      IL_0077:  ldloc.s    V_4
      IL_0079:  ldloc.s    V_5
      IL_007b:  newobj     instance void [System.Core]System.IO.Pipes.NamedPipeServerStream::.ctor(string,
                                                                                                   valuetype [System.Core]System.IO.Pipes.PipeDirection,
                                                                                                   int32,
                                                                                                   valuetype [System.Core]System.IO.Pipes.PipeTransmissionMode,
                                                                                                   valuetype [System.Core]System.IO.Pipes.PipeOptions,
                                                                                                   int32,
                                                                                                   int32)
      IL_0080:  stfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
      IL_0085:  ldarg.0
      IL_0086:  ldc.i4.0
      IL_0087:  stfld      bool Thrift.Transport.TNamedPipeServerTransport::asyncMode
      IL_008c:  br         IL_0093

      IL_0091:  rethrow
      IL_0093:  leave      IL_0098

    }  // end handler
    IL_0098:  ret
  } // end of method TNamedPipeServerTransport::EnsurePipeInstance

  .method family hidebysig virtual instance class Thrift.Transport.TTransport 
          AcceptImpl() cil managed
  {
    // Code size       183 (0xb7)
    .maxstack  3
    .locals init (class Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0' V_0,
             class Thrift.Transport.TNamedPipeServerTransport/ServerTransport V_1,
             class Thrift.Transport.TTransport V_2,
             class [mscorlib]System.Exception V_3)
    .try
    {
      IL_0000:  ldarg.0
      IL_0001:  call       instance void Thrift.Transport.TNamedPipeServerTransport::EnsurePipeInstance()
      IL_0006:  ldarg.0
      IL_0007:  ldfld      bool Thrift.Transport.TNamedPipeServerTransport::asyncMode
      IL_000c:  brfalse    IL_006d

      IL_0011:  newobj     instance void Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::.ctor()
      IL_0016:  stloc.0
      IL_0017:  ldloc.0
      IL_0018:  ldarg.0
      IL_0019:  stfld      class Thrift.Transport.TNamedPipeServerTransport Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::$this
      IL_001e:  ldloc.0
      IL_001f:  ldc.i4.0
      IL_0020:  newobj     instance void [mscorlib]System.Threading.ManualResetEvent::.ctor(bool)
      IL_0025:  stfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::evt
      IL_002a:  ldloc.0
      IL_002b:  ldnull
      IL_002c:  stfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::eOuter
      IL_0031:  ldarg.0
      IL_0032:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
      IL_0037:  ldloc.0
      IL_0038:  ldftn      instance void Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::'<>m__0'(class [mscorlib]System.IAsyncResult)
      IL_003e:  newobj     instance void [mscorlib]System.AsyncCallback::.ctor(object,
                                                                               native int)
      IL_0043:  ldnull
      IL_0044:  callvirt   instance class [mscorlib]System.IAsyncResult [System.Core]System.IO.Pipes.NamedPipeServerStream::BeginWaitForConnection(class [mscorlib]System.AsyncCallback,
                                                                                                                                                   object)
      IL_0049:  pop
      IL_004a:  ldloc.0
      IL_004b:  ldfld      class [mscorlib]System.Threading.ManualResetEvent Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::evt
      IL_0050:  callvirt   instance bool [mscorlib]System.Threading.WaitHandle::WaitOne()
      IL_0055:  pop
      IL_0056:  ldloc.0
      IL_0057:  ldfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::eOuter
      IL_005c:  brfalse    IL_0068

      IL_0061:  ldloc.0
      IL_0062:  ldfld      class [mscorlib]System.Exception Thrift.Transport.TNamedPipeServerTransport/'<AcceptImpl>c__AnonStorey0'::eOuter
      IL_0067:  throw

      IL_0068:  br         IL_0078

      IL_006d:  ldarg.0
      IL_006e:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
      IL_0073:  callvirt   instance void [System.Core]System.IO.Pipes.NamedPipeServerStream::WaitForConnection()
      IL_0078:  ldarg.0
      IL_0079:  ldfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
      IL_007e:  ldarg.0
      IL_007f:  ldfld      bool Thrift.Transport.TNamedPipeServerTransport::asyncMode
      IL_0084:  newobj     instance void Thrift.Transport.TNamedPipeServerTransport/ServerTransport::.ctor(class [System.Core]System.IO.Pipes.NamedPipeServerStream,
                                                                                                           bool)
      IL_0089:  stloc.1
      IL_008a:  ldarg.0
      IL_008b:  ldnull
      IL_008c:  stfld      class [System.Core]System.IO.Pipes.NamedPipeServerStream Thrift.Transport.TNamedPipeServerTransport::'stream'
      IL_0091:  ldloc.1
      IL_0092:  stloc.2
      IL_0093:  leave      IL_00b5

    }  // end .try
    catch Thrift.Transport.TTransportException 
    {
      IL_0098:  pop
      IL_0099:  ldarg.0
      IL_009a:  callvirt   instance void Thrift.Transport.TServerTransport::Close()
      IL_009f:  rethrow
    }  // end handler
    catch [mscorlib]System.Exception 
    {
      IL_00a1:  stloc.3
      IL_00a2:  ldarg.0
      IL_00a3:  callvirt   instance void Thrift.Transport.TServerTransport::Close()
      IL_00a8:  ldc.i4.1
      IL_00a9:  ldloc.3
      IL_00aa:  callvirt   instance string [mscorlib]System.Exception::get_Message()
      IL_00af:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                     string)
      IL_00b4:  throw

    }  // end handler
    IL_00b5:  ldloc.2
    IL_00b6:  ret
  } // end of method TNamedPipeServerTransport::AcceptImpl

} // end of class Thrift.Transport.TNamedPipeServerTransport

.class public auto ansi beforefieldinit Thrift.Transport.TServerSocket
       extends Thrift.Transport.TServerTransport
{
  .field private class [System]System.Net.Sockets.TcpListener server
  .field private int32 port
  .field private int32 clientTimeout
  .field private bool useBufferedSockets
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class [System]System.Net.Sockets.TcpListener listener) cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldc.i4.0
    IL_0003:  call       instance void Thrift.Transport.TServerSocket::.ctor(class [System]System.Net.Sockets.TcpListener,
                                                                             int32)
    IL_0008:  ret
  } // end of method TServerSocket::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class [System]System.Net.Sockets.TcpListener listener,
                               int32 clientTimeout) cil managed
  {
    // Code size       21 (0x15)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TServerTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TServerSocket::server
    IL_000d:  ldarg.0
    IL_000e:  ldarg.2
    IL_000f:  stfld      int32 Thrift.Transport.TServerSocket::clientTimeout
    IL_0014:  ret
  } // end of method TServerSocket::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(int32 port) cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldc.i4.0
    IL_0003:  call       instance void Thrift.Transport.TServerSocket::.ctor(int32,
                                                                             int32)
    IL_0008:  ret
  } // end of method TServerSocket::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(int32 port,
                               int32 clientTimeout) cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldc.i4.0
    IL_0004:  call       instance void Thrift.Transport.TServerSocket::.ctor(int32,
                                                                             int32,
                                                                             bool)
    IL_0009:  ret
  } // end of method TServerSocket::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(int32 port,
                               int32 clientTimeout,
                               bool useBufferedSockets) cil managed
  {
    // Code size       107 (0x6b)
    .maxstack  3
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TServerTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      int32 Thrift.Transport.TServerSocket::port
    IL_000d:  ldarg.0
    IL_000e:  ldarg.2
    IL_000f:  stfld      int32 Thrift.Transport.TServerSocket::clientTimeout
    IL_0014:  ldarg.0
    IL_0015:  ldarg.3
    IL_0016:  stfld      bool Thrift.Transport.TServerSocket::useBufferedSockets
    .try
    {
      IL_001b:  ldarg.0
      IL_001c:  ldsfld     class [System]System.Net.IPAddress [System]System.Net.IPAddress::Any
      IL_0021:  ldarg.0
      IL_0022:  ldfld      int32 Thrift.Transport.TServerSocket::port
      IL_0027:  newobj     instance void [System]System.Net.Sockets.TcpListener::.ctor(class [System]System.Net.IPAddress,
                                                                                       int32)
      IL_002c:  stfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TServerSocket::server
      IL_0031:  ldarg.0
      IL_0032:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TServerSocket::server
      IL_0037:  callvirt   instance class [System]System.Net.Sockets.Socket [System]System.Net.Sockets.TcpListener::get_Server()
      IL_003c:  ldc.i4.1
      IL_003d:  callvirt   instance void [System]System.Net.Sockets.Socket::set_NoDelay(bool)
      IL_0042:  leave      IL_006a

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_0047:  pop
      IL_0048:  ldarg.0
      IL_0049:  ldnull
      IL_004a:  stfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TServerSocket::server
      IL_004f:  ldstr      "Could not create ServerSocket on port "
      IL_0054:  ldarg.1
      IL_0055:  box        [mscorlib]System.Int32
      IL_005a:  ldstr      "."
      IL_005f:  call       string [mscorlib]System.String::Concat(object,
                                                                  object,
                                                                  object)
      IL_0064:  newobj     instance void Thrift.Transport.TTransportException::.ctor(string)
      IL_0069:  throw

    }  // end handler
    IL_006a:  ret
  } // end of method TServerSocket::.ctor

  .method public hidebysig virtual instance void 
          Listen() cil managed
  {
    // Code size       51 (0x33)
    .maxstack  2
    .locals init (class [System]System.Net.Sockets.SocketException V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TServerSocket::server
    IL_0006:  brfalse    IL_0032

    .try
    {
      IL_000b:  ldarg.0
      IL_000c:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TServerSocket::server
      IL_0011:  callvirt   instance void [System]System.Net.Sockets.TcpListener::Start()
      IL_0016:  leave      IL_0032

    }  // end .try
    catch [System]System.Net.Sockets.SocketException 
    {
      IL_001b:  stloc.0
      IL_001c:  ldstr      "Could not accept on listening socket: "
      IL_0021:  ldloc.0
      IL_0022:  callvirt   instance string [mscorlib]System.Exception::get_Message()
      IL_0027:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_002c:  newobj     instance void Thrift.Transport.TTransportException::.ctor(string)
      IL_0031:  throw

    }  // end handler
    IL_0032:  ret
  } // end of method TServerSocket::Listen

  .method family hidebysig virtual instance class Thrift.Transport.TTransport 
          AcceptImpl() cil managed
  {
    // Code size       131 (0x83)
    .maxstack  2
    .locals init (class Thrift.Transport.TSocket V_0,
             class [System]System.Net.Sockets.TcpClient V_1,
             class Thrift.Transport.TBufferedTransport V_2,
             class Thrift.Transport.TTransport V_3,
             class [mscorlib]System.Exception V_4)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TServerSocket::server
    IL_0006:  brtrue     IL_0017

    IL_000b:  ldc.i4.1
    IL_000c:  ldstr      "No underlying server socket."
    IL_0011:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_0016:  throw

    .try
    {
      IL_0017:  ldnull
      IL_0018:  stloc.0
      IL_0019:  ldarg.0
      IL_001a:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TServerSocket::server
      IL_001f:  callvirt   instance class [System]System.Net.Sockets.TcpClient [System]System.Net.Sockets.TcpListener::AcceptTcpClient()
      IL_0024:  stloc.1
      .try
      {
        IL_0025:  ldloc.1
        IL_0026:  newobj     instance void Thrift.Transport.TSocket::.ctor(class [System]System.Net.Sockets.TcpClient)
        IL_002b:  stloc.0
        IL_002c:  ldloc.0
        IL_002d:  ldarg.0
        IL_002e:  ldfld      int32 Thrift.Transport.TServerSocket::clientTimeout
        IL_0033:  callvirt   instance void Thrift.Transport.TSocket::set_Timeout(int32)
        IL_0038:  ldarg.0
        IL_0039:  ldfld      bool Thrift.Transport.TServerSocket::useBufferedSockets
        IL_003e:  brfalse    IL_0051

        IL_0043:  ldloc.0
        IL_0044:  newobj     instance void Thrift.Transport.TBufferedTransport::.ctor(class Thrift.Transport.TStreamTransport)
        IL_0049:  stloc.2
        IL_004a:  ldloc.2
        IL_004b:  stloc.3
        IL_004c:  leave      IL_0081

        IL_0051:  ldloc.0
        IL_0052:  stloc.3
        IL_0053:  leave      IL_0081

      }  // end .try
      catch [mscorlib]System.Exception 
      {
        IL_0058:  pop
        IL_0059:  ldloc.0
        IL_005a:  brfalse    IL_006a

        IL_005f:  ldloc.0
        IL_0060:  callvirt   instance void Thrift.Transport.TTransport::Dispose()
        IL_0065:  br         IL_0070

        IL_006a:  ldloc.1
        IL_006b:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_0070:  rethrow
      }  // end handler
    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_0072:  stloc.s    V_4
      IL_0074:  ldloc.s    V_4
      IL_0076:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_007b:  newobj     instance void Thrift.Transport.TTransportException::.ctor(string)
      IL_0080:  throw

    }  // end handler
    IL_0081:  ldloc.3
    IL_0082:  ret
  } // end of method TServerSocket::AcceptImpl

  .method public hidebysig virtual instance void 
          Close() cil managed
  {
    // Code size       53 (0x35)
    .maxstack  2
    .locals init (class [mscorlib]System.Exception V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TServerSocket::server
    IL_0006:  brfalse    IL_0034

    .try
    {
      IL_000b:  ldarg.0
      IL_000c:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TServerSocket::server
      IL_0011:  callvirt   instance void [System]System.Net.Sockets.TcpListener::Stop()
      IL_0016:  leave      IL_002d

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_001b:  stloc.0
      IL_001c:  ldstr      "WARNING: Could not close server socket: "
      IL_0021:  ldloc.0
      IL_0022:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_0027:  newobj     instance void Thrift.Transport.TTransportException::.ctor(string)
      IL_002c:  throw

    }  // end handler
    IL_002d:  ldarg.0
    IL_002e:  ldnull
    IL_002f:  stfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TServerSocket::server
    IL_0034:  ret
  } // end of method TServerSocket::Close

} // end of class Thrift.Transport.TServerSocket

.class public abstract auto ansi beforefieldinit Thrift.Transport.TServerTransport
       extends [mscorlib]System.Object
{
  .method family hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  } // end of method TServerTransport::.ctor

  .method public hidebysig newslot abstract virtual 
          instance void  Listen() cil managed
  {
  } // end of method TServerTransport::Listen

  .method public hidebysig newslot abstract virtual 
          instance void  Close() cil managed
  {
  } // end of method TServerTransport::Close

  .method family hidebysig newslot abstract virtual 
          instance class Thrift.Transport.TTransport 
          AcceptImpl() cil managed
  {
  } // end of method TServerTransport::AcceptImpl

  .method public hidebysig instance class Thrift.Transport.TTransport 
          Accept() cil managed
  {
    // Code size       26 (0x1a)
    .maxstack  1
    .locals init (class Thrift.Transport.TTransport V_0)
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance class Thrift.Transport.TTransport Thrift.Transport.TServerTransport::AcceptImpl()
    IL_0006:  stloc.0
    IL_0007:  ldloc.0
    IL_0008:  brtrue     IL_0018

    IL_000d:  ldstr      "accept() may not return NULL"
    IL_0012:  newobj     instance void Thrift.Transport.TTransportException::.ctor(string)
    IL_0017:  throw

    IL_0018:  ldloc.0
    IL_0019:  ret
  } // end of method TServerTransport::Accept

} // end of class Thrift.Transport.TServerTransport

.class public auto ansi beforefieldinit Thrift.Transport.TSocket
       extends Thrift.Transport.TStreamTransport
{
  .class auto ansi nested private beforefieldinit ConnectHelper
         extends [mscorlib]System.Object
  {
    .field public object Mutex
    .field public bool DoCleanup
    .field public bool CallbackDone
    .field public class [System]System.Net.Sockets.TcpClient Client
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor(class [System]System.Net.Sockets.TcpClient client) cil managed
    {
      // Code size       25 (0x19)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  newobj     instance void [mscorlib]System.Object::.ctor()
      IL_0006:  stfld      object Thrift.Transport.TSocket/ConnectHelper::Mutex
      IL_000b:  ldarg.0
      IL_000c:  call       instance void [mscorlib]System.Object::.ctor()
      IL_0011:  ldarg.0
      IL_0012:  ldarg.1
      IL_0013:  stfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket/ConnectHelper::Client
      IL_0018:  ret
    } // end of method ConnectHelper::.ctor

  } // end of class ConnectHelper

  .field private class [System]System.Net.Sockets.TcpClient client
  .field private string host
  .field private int32 port
  .field private int32 timeout
  .field private bool _IsDisposed
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class [System]System.Net.Sockets.TcpClient client) cil managed
  {
    // Code size       49 (0x31)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TStreamTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_000d:  ldarg.0
    IL_000e:  callvirt   instance bool Thrift.Transport.TTransport::get_IsOpen()
    IL_0013:  brfalse    IL_0030

    IL_0018:  ldarg.0
    IL_0019:  ldarg.1
    IL_001a:  callvirt   instance class [System]System.Net.Sockets.NetworkStream [System]System.Net.Sockets.TcpClient::GetStream()
    IL_001f:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::inputStream
    IL_0024:  ldarg.0
    IL_0025:  ldarg.1
    IL_0026:  callvirt   instance class [System]System.Net.Sockets.NetworkStream [System]System.Net.Sockets.TcpClient::GetStream()
    IL_002b:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::outputStream
    IL_0030:  ret
  } // end of method TSocket::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string host,
                               int32 port) cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldc.i4.0
    IL_0004:  call       instance void Thrift.Transport.TSocket::.ctor(string,
                                                                       int32,
                                                                       int32)
    IL_0009:  ret
  } // end of method TSocket::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string host,
                               int32 port,
                               int32 timeout) cil managed
  {
    // Code size       34 (0x22)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TStreamTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      string Thrift.Transport.TSocket::host
    IL_000d:  ldarg.0
    IL_000e:  ldarg.2
    IL_000f:  stfld      int32 Thrift.Transport.TSocket::port
    IL_0014:  ldarg.0
    IL_0015:  ldarg.3
    IL_0016:  stfld      int32 Thrift.Transport.TSocket::timeout
    IL_001b:  ldarg.0
    IL_001c:  call       instance void Thrift.Transport.TSocket::InitSocket()
    IL_0021:  ret
  } // end of method TSocket::.ctor

  .method public hidebysig specialname instance void 
          set_Timeout(int32 'value') cil managed
  {
    // Code size       36 (0x24)
    .maxstack  4
    .locals init (int32 V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  dup
    IL_0009:  stloc.0
    IL_000a:  stfld      int32 Thrift.Transport.TSocket::timeout
    IL_000f:  ldloc.0
    IL_0010:  stloc.0
    IL_0011:  ldarg.0
    IL_0012:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0017:  ldloc.0
    IL_0018:  callvirt   instance void [System]System.Net.Sockets.TcpClient::set_SendTimeout(int32)
    IL_001d:  ldloc.0
    IL_001e:  callvirt   instance void [System]System.Net.Sockets.TcpClient::set_ReceiveTimeout(int32)
    IL_0023:  ret
  } // end of method TSocket::set_Timeout

  .method public hidebysig specialname instance class [System]System.Net.Sockets.TcpClient 
          get_TcpClient() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0006:  ret
  } // end of method TSocket::get_TcpClient

  .method public hidebysig specialname instance string 
          get_Host() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      string Thrift.Transport.TSocket::host
    IL_0006:  ret
  } // end of method TSocket::get_Host

  .method public hidebysig specialname instance int32 
          get_Port() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      int32 Thrift.Transport.TSocket::port
    IL_0006:  ret
  } // end of method TSocket::get_Port

  .method public hidebysig specialname virtual 
          instance bool  get_IsOpen() cil managed
  {
    // Code size       25 (0x19)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0006:  brtrue     IL_000d

    IL_000b:  ldc.i4.0
    IL_000c:  ret

    IL_000d:  ldarg.0
    IL_000e:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0013:  callvirt   instance bool [System]System.Net.Sockets.TcpClient::get_Connected()
    IL_0018:  ret
  } // end of method TSocket::get_IsOpen

  .method private hidebysig instance void 
          InitSocket() cil managed
  {
    // Code size       60 (0x3c)
    .maxstack  3
    .locals init (int32 V_0)
    IL_0000:  ldarg.0
    IL_0001:  newobj     instance void [System]System.Net.Sockets.TcpClient::.ctor()
    IL_0006:  stfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_000b:  ldarg.0
    IL_000c:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0011:  ldarg.0
    IL_0012:  ldfld      int32 Thrift.Transport.TSocket::timeout
    IL_0017:  stloc.0
    IL_0018:  ldarg.0
    IL_0019:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_001e:  ldloc.0
    IL_001f:  callvirt   instance void [System]System.Net.Sockets.TcpClient::set_SendTimeout(int32)
    IL_0024:  ldloc.0
    IL_0025:  callvirt   instance void [System]System.Net.Sockets.TcpClient::set_ReceiveTimeout(int32)
    IL_002a:  ldarg.0
    IL_002b:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0030:  callvirt   instance class [System]System.Net.Sockets.Socket [System]System.Net.Sockets.TcpClient::get_Client()
    IL_0035:  ldc.i4.1
    IL_0036:  callvirt   instance void [System]System.Net.Sockets.Socket::set_NoDelay(bool)
    IL_003b:  ret
  } // end of method TSocket::InitSocket

  .method public hidebysig virtual instance void 
          Open() cil managed
  {
    // Code size       347 (0x15b)
    .maxstack  5
    .locals init (class Thrift.Transport.TSocket/ConnectHelper V_0,
             class [mscorlib]System.IAsyncResult V_1,
             bool V_2,
             object V_3)
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance bool Thrift.Transport.TTransport::get_IsOpen()
    IL_0006:  brfalse    IL_0017

    IL_000b:  ldc.i4.2
    IL_000c:  ldstr      "Socket already connected"
    IL_0011:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_0016:  throw

    IL_0017:  ldarg.0
    IL_0018:  ldfld      string Thrift.Transport.TSocket::host
    IL_001d:  call       bool [mscorlib]System.String::IsNullOrEmpty(string)
    IL_0022:  brfalse    IL_0033

    IL_0027:  ldc.i4.1
    IL_0028:  ldstr      "Cannot open null host"
    IL_002d:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_0032:  throw

    IL_0033:  ldarg.0
    IL_0034:  ldfld      int32 Thrift.Transport.TSocket::port
    IL_0039:  ldc.i4.0
    IL_003a:  bgt        IL_004b

    IL_003f:  ldc.i4.1
    IL_0040:  ldstr      "Cannot open without port"
    IL_0045:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_004a:  throw

    IL_004b:  ldarg.0
    IL_004c:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0051:  brtrue     IL_005c

    IL_0056:  ldarg.0
    IL_0057:  call       instance void Thrift.Transport.TSocket::InitSocket()
    IL_005c:  ldarg.0
    IL_005d:  ldfld      int32 Thrift.Transport.TSocket::timeout
    IL_0062:  brtrue     IL_0083

    IL_0067:  ldarg.0
    IL_0068:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_006d:  ldarg.0
    IL_006e:  ldfld      string Thrift.Transport.TSocket::host
    IL_0073:  ldarg.0
    IL_0074:  ldfld      int32 Thrift.Transport.TSocket::port
    IL_0079:  callvirt   instance void [System]System.Net.Sockets.TcpClient::Connect(string,
                                                                                     int32)
    IL_007e:  br         IL_0138

    IL_0083:  ldarg.0
    IL_0084:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0089:  newobj     instance void Thrift.Transport.TSocket/ConnectHelper::.ctor(class [System]System.Net.Sockets.TcpClient)
    IL_008e:  stloc.0
    IL_008f:  ldarg.0
    IL_0090:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0095:  ldarg.0
    IL_0096:  ldfld      string Thrift.Transport.TSocket::host
    IL_009b:  ldarg.0
    IL_009c:  ldfld      int32 Thrift.Transport.TSocket::port
    IL_00a1:  ldnull
    IL_00a2:  ldftn      void Thrift.Transport.TSocket::ConnectCallback(class [mscorlib]System.IAsyncResult)
    IL_00a8:  newobj     instance void [mscorlib]System.AsyncCallback::.ctor(object,
                                                                             native int)
    IL_00ad:  ldloc.0
    IL_00ae:  callvirt   instance class [mscorlib]System.IAsyncResult [System]System.Net.Sockets.TcpClient::BeginConnect(string,
                                                                                                                         int32,
                                                                                                                         class [mscorlib]System.AsyncCallback,
                                                                                                                         object)
    IL_00b3:  stloc.1
    IL_00b4:  ldloc.1
    IL_00b5:  callvirt   instance class [mscorlib]System.Threading.WaitHandle [mscorlib]System.IAsyncResult::get_AsyncWaitHandle()
    IL_00ba:  ldarg.0
    IL_00bb:  ldfld      int32 Thrift.Transport.TSocket::timeout
    IL_00c0:  callvirt   instance bool [mscorlib]System.Threading.WaitHandle::WaitOne(int32)
    IL_00c5:  brfalse    IL_00d7

    IL_00ca:  ldarg.0
    IL_00cb:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_00d0:  callvirt   instance bool [System]System.Net.Sockets.TcpClient::get_Connected()
    IL_00d5:  br.s       IL_00d8

    IL_00d7:  ldc.i4.0
    IL_00d8:  stloc.2
    IL_00d9:  ldloc.2
    IL_00da:  brtrue     IL_0138

    IL_00df:  ldloc.0
    IL_00e0:  ldfld      object Thrift.Transport.TSocket/ConnectHelper::Mutex
    IL_00e5:  stloc.3
    IL_00e6:  ldloc.3
    IL_00e7:  call       void [mscorlib]System.Threading.Monitor::Enter(object)
    .try
    {
      IL_00ec:  ldloc.0
      IL_00ed:  ldfld      bool Thrift.Transport.TSocket/ConnectHelper::CallbackDone
      IL_00f2:  brfalse    IL_0112

      IL_00f7:  ldloc.1
      IL_00f8:  callvirt   instance class [mscorlib]System.Threading.WaitHandle [mscorlib]System.IAsyncResult::get_AsyncWaitHandle()
      IL_00fd:  callvirt   instance void [mscorlib]System.Threading.WaitHandle::Close()
      IL_0102:  ldarg.0
      IL_0103:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
      IL_0108:  callvirt   instance void [System]System.Net.Sockets.TcpClient::Close()
      IL_010d:  br         IL_0120

      IL_0112:  ldloc.0
      IL_0113:  ldc.i4.1
      IL_0114:  stfld      bool Thrift.Transport.TSocket/ConnectHelper::DoCleanup
      IL_0119:  ldarg.0
      IL_011a:  ldnull
      IL_011b:  stfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
      IL_0120:  leave      IL_012c

    }  // end .try
    finally
    {
      IL_0125:  ldloc.3
      IL_0126:  call       void [mscorlib]System.Threading.Monitor::Exit(object)
      IL_012b:  endfinally
    }  // end handler
    IL_012c:  ldc.i4.3
    IL_012d:  ldstr      "Connect timed out"
    IL_0132:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_0137:  throw

    IL_0138:  ldarg.0
    IL_0139:  ldarg.0
    IL_013a:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_013f:  callvirt   instance class [System]System.Net.Sockets.NetworkStream [System]System.Net.Sockets.TcpClient::GetStream()
    IL_0144:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::inputStream
    IL_0149:  ldarg.0
    IL_014a:  ldarg.0
    IL_014b:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0150:  callvirt   instance class [System]System.Net.Sockets.NetworkStream [System]System.Net.Sockets.TcpClient::GetStream()
    IL_0155:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::outputStream
    IL_015a:  ret
  } // end of method TSocket::Open

  .method private hidebysig static void  ConnectCallback(class [mscorlib]System.IAsyncResult asyncres) cil managed
  {
    // Code size       157 (0x9d)
    .maxstack  2
    .locals init (class Thrift.Transport.TSocket/ConnectHelper V_0,
             object V_1)
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance object [mscorlib]System.IAsyncResult::get_AsyncState()
    IL_0006:  isinst     Thrift.Transport.TSocket/ConnectHelper
    IL_000b:  stloc.0
    IL_000c:  ldloc.0
    IL_000d:  ldfld      object Thrift.Transport.TSocket/ConnectHelper::Mutex
    IL_0012:  stloc.1
    IL_0013:  ldloc.1
    IL_0014:  call       void [mscorlib]System.Threading.Monitor::Enter(object)
    .try
    {
      IL_0019:  ldloc.0
      IL_001a:  ldc.i4.1
      IL_001b:  stfld      bool Thrift.Transport.TSocket/ConnectHelper::CallbackDone
      .try
      {
        IL_0020:  ldloc.0
        IL_0021:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket/ConnectHelper::Client
        IL_0026:  callvirt   instance class [System]System.Net.Sockets.Socket [System]System.Net.Sockets.TcpClient::get_Client()
        IL_002b:  brfalse    IL_003c

        IL_0030:  ldloc.0
        IL_0031:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket/ConnectHelper::Client
        IL_0036:  ldarg.0
        IL_0037:  callvirt   instance void [System]System.Net.Sockets.TcpClient::EndConnect(class [mscorlib]System.IAsyncResult)
        IL_003c:  leave      IL_0047

      }  // end .try
      catch [mscorlib]System.Exception 
      {
        IL_0041:  pop
        IL_0042:  leave      IL_0047

      }  // end handler
      IL_0047:  ldloc.0
      IL_0048:  ldfld      bool Thrift.Transport.TSocket/ConnectHelper::DoCleanup
      IL_004d:  brfalse    IL_0090

      .try
      {
        IL_0052:  ldarg.0
        IL_0053:  callvirt   instance class [mscorlib]System.Threading.WaitHandle [mscorlib]System.IAsyncResult::get_AsyncWaitHandle()
        IL_0058:  callvirt   instance void [mscorlib]System.Threading.WaitHandle::Close()
        IL_005d:  leave      IL_0068

      }  // end .try
      catch [mscorlib]System.Exception 
      {
        IL_0062:  pop
        IL_0063:  leave      IL_0068

      }  // end handler
      .try
      {
        IL_0068:  ldloc.0
        IL_0069:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket/ConnectHelper::Client
        IL_006e:  brfalse    IL_007e

        IL_0073:  ldloc.0
        IL_0074:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket/ConnectHelper::Client
        IL_0079:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_007e:  leave      IL_0089

      }  // end .try
      catch [mscorlib]System.Exception 
      {
        IL_0083:  pop
        IL_0084:  leave      IL_0089

      }  // end handler
      IL_0089:  ldloc.0
      IL_008a:  ldnull
      IL_008b:  stfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket/ConnectHelper::Client
      IL_0090:  leave      IL_009c

    }  // end .try
    finally
    {
      IL_0095:  ldloc.1
      IL_0096:  call       void [mscorlib]System.Threading.Monitor::Exit(object)
      IL_009b:  endfinally
    }  // end handler
    IL_009c:  ret
  } // end of method TSocket::ConnectCallback

  .method public hidebysig virtual instance void 
          Close() cil managed
  {
    // Code size       36 (0x24)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TStreamTransport::Close()
    IL_0006:  ldarg.0
    IL_0007:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_000c:  brfalse    IL_0023

    IL_0011:  ldarg.0
    IL_0012:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0017:  callvirt   instance void [System]System.Net.Sockets.TcpClient::Close()
    IL_001c:  ldarg.0
    IL_001d:  ldnull
    IL_001e:  stfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0023:  ret
  } // end of method TSocket::Close

  .method family hidebysig virtual instance void 
          Dispose(bool disposing) cil managed
  {
    // Code size       54 (0x36)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      bool Thrift.Transport.TSocket::_IsDisposed
    IL_0006:  brtrue     IL_002e

    IL_000b:  ldarg.1
    IL_000c:  brfalse    IL_002e

    IL_0011:  ldarg.0
    IL_0012:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0017:  brfalse    IL_0027

    IL_001c:  ldarg.0
    IL_001d:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::client
    IL_0022:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
    IL_0027:  ldarg.0
    IL_0028:  ldarg.1
    IL_0029:  call       instance void Thrift.Transport.TStreamTransport::Dispose(bool)
    IL_002e:  ldarg.0
    IL_002f:  ldc.i4.1
    IL_0030:  stfld      bool Thrift.Transport.TSocket::_IsDisposed
    IL_0035:  ret
  } // end of method TSocket::Dispose

  .property instance int32 Timeout()
  {
    .set instance void Thrift.Transport.TSocket::set_Timeout(int32)
  } // end of property TSocket::Timeout
  .property instance class [System]System.Net.Sockets.TcpClient
          TcpClient()
  {
    .get instance class [System]System.Net.Sockets.TcpClient Thrift.Transport.TSocket::get_TcpClient()
  } // end of property TSocket::TcpClient
  .property instance string Host()
  {
    .get instance string Thrift.Transport.TSocket::get_Host()
  } // end of property TSocket::Host
  .property instance int32 Port()
  {
    .get instance int32 Thrift.Transport.TSocket::get_Port()
  } // end of property TSocket::Port
  .property instance bool IsOpen()
  {
    .get instance bool Thrift.Transport.TSocket::get_IsOpen()
  } // end of property TSocket::IsOpen
} // end of class Thrift.Transport.TSocket

.class public auto ansi beforefieldinit Thrift.Transport.TStreamTransport
       extends Thrift.Transport.TTransport
{
  .field family class [mscorlib]System.IO.Stream inputStream
  .field family class [mscorlib]System.IO.Stream outputStream
  .field private bool _IsDisposed
  .method family hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TTransport::.ctor()
    IL_0006:  ret
  } // end of method TStreamTransport::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class [mscorlib]System.IO.Stream inputStream,
                               class [mscorlib]System.IO.Stream outputStream) cil managed
  {
    // Code size       21 (0x15)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::inputStream
    IL_000d:  ldarg.0
    IL_000e:  ldarg.2
    IL_000f:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::outputStream
    IL_0014:  ret
  } // end of method TStreamTransport::.ctor

  .method public hidebysig specialname instance class [mscorlib]System.IO.Stream 
          get_OutputStream() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::outputStream
    IL_0006:  ret
  } // end of method TStreamTransport::get_OutputStream

  .method public hidebysig specialname instance class [mscorlib]System.IO.Stream 
          get_InputStream() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::inputStream
    IL_0006:  ret
  } // end of method TStreamTransport::get_InputStream

  .method public hidebysig specialname virtual 
          instance bool  get_IsOpen() cil managed
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldc.i4.1
    IL_0001:  ret
  } // end of method TStreamTransport::get_IsOpen

  .method public hidebysig virtual instance void 
          Open() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TStreamTransport::Open

  .method public hidebysig virtual instance void 
          Close() cil managed
  {
    // Code size       59 (0x3b)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::inputStream
    IL_0006:  brfalse    IL_001d

    IL_000b:  ldarg.0
    IL_000c:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::inputStream
    IL_0011:  callvirt   instance void [mscorlib]System.IO.Stream::Close()
    IL_0016:  ldarg.0
    IL_0017:  ldnull
    IL_0018:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::inputStream
    IL_001d:  ldarg.0
    IL_001e:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::outputStream
    IL_0023:  brfalse    IL_003a

    IL_0028:  ldarg.0
    IL_0029:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::outputStream
    IL_002e:  callvirt   instance void [mscorlib]System.IO.Stream::Close()
    IL_0033:  ldarg.0
    IL_0034:  ldnull
    IL_0035:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::outputStream
    IL_003a:  ret
  } // end of method TStreamTransport::Close

  .method public hidebysig virtual instance int32 
          Read(uint8[] buf,
               int32 'off',
               int32 len) cil managed
  {
    // Code size       38 (0x26)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::inputStream
    IL_0006:  brtrue     IL_0017

    IL_000b:  ldc.i4.1
    IL_000c:  ldstr      "Cannot read from null inputstream"
    IL_0011:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_0016:  throw

    IL_0017:  ldarg.0
    IL_0018:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::inputStream
    IL_001d:  ldarg.1
    IL_001e:  ldarg.2
    IL_001f:  ldarg.3
    IL_0020:  callvirt   instance int32 [mscorlib]System.IO.Stream::Read(uint8[],
                                                                         int32,
                                                                         int32)
    IL_0025:  ret
  } // end of method TStreamTransport::Read

  .method public hidebysig virtual instance void 
          Write(uint8[] buf,
                int32 'off',
                int32 len) cil managed
  {
    // Code size       38 (0x26)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::outputStream
    IL_0006:  brtrue     IL_0017

    IL_000b:  ldc.i4.1
    IL_000c:  ldstr      "Cannot write to null outputstream"
    IL_0011:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_0016:  throw

    IL_0017:  ldarg.0
    IL_0018:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::outputStream
    IL_001d:  ldarg.1
    IL_001e:  ldarg.2
    IL_001f:  ldarg.3
    IL_0020:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                         int32,
                                                                         int32)
    IL_0025:  ret
  } // end of method TStreamTransport::Write

  .method public hidebysig virtual instance void 
          Flush() cil managed
  {
    // Code size       35 (0x23)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::outputStream
    IL_0006:  brtrue     IL_0017

    IL_000b:  ldc.i4.1
    IL_000c:  ldstr      "Cannot flush null outputstream"
    IL_0011:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_0016:  throw

    IL_0017:  ldarg.0
    IL_0018:  ldfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::outputStream
    IL_001d:  callvirt   instance void [mscorlib]System.IO.Stream::Flush()
    IL_0022:  ret
  } // end of method TStreamTransport::Flush

  .method family hidebysig virtual instance void 
          Dispose(bool disposing) cil managed
  {
    // Code size       69 (0x45)
    .maxstack  2
    IL_0000:  ldarg.0
    IL_0001:  ldfld      bool Thrift.Transport.TStreamTransport::_IsDisposed
    IL_0006:  brtrue     IL_003d

    IL_000b:  ldarg.1
    IL_000c:  brfalse    IL_003d

    IL_0011:  ldarg.0
    IL_0012:  call       instance class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::get_InputStream()
    IL_0017:  brfalse    IL_0027

    IL_001c:  ldarg.0
    IL_001d:  call       instance class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::get_InputStream()
    IL_0022:  callvirt   instance void [mscorlib]System.IO.Stream::Dispose()
    IL_0027:  ldarg.0
    IL_0028:  call       instance class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::get_OutputStream()
    IL_002d:  brfalse    IL_003d

    IL_0032:  ldarg.0
    IL_0033:  call       instance class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::get_OutputStream()
    IL_0038:  callvirt   instance void [mscorlib]System.IO.Stream::Dispose()
    IL_003d:  ldarg.0
    IL_003e:  ldc.i4.1
    IL_003f:  stfld      bool Thrift.Transport.TStreamTransport::_IsDisposed
    IL_0044:  ret
  } // end of method TStreamTransport::Dispose

  .property instance class [mscorlib]System.IO.Stream
          OutputStream()
  {
    .get instance class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::get_OutputStream()
  } // end of property TStreamTransport::OutputStream
  .property instance class [mscorlib]System.IO.Stream
          InputStream()
  {
    .get instance class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::get_InputStream()
  } // end of property TStreamTransport::InputStream
  .property instance bool IsOpen()
  {
    .get instance bool Thrift.Transport.TStreamTransport::get_IsOpen()
  } // end of property TStreamTransport::IsOpen
} // end of class Thrift.Transport.TStreamTransport

.class public auto ansi beforefieldinit Thrift.Transport.TTLSServerSocket
       extends Thrift.Transport.TServerTransport
{
  .field private class [System]System.Net.Sockets.TcpListener server
  .field private int32 port
  .field private int32 clientTimeout
  .field private bool useBufferedSockets
  .field private class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate serverCertificate
  .field private class [System]System.Net.Security.RemoteCertificateValidationCallback clientCertValidator
  .field private class [System]System.Net.Security.LocalCertificateSelectionCallback localCertificateSelectionCallback
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(int32 port,
                               class [System]System.Security.Cryptography.X509Certificates.X509Certificate2 certificate) cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldc.i4.0
    IL_0003:  ldarg.2
    IL_0004:  call       instance void Thrift.Transport.TTLSServerSocket::.ctor(int32,
                                                                                int32,
                                                                                class [System]System.Security.Cryptography.X509Certificates.X509Certificate2)
    IL_0009:  ret
  } // end of method TTLSServerSocket::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(int32 port,
                               int32 clientTimeout,
                               class [System]System.Security.Cryptography.X509Certificates.X509Certificate2 certificate) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldc.i4.0
    IL_0004:  ldarg.3
    IL_0005:  ldnull
    IL_0006:  ldnull
    IL_0007:  call       instance void Thrift.Transport.TTLSServerSocket::.ctor(int32,
                                                                                int32,
                                                                                bool,
                                                                                class [System]System.Security.Cryptography.X509Certificates.X509Certificate2,
                                                                                class [System]System.Net.Security.RemoteCertificateValidationCallback,
                                                                                class [System]System.Net.Security.LocalCertificateSelectionCallback)
    IL_000c:  ret
  } // end of method TTLSServerSocket::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(int32 port,
                               int32 clientTimeout,
                               bool useBufferedSockets,
                               class [System]System.Security.Cryptography.X509Certificates.X509Certificate2 certificate,
                               [opt] class [System]System.Net.Security.RemoteCertificateValidationCallback clientCertValidator,
                               [opt] class [System]System.Net.Security.LocalCertificateSelectionCallback localCertificateSelectionCallback) cil managed
  {
    .param [5] = nullref
    .param [6] = nullref
    // Code size       148 (0x94)
    .maxstack  3
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TServerTransport::.ctor()
    IL_0006:  ldarg.s    certificate
    IL_0008:  callvirt   instance bool [System]System.Security.Cryptography.X509Certificates.X509Certificate2::get_HasPrivateKey()
    IL_000d:  brtrue     IL_001e

    IL_0012:  ldc.i4.0
    IL_0013:  ldstr      "Your server-certificate needs to have a private key"
    IL_0018:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_001d:  throw

    IL_001e:  ldarg.0
    IL_001f:  ldarg.1
    IL_0020:  stfld      int32 Thrift.Transport.TTLSServerSocket::port
    IL_0025:  ldarg.0
    IL_0026:  ldarg.s    certificate
    IL_0028:  stfld      class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate Thrift.Transport.TTLSServerSocket::serverCertificate
    IL_002d:  ldarg.0
    IL_002e:  ldarg.3
    IL_002f:  stfld      bool Thrift.Transport.TTLSServerSocket::useBufferedSockets
    IL_0034:  ldarg.0
    IL_0035:  ldarg.s    clientCertValidator
    IL_0037:  stfld      class [System]System.Net.Security.RemoteCertificateValidationCallback Thrift.Transport.TTLSServerSocket::clientCertValidator
    IL_003c:  ldarg.0
    IL_003d:  ldarg.s    localCertificateSelectionCallback
    IL_003f:  stfld      class [System]System.Net.Security.LocalCertificateSelectionCallback Thrift.Transport.TTLSServerSocket::localCertificateSelectionCallback
    .try
    {
      IL_0044:  ldarg.0
      IL_0045:  ldsfld     class [System]System.Net.IPAddress [System]System.Net.IPAddress::Any
      IL_004a:  ldarg.0
      IL_004b:  ldfld      int32 Thrift.Transport.TTLSServerSocket::port
      IL_0050:  newobj     instance void [System]System.Net.Sockets.TcpListener::.ctor(class [System]System.Net.IPAddress,
                                                                                       int32)
      IL_0055:  stfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TTLSServerSocket::server
      IL_005a:  ldarg.0
      IL_005b:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TTLSServerSocket::server
      IL_0060:  callvirt   instance class [System]System.Net.Sockets.Socket [System]System.Net.Sockets.TcpListener::get_Server()
      IL_0065:  ldc.i4.1
      IL_0066:  callvirt   instance void [System]System.Net.Sockets.Socket::set_NoDelay(bool)
      IL_006b:  leave      IL_0093

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_0070:  pop
      IL_0071:  ldarg.0
      IL_0072:  ldnull
      IL_0073:  stfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TTLSServerSocket::server
      IL_0078:  ldstr      "Could not create ServerSocket on port "
      IL_007d:  ldarg.1
      IL_007e:  box        [mscorlib]System.Int32
      IL_0083:  ldstr      "."
      IL_0088:  call       string [mscorlib]System.String::Concat(object,
                                                                  object,
                                                                  object)
      IL_008d:  newobj     instance void Thrift.Transport.TTransportException::.ctor(string)
      IL_0092:  throw

    }  // end handler
    IL_0093:  ret
  } // end of method TTLSServerSocket::.ctor

  .method public hidebysig virtual instance void 
          Listen() cil managed
  {
    // Code size       51 (0x33)
    .maxstack  2
    .locals init (class [System]System.Net.Sockets.SocketException V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TTLSServerSocket::server
    IL_0006:  brfalse    IL_0032

    .try
    {
      IL_000b:  ldarg.0
      IL_000c:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TTLSServerSocket::server
      IL_0011:  callvirt   instance void [System]System.Net.Sockets.TcpListener::Start()
      IL_0016:  leave      IL_0032

    }  // end .try
    catch [System]System.Net.Sockets.SocketException 
    {
      IL_001b:  stloc.0
      IL_001c:  ldstr      "Could not accept on listening socket: "
      IL_0021:  ldloc.0
      IL_0022:  callvirt   instance string [mscorlib]System.Exception::get_Message()
      IL_0027:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_002c:  newobj     instance void Thrift.Transport.TTransportException::.ctor(string)
      IL_0031:  throw

    }  // end handler
    IL_0032:  ret
  } // end of method TTLSServerSocket::Listen

  .method family hidebysig virtual instance class Thrift.Transport.TTransport 
          AcceptImpl() cil managed
  {
    // Code size       140 (0x8c)
    .maxstack  5
    .locals init (class [System]System.Net.Sockets.TcpClient V_0,
             int32 V_1,
             class Thrift.Transport.TTLSSocket V_2,
             class Thrift.Transport.TBufferedTransport V_3,
             class Thrift.Transport.TTransport V_4,
             class [mscorlib]System.Exception V_5)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TTLSServerSocket::server
    IL_0006:  brtrue     IL_0017

    IL_000b:  ldc.i4.1
    IL_000c:  ldstr      "No underlying server socket."
    IL_0011:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_0016:  throw

    .try
    {
      IL_0017:  ldarg.0
      IL_0018:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TTLSServerSocket::server
      IL_001d:  callvirt   instance class [System]System.Net.Sockets.TcpClient [System]System.Net.Sockets.TcpListener::AcceptTcpClient()
      IL_0022:  stloc.0
      IL_0023:  ldloc.0
      IL_0024:  ldarg.0
      IL_0025:  ldfld      int32 Thrift.Transport.TTLSServerSocket::clientTimeout
      IL_002a:  stloc.1
      IL_002b:  ldloc.0
      IL_002c:  ldloc.1
      IL_002d:  callvirt   instance void [System]System.Net.Sockets.TcpClient::set_ReceiveTimeout(int32)
      IL_0032:  ldloc.1
      IL_0033:  callvirt   instance void [System]System.Net.Sockets.TcpClient::set_SendTimeout(int32)
      IL_0038:  ldloc.0
      IL_0039:  ldarg.0
      IL_003a:  ldfld      class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate Thrift.Transport.TTLSServerSocket::serverCertificate
      IL_003f:  ldc.i4.1
      IL_0040:  ldarg.0
      IL_0041:  ldfld      class [System]System.Net.Security.RemoteCertificateValidationCallback Thrift.Transport.TTLSServerSocket::clientCertValidator
      IL_0046:  ldarg.0
      IL_0047:  ldfld      class [System]System.Net.Security.LocalCertificateSelectionCallback Thrift.Transport.TTLSServerSocket::localCertificateSelectionCallback
      IL_004c:  newobj     instance void Thrift.Transport.TTLSSocket::.ctor(class [System]System.Net.Sockets.TcpClient,
                                                                            class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate,
                                                                            bool,
                                                                            class [System]System.Net.Security.RemoteCertificateValidationCallback,
                                                                            class [System]System.Net.Security.LocalCertificateSelectionCallback)
      IL_0051:  stloc.2
      IL_0052:  ldloc.2
      IL_0053:  callvirt   instance void Thrift.Transport.TTLSSocket::setupTLS()
      IL_0058:  ldarg.0
      IL_0059:  ldfld      bool Thrift.Transport.TTLSServerSocket::useBufferedSockets
      IL_005e:  brfalse    IL_0072

      IL_0063:  ldloc.2
      IL_0064:  newobj     instance void Thrift.Transport.TBufferedTransport::.ctor(class Thrift.Transport.TStreamTransport)
      IL_0069:  stloc.3
      IL_006a:  ldloc.3
      IL_006b:  stloc.s    V_4
      IL_006d:  leave      IL_0089

      IL_0072:  ldloc.2
      IL_0073:  stloc.s    V_4
      IL_0075:  leave      IL_0089

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_007a:  stloc.s    V_5
      IL_007c:  ldloc.s    V_5
      IL_007e:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_0083:  newobj     instance void Thrift.Transport.TTransportException::.ctor(string)
      IL_0088:  throw

    }  // end handler
    IL_0089:  ldloc.s    V_4
    IL_008b:  ret
  } // end of method TTLSServerSocket::AcceptImpl

  .method public hidebysig virtual instance void 
          Close() cil managed
  {
    // Code size       53 (0x35)
    .maxstack  2
    .locals init (class [mscorlib]System.Exception V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TTLSServerSocket::server
    IL_0006:  brfalse    IL_0034

    .try
    {
      IL_000b:  ldarg.0
      IL_000c:  ldfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TTLSServerSocket::server
      IL_0011:  callvirt   instance void [System]System.Net.Sockets.TcpListener::Stop()
      IL_0016:  leave      IL_002d

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_001b:  stloc.0
      IL_001c:  ldstr      "WARNING: Could not close server socket: "
      IL_0021:  ldloc.0
      IL_0022:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_0027:  newobj     instance void Thrift.Transport.TTransportException::.ctor(string)
      IL_002c:  throw

    }  // end handler
    IL_002d:  ldarg.0
    IL_002e:  ldnull
    IL_002f:  stfld      class [System]System.Net.Sockets.TcpListener Thrift.Transport.TTLSServerSocket::server
    IL_0034:  ret
  } // end of method TTLSServerSocket::Close

} // end of class Thrift.Transport.TTLSServerSocket

.class public auto ansi beforefieldinit Thrift.Transport.TTLSSocket
       extends Thrift.Transport.TStreamTransport
{
  .field private class [System]System.Net.Sockets.TcpClient client
  .field private string host
  .field private int32 port
  .field private int32 timeout
  .field private class [System]System.Net.Security.SslStream secureStream
  .field private bool isServer
  .field private class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate certificate
  .field private class [System]System.Net.Security.RemoteCertificateValidationCallback certValidator
  .field private class [System]System.Net.Security.LocalCertificateSelectionCallback localCertificateSelectionCallback
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class [System]System.Net.Sockets.TcpClient client,
                               class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                               [opt] bool isServer,
                               [opt] class [System]System.Net.Security.RemoteCertificateValidationCallback certValidator,
                               [opt] class [System]System.Net.Security.LocalCertificateSelectionCallback localCertificateSelectionCallback) cil managed
  {
    .param [3] = bool(false)
    .param [4] = nullref
    .param [5] = nullref
    // Code size       79 (0x4f)
    .maxstack  2
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TStreamTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_000d:  ldarg.0
    IL_000e:  ldarg.2
    IL_000f:  stfld      class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate Thrift.Transport.TTLSSocket::certificate
    IL_0014:  ldarg.0
    IL_0015:  ldarg.s    certValidator
    IL_0017:  stfld      class [System]System.Net.Security.RemoteCertificateValidationCallback Thrift.Transport.TTLSSocket::certValidator
    IL_001c:  ldarg.0
    IL_001d:  ldarg.s    localCertificateSelectionCallback
    IL_001f:  stfld      class [System]System.Net.Security.LocalCertificateSelectionCallback Thrift.Transport.TTLSSocket::localCertificateSelectionCallback
    IL_0024:  ldarg.0
    IL_0025:  ldarg.3
    IL_0026:  stfld      bool Thrift.Transport.TTLSSocket::isServer
    IL_002b:  ldarg.0
    IL_002c:  callvirt   instance bool Thrift.Transport.TTransport::get_IsOpen()
    IL_0031:  brfalse    IL_004e

    IL_0036:  ldarg.0
    IL_0037:  ldarg.1
    IL_0038:  callvirt   instance class [System]System.Net.Sockets.NetworkStream [System]System.Net.Sockets.TcpClient::GetStream()
    IL_003d:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::inputStream
    IL_0042:  ldarg.0
    IL_0043:  ldarg.1
    IL_0044:  callvirt   instance class [System]System.Net.Sockets.NetworkStream [System]System.Net.Sockets.TcpClient::GetStream()
    IL_0049:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::outputStream
    IL_004e:  ret
  } // end of method TTLSSocket::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string host,
                               int32 port,
                               string certificatePath,
                               [opt] class [System]System.Net.Security.RemoteCertificateValidationCallback certValidator,
                               [opt] class [System]System.Net.Security.LocalCertificateSelectionCallback localCertificateSelectionCallback) cil managed
  {
    .param [4] = nullref
    .param [5] = nullref
    // Code size       20 (0x14)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldc.i4.0
    IL_0004:  ldarg.3
    IL_0005:  call       class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate::CreateFromCertFile(string)
    IL_000a:  ldarg.s    certValidator
    IL_000c:  ldarg.s    localCertificateSelectionCallback
    IL_000e:  call       instance void Thrift.Transport.TTLSSocket::.ctor(string,
                                                                          int32,
                                                                          int32,
                                                                          class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate,
                                                                          class [System]System.Net.Security.RemoteCertificateValidationCallback,
                                                                          class [System]System.Net.Security.LocalCertificateSelectionCallback)
    IL_0013:  ret
  } // end of method TTLSSocket::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string host,
                               int32 port,
                               class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                               [opt] class [System]System.Net.Security.RemoteCertificateValidationCallback certValidator,
                               [opt] class [System]System.Net.Security.LocalCertificateSelectionCallback localCertificateSelectionCallback) cil managed
  {
    .param [4] = nullref
    .param [5] = nullref
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldarg.2
    IL_0003:  ldc.i4.0
    IL_0004:  ldarg.3
    IL_0005:  ldarg.s    certValidator
    IL_0007:  ldarg.s    localCertificateSelectionCallback
    IL_0009:  call       instance void Thrift.Transport.TTLSSocket::.ctor(string,
                                                                          int32,
                                                                          int32,
                                                                          class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate,
                                                                          class [System]System.Net.Security.RemoteCertificateValidationCallback,
                                                                          class [System]System.Net.Security.LocalCertificateSelectionCallback)
    IL_000e:  ret
  } // end of method TTLSSocket::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string host,
                               int32 port,
                               int32 timeout,
                               class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                               [opt] class [System]System.Net.Security.RemoteCertificateValidationCallback certValidator,
                               [opt] class [System]System.Net.Security.LocalCertificateSelectionCallback localCertificateSelectionCallback) cil managed
  {
    .param [5] = nullref
    .param [6] = nullref
    // Code size       58 (0x3a)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TStreamTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      string Thrift.Transport.TTLSSocket::host
    IL_000d:  ldarg.0
    IL_000e:  ldarg.2
    IL_000f:  stfld      int32 Thrift.Transport.TTLSSocket::port
    IL_0014:  ldarg.0
    IL_0015:  ldarg.3
    IL_0016:  stfld      int32 Thrift.Transport.TTLSSocket::timeout
    IL_001b:  ldarg.0
    IL_001c:  ldarg.s    certificate
    IL_001e:  stfld      class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate Thrift.Transport.TTLSSocket::certificate
    IL_0023:  ldarg.0
    IL_0024:  ldarg.s    certValidator
    IL_0026:  stfld      class [System]System.Net.Security.RemoteCertificateValidationCallback Thrift.Transport.TTLSSocket::certValidator
    IL_002b:  ldarg.0
    IL_002c:  ldarg.s    localCertificateSelectionCallback
    IL_002e:  stfld      class [System]System.Net.Security.LocalCertificateSelectionCallback Thrift.Transport.TTLSSocket::localCertificateSelectionCallback
    IL_0033:  ldarg.0
    IL_0034:  call       instance void Thrift.Transport.TTLSSocket::InitSocket()
    IL_0039:  ret
  } // end of method TTLSSocket::.ctor

  .method public hidebysig specialname instance void 
          set_Timeout(int32 'value') cil managed
  {
    // Code size       36 (0x24)
    .maxstack  4
    .locals init (int32 V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  dup
    IL_0009:  stloc.0
    IL_000a:  stfld      int32 Thrift.Transport.TTLSSocket::timeout
    IL_000f:  ldloc.0
    IL_0010:  stloc.0
    IL_0011:  ldarg.0
    IL_0012:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_0017:  ldloc.0
    IL_0018:  callvirt   instance void [System]System.Net.Sockets.TcpClient::set_SendTimeout(int32)
    IL_001d:  ldloc.0
    IL_001e:  callvirt   instance void [System]System.Net.Sockets.TcpClient::set_ReceiveTimeout(int32)
    IL_0023:  ret
  } // end of method TTLSSocket::set_Timeout

  .method public hidebysig specialname instance class [System]System.Net.Sockets.TcpClient 
          get_TcpClient() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_0006:  ret
  } // end of method TTLSSocket::get_TcpClient

  .method public hidebysig specialname instance string 
          get_Host() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      string Thrift.Transport.TTLSSocket::host
    IL_0006:  ret
  } // end of method TTLSSocket::get_Host

  .method public hidebysig specialname instance int32 
          get_Port() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      int32 Thrift.Transport.TTLSSocket::port
    IL_0006:  ret
  } // end of method TTLSSocket::get_Port

  .method public hidebysig specialname virtual 
          instance bool  get_IsOpen() cil managed
  {
    // Code size       25 (0x19)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_0006:  brtrue     IL_000d

    IL_000b:  ldc.i4.0
    IL_000c:  ret

    IL_000d:  ldarg.0
    IL_000e:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_0013:  callvirt   instance bool [System]System.Net.Sockets.TcpClient::get_Connected()
    IL_0018:  ret
  } // end of method TTLSSocket::get_IsOpen

  .method private hidebysig instance void 
          InitSocket() cil managed
  {
    // Code size       60 (0x3c)
    .maxstack  3
    .locals init (int32 V_0)
    IL_0000:  ldarg.0
    IL_0001:  newobj     instance void [System]System.Net.Sockets.TcpClient::.ctor()
    IL_0006:  stfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_000b:  ldarg.0
    IL_000c:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_0011:  ldarg.0
    IL_0012:  ldfld      int32 Thrift.Transport.TTLSSocket::timeout
    IL_0017:  stloc.0
    IL_0018:  ldarg.0
    IL_0019:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_001e:  ldloc.0
    IL_001f:  callvirt   instance void [System]System.Net.Sockets.TcpClient::set_SendTimeout(int32)
    IL_0024:  ldloc.0
    IL_0025:  callvirt   instance void [System]System.Net.Sockets.TcpClient::set_ReceiveTimeout(int32)
    IL_002a:  ldarg.0
    IL_002b:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_0030:  callvirt   instance class [System]System.Net.Sockets.Socket [System]System.Net.Sockets.TcpClient::get_Client()
    IL_0035:  ldc.i4.1
    IL_0036:  callvirt   instance void [System]System.Net.Sockets.Socket::set_NoDelay(bool)
    IL_003b:  ret
  } // end of method TTLSSocket::InitSocket

  .method private hidebysig instance bool 
          DefaultCertificateValidator(object sender,
                                      class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                      class [System]System.Security.Cryptography.X509Certificates.X509Chain chain,
                                      valuetype [System]System.Net.Security.SslPolicyErrors sslValidationErrors) cil managed
  {
    // Code size       6 (0x6)
    .maxstack  8
    IL_0000:  ldarg.s    sslValidationErrors
    IL_0002:  ldc.i4.0
    IL_0003:  ceq
    IL_0005:  ret
  } // end of method TTLSSocket::DefaultCertificateValidator

  .method public hidebysig virtual instance void 
          Open() cil managed
  {
    // Code size       122 (0x7a)
    .maxstack  3
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance bool Thrift.Transport.TTransport::get_IsOpen()
    IL_0006:  brfalse    IL_0017

    IL_000b:  ldc.i4.2
    IL_000c:  ldstr      "Socket already connected"
    IL_0011:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_0016:  throw

    IL_0017:  ldarg.0
    IL_0018:  ldfld      string Thrift.Transport.TTLSSocket::host
    IL_001d:  call       bool [mscorlib]System.String::IsNullOrEmpty(string)
    IL_0022:  brfalse    IL_0033

    IL_0027:  ldc.i4.1
    IL_0028:  ldstr      "Cannot open null host"
    IL_002d:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_0032:  throw

    IL_0033:  ldarg.0
    IL_0034:  ldfld      int32 Thrift.Transport.TTLSSocket::port
    IL_0039:  ldc.i4.0
    IL_003a:  bgt        IL_004b

    IL_003f:  ldc.i4.1
    IL_0040:  ldstr      "Cannot open without port"
    IL_0045:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_004a:  throw

    IL_004b:  ldarg.0
    IL_004c:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_0051:  brtrue     IL_005c

    IL_0056:  ldarg.0
    IL_0057:  call       instance void Thrift.Transport.TTLSSocket::InitSocket()
    IL_005c:  ldarg.0
    IL_005d:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_0062:  ldarg.0
    IL_0063:  ldfld      string Thrift.Transport.TTLSSocket::host
    IL_0068:  ldarg.0
    IL_0069:  ldfld      int32 Thrift.Transport.TTLSSocket::port
    IL_006e:  callvirt   instance void [System]System.Net.Sockets.TcpClient::Connect(string,
                                                                                     int32)
    IL_0073:  ldarg.0
    IL_0074:  call       instance void Thrift.Transport.TTLSSocket::setupTLS()
    IL_0079:  ret
  } // end of method TTLSSocket::Open

  .method public hidebysig instance void 
          setupTLS() cil managed
  {
    // Code size       229 (0xe5)
    .maxstack  5
    .locals init (class [System]System.Net.Security.RemoteCertificateValidationCallback V_0,
             class [System]System.Security.Cryptography.X509Certificates.X509CertificateCollection V_1)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [System]System.Net.Security.RemoteCertificateValidationCallback Thrift.Transport.TTLSSocket::certValidator
    IL_0006:  dup
    IL_0007:  brtrue     IL_0019

    IL_000c:  pop
    IL_000d:  ldarg.0
    IL_000e:  ldftn      instance bool Thrift.Transport.TTLSSocket::DefaultCertificateValidator(object,
                                                                                                class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate,
                                                                                                class [System]System.Security.Cryptography.X509Certificates.X509Chain,
                                                                                                valuetype [System]System.Net.Security.SslPolicyErrors)
    IL_0014:  newobj     instance void [System]System.Net.Security.RemoteCertificateValidationCallback::.ctor(object,
                                                                                                              native int)
    IL_0019:  stloc.0
    IL_001a:  ldarg.0
    IL_001b:  ldfld      class [System]System.Net.Security.LocalCertificateSelectionCallback Thrift.Transport.TTLSSocket::localCertificateSelectionCallback
    IL_0020:  brfalse    IL_0048

    IL_0025:  ldarg.0
    IL_0026:  ldarg.0
    IL_0027:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_002c:  callvirt   instance class [System]System.Net.Sockets.NetworkStream [System]System.Net.Sockets.TcpClient::GetStream()
    IL_0031:  ldc.i4.0
    IL_0032:  ldloc.0
    IL_0033:  ldarg.0
    IL_0034:  ldfld      class [System]System.Net.Security.LocalCertificateSelectionCallback Thrift.Transport.TTLSSocket::localCertificateSelectionCallback
    IL_0039:  newobj     instance void [System]System.Net.Security.SslStream::.ctor(class [mscorlib]System.IO.Stream,
                                                                                    bool,
                                                                                    class [System]System.Net.Security.RemoteCertificateValidationCallback,
                                                                                    class [System]System.Net.Security.LocalCertificateSelectionCallback)
    IL_003e:  stfld      class [System]System.Net.Security.SslStream Thrift.Transport.TTLSSocket::secureStream
    IL_0043:  br         IL_0060

    IL_0048:  ldarg.0
    IL_0049:  ldarg.0
    IL_004a:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_004f:  callvirt   instance class [System]System.Net.Sockets.NetworkStream [System]System.Net.Sockets.TcpClient::GetStream()
    IL_0054:  ldc.i4.0
    IL_0055:  ldloc.0
    IL_0056:  newobj     instance void [System]System.Net.Security.SslStream::.ctor(class [mscorlib]System.IO.Stream,
                                                                                    bool,
                                                                                    class [System]System.Net.Security.RemoteCertificateValidationCallback)
    IL_005b:  stfld      class [System]System.Net.Security.SslStream Thrift.Transport.TTLSSocket::secureStream
    .try
    {
      IL_0060:  ldarg.0
      IL_0061:  ldfld      bool Thrift.Transport.TTLSSocket::isServer
      IL_0066:  brfalse    IL_0093

      IL_006b:  ldarg.0
      IL_006c:  ldfld      class [System]System.Net.Security.SslStream Thrift.Transport.TTLSSocket::secureStream
      IL_0071:  ldarg.0
      IL_0072:  ldfld      class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate Thrift.Transport.TTLSSocket::certificate
      IL_0077:  ldarg.0
      IL_0078:  ldfld      class [System]System.Net.Security.RemoteCertificateValidationCallback Thrift.Transport.TTLSSocket::certValidator
      IL_007d:  ldnull
      IL_007e:  ceq
      IL_0080:  ldc.i4.0
      IL_0081:  ceq
      IL_0083:  ldc.i4     0xc0
      IL_0088:  ldc.i4.1
      IL_0089:  callvirt   instance void [System]System.Net.Security.SslStream::AuthenticateAsServer(class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate,
                                                                                                     bool,
                                                                                                     valuetype [System]System.Security.Authentication.SslProtocols,
                                                                                                     bool)
      IL_008e:  br         IL_00be

      IL_0093:  ldarg.0
      IL_0094:  ldfld      class [System]System.Net.Security.SslStream Thrift.Transport.TTLSSocket::secureStream
      IL_0099:  ldarg.0
      IL_009a:  ldfld      string Thrift.Transport.TTLSSocket::host
      IL_009f:  newobj     instance void [System]System.Security.Cryptography.X509Certificates.X509CertificateCollection::.ctor()
      IL_00a4:  stloc.1
      IL_00a5:  ldloc.1
      IL_00a6:  ldarg.0
      IL_00a7:  ldfld      class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate Thrift.Transport.TTLSSocket::certificate
      IL_00ac:  callvirt   instance int32 [System]System.Security.Cryptography.X509Certificates.X509CertificateCollection::Add(class [mscorlib]System.Security.Cryptography.X509Certificates.X509Certificate)
      IL_00b1:  pop
      IL_00b2:  ldloc.1
      IL_00b3:  ldc.i4     0xc0
      IL_00b8:  ldc.i4.1
      IL_00b9:  callvirt   instance void [System]System.Net.Security.SslStream::AuthenticateAsClient(string,
                                                                                                     class [System]System.Security.Cryptography.X509Certificates.X509CertificateCollection,
                                                                                                     valuetype [System]System.Security.Authentication.SslProtocols,
                                                                                                     bool)
      IL_00be:  leave      IL_00cc

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_00c3:  pop
      IL_00c4:  ldarg.0
      IL_00c5:  callvirt   instance void Thrift.Transport.TTransport::Close()
      IL_00ca:  rethrow
    }  // end handler
    IL_00cc:  ldarg.0
    IL_00cd:  ldarg.0
    IL_00ce:  ldfld      class [System]System.Net.Security.SslStream Thrift.Transport.TTLSSocket::secureStream
    IL_00d3:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::inputStream
    IL_00d8:  ldarg.0
    IL_00d9:  ldarg.0
    IL_00da:  ldfld      class [System]System.Net.Security.SslStream Thrift.Transport.TTLSSocket::secureStream
    IL_00df:  stfld      class [mscorlib]System.IO.Stream Thrift.Transport.TStreamTransport::outputStream
    IL_00e4:  ret
  } // end of method TTLSSocket::setupTLS

  .method public hidebysig virtual instance void 
          Close() cil managed
  {
    // Code size       65 (0x41)
    .maxstack  2
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TStreamTransport::Close()
    IL_0006:  ldarg.0
    IL_0007:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_000c:  brfalse    IL_0023

    IL_0011:  ldarg.0
    IL_0012:  ldfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_0017:  callvirt   instance void [System]System.Net.Sockets.TcpClient::Close()
    IL_001c:  ldarg.0
    IL_001d:  ldnull
    IL_001e:  stfld      class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::client
    IL_0023:  ldarg.0
    IL_0024:  ldfld      class [System]System.Net.Security.SslStream Thrift.Transport.TTLSSocket::secureStream
    IL_0029:  brfalse    IL_0040

    IL_002e:  ldarg.0
    IL_002f:  ldfld      class [System]System.Net.Security.SslStream Thrift.Transport.TTLSSocket::secureStream
    IL_0034:  callvirt   instance void [mscorlib]System.IO.Stream::Close()
    IL_0039:  ldarg.0
    IL_003a:  ldnull
    IL_003b:  stfld      class [System]System.Net.Security.SslStream Thrift.Transport.TTLSSocket::secureStream
    IL_0040:  ret
  } // end of method TTLSSocket::Close

  .property instance int32 Timeout()
  {
    .set instance void Thrift.Transport.TTLSSocket::set_Timeout(int32)
  } // end of property TTLSSocket::Timeout
  .property instance class [System]System.Net.Sockets.TcpClient
          TcpClient()
  {
    .get instance class [System]System.Net.Sockets.TcpClient Thrift.Transport.TTLSSocket::get_TcpClient()
  } // end of property TTLSSocket::TcpClient
  .property instance string Host()
  {
    .get instance string Thrift.Transport.TTLSSocket::get_Host()
  } // end of property TTLSSocket::Host
  .property instance int32 Port()
  {
    .get instance int32 Thrift.Transport.TTLSSocket::get_Port()
  } // end of property TTLSSocket::Port
  .property instance bool IsOpen()
  {
    .get instance bool Thrift.Transport.TTLSSocket::get_IsOpen()
  } // end of property TTLSSocket::IsOpen
} // end of class Thrift.Transport.TTLSSocket

.class public abstract auto ansi beforefieldinit Thrift.Transport.TTransport
       extends [mscorlib]System.Object
       implements [mscorlib]System.IDisposable
{
  .field private uint8[] _peekBuffer
  .field private bool _hasPeekByte
  .method public hidebysig newslot specialname abstract virtual 
          instance bool  get_IsOpen() cil managed
  {
  } // end of method TTransport::get_IsOpen

  .method family hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       19 (0x13)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.1
    IL_0002:  newarr     [mscorlib]System.Byte
    IL_0007:  stfld      uint8[] Thrift.Transport.TTransport::_peekBuffer
    IL_000c:  ldarg.0
    IL_000d:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0012:  ret
  } // end of method TTransport::.ctor

  .method public hidebysig instance bool 
          Peek() cil managed
  {
    // Code size       78 (0x4e)
    .maxstack  4
    .locals init (int32 V_0,
             bool V_1)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      bool Thrift.Transport.TTransport::_hasPeekByte
    IL_0006:  brfalse    IL_000d

    IL_000b:  ldc.i4.1
    IL_000c:  ret

    IL_000d:  ldarg.0
    IL_000e:  callvirt   instance bool Thrift.Transport.TTransport::get_IsOpen()
    IL_0013:  brtrue     IL_001a

    IL_0018:  ldc.i4.0
    IL_0019:  ret

    .try
    {
      IL_001a:  ldarg.0
      IL_001b:  ldarg.0
      IL_001c:  ldfld      uint8[] Thrift.Transport.TTransport::_peekBuffer
      IL_0021:  ldc.i4.0
      IL_0022:  ldc.i4.1
      IL_0023:  callvirt   instance int32 Thrift.Transport.TTransport::Read(uint8[],
                                                                            int32,
                                                                            int32)
      IL_0028:  stloc.0
      IL_0029:  ldloc.0
      IL_002a:  brtrue     IL_0036

      IL_002f:  ldc.i4.0
      IL_0030:  stloc.1
      IL_0031:  leave      IL_004c

      IL_0036:  leave      IL_0043

    }  // end .try
    catch [mscorlib]System.IO.IOException 
    {
      IL_003b:  pop
      IL_003c:  ldc.i4.0
      IL_003d:  stloc.1
      IL_003e:  leave      IL_004c

    }  // end handler
    IL_0043:  ldarg.0
    IL_0044:  ldc.i4.1
    IL_0045:  stfld      bool Thrift.Transport.TTransport::_hasPeekByte
    IL_004a:  ldc.i4.1
    IL_004b:  ret

    IL_004c:  ldloc.1
    IL_004d:  ret
  } // end of method TTransport::Peek

  .method public hidebysig newslot abstract virtual 
          instance void  Open() cil managed
  {
  } // end of method TTransport::Open

  .method public hidebysig newslot abstract virtual 
          instance void  Close() cil managed
  {
  } // end of method TTransport::Close

  .method public hidebysig newslot abstract virtual 
          instance int32  Read(uint8[] buf,
                               int32 'off',
                               int32 len) cil managed
  {
  } // end of method TTransport::Read

  .method public hidebysig instance int32 
          ReadAll(uint8[] buf,
                  int32 'off',
                  int32 len) cil managed
  {
    // Code size       88 (0x58)
    .maxstack  5
    .locals init (int32 V_0,
             int32 V_1)
    IL_0000:  ldc.i4.0
    IL_0001:  stloc.0
    IL_0002:  ldarg.0
    IL_0003:  ldfld      bool Thrift.Transport.TTransport::_hasPeekByte
    IL_0008:  brfalse    IL_0025

    IL_000d:  ldarg.1
    IL_000e:  ldarg.2
    IL_000f:  ldloc.0
    IL_0010:  dup
    IL_0011:  ldc.i4.1
    IL_0012:  add
    IL_0013:  stloc.0
    IL_0014:  add
    IL_0015:  ldarg.0
    IL_0016:  ldfld      uint8[] Thrift.Transport.TTransport::_peekBuffer
    IL_001b:  ldc.i4.0
    IL_001c:  ldelem.u1
    IL_001d:  stelem.i1
    IL_001e:  ldarg.0
    IL_001f:  ldc.i4.0
    IL_0020:  stfld      bool Thrift.Transport.TTransport::_hasPeekByte
    IL_0025:  br         IL_004f

    IL_002a:  ldarg.0
    IL_002b:  ldarg.1
    IL_002c:  ldarg.2
    IL_002d:  ldloc.0
    IL_002e:  add
    IL_002f:  ldarg.3
    IL_0030:  ldloc.0
    IL_0031:  sub
    IL_0032:  callvirt   instance int32 Thrift.Transport.TTransport::Read(uint8[],
                                                                          int32,
                                                                          int32)
    IL_0037:  stloc.1
    IL_0038:  ldloc.1
    IL_0039:  ldc.i4.0
    IL_003a:  bgt        IL_004b

    IL_003f:  ldc.i4.4
    IL_0040:  ldstr      "Cannot read, Remote side has closed"
    IL_0045:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_004a:  throw

    IL_004b:  ldloc.0
    IL_004c:  ldloc.1
    IL_004d:  add
    IL_004e:  stloc.0
    IL_004f:  ldloc.0
    IL_0050:  ldarg.3
    IL_0051:  blt        IL_002a

    IL_0056:  ldloc.0
    IL_0057:  ret
  } // end of method TTransport::ReadAll

  .method public hidebysig newslot virtual 
          instance void  Write(uint8[] buf) cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  ldc.i4.0
    IL_0003:  ldarg.1
    IL_0004:  ldlen
    IL_0005:  conv.i4
    IL_0006:  callvirt   instance void Thrift.Transport.TTransport::Write(uint8[],
                                                                          int32,
                                                                          int32)
    IL_000b:  ret
  } // end of method TTransport::Write

  .method public hidebysig newslot abstract virtual 
          instance void  Write(uint8[] buf,
                               int32 'off',
                               int32 len) cil managed
  {
  } // end of method TTransport::Write

  .method public hidebysig newslot virtual 
          instance void  Flush() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TTransport::Flush

  .method public hidebysig newslot virtual 
          instance class [mscorlib]System.IAsyncResult 
          BeginFlush(class [mscorlib]System.AsyncCallback callback,
                     object state) cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldc.i4.0
    IL_0001:  ldstr      "Asynchronous operations are not supported by this "
    + "transport."
    IL_0006:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_000b:  throw
  } // end of method TTransport::BeginFlush

  .method public hidebysig newslot virtual 
          instance void  EndFlush(class [mscorlib]System.IAsyncResult asyncResult) cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldc.i4.0
    IL_0001:  ldstr      "Asynchronous operations are not supported by this "
    + "transport."
    IL_0006:  newobj     instance void Thrift.Transport.TTransportException::.ctor(valuetype Thrift.Transport.TTransportException/ExceptionType,
                                                                                   string)
    IL_000b:  throw
  } // end of method TTransport::EndFlush

  .method family hidebysig newslot abstract virtual 
          instance void  Dispose(bool disposing) cil managed
  {
  } // end of method TTransport::Dispose

  .method public hidebysig newslot virtual final 
          instance void  Dispose() cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.1
    IL_0002:  callvirt   instance void Thrift.Transport.TTransport::Dispose(bool)
    IL_0007:  ldarg.0
    IL_0008:  call       void [mscorlib]System.GC::SuppressFinalize(object)
    IL_000d:  ret
  } // end of method TTransport::Dispose

  .property instance bool IsOpen()
  {
    .get instance bool Thrift.Transport.TTransport::get_IsOpen()
  } // end of property TTransport::IsOpen
} // end of class Thrift.Transport.TTransport

.class public auto ansi beforefieldinit Thrift.Transport.TTransportException
       extends Thrift.TException
{
  .class auto ansi sealed nested public ExceptionType
         extends [mscorlib]System.Enum
  {
    .field public specialname rtspecialname int32 value__
    .field public static literal valuetype Thrift.Transport.TTransportException/ExceptionType Unknown = int32(0x00000000)
    .field public static literal valuetype Thrift.Transport.TTransportException/ExceptionType NotOpen = int32(0x00000001)
    .field public static literal valuetype Thrift.Transport.TTransportException/ExceptionType AlreadyOpen = int32(0x00000002)
    .field public static literal valuetype Thrift.Transport.TTransportException/ExceptionType TimedOut = int32(0x00000003)
    .field public static literal valuetype Thrift.Transport.TTransportException/ExceptionType EndOfFile = int32(0x00000004)
    .field public static literal valuetype Thrift.Transport.TTransportException/ExceptionType Interrupted = int32(0x00000005)
  } // end of class ExceptionType

  .field family valuetype Thrift.Transport.TTransportException/ExceptionType 'type'
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.TException::.ctor()
    IL_0006:  ret
  } // end of method TTransportException::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype Thrift.Transport.TTransportException/ExceptionType 'type') cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TTransportException::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      valuetype Thrift.Transport.TTransportException/ExceptionType Thrift.Transport.TTransportException::'type'
    IL_000d:  ret
  } // end of method TTransportException::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(valuetype Thrift.Transport.TTransportException/ExceptionType 'type',
                               string message) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.2
    IL_0002:  call       instance void Thrift.TException::.ctor(string)
    IL_0007:  ldarg.0
    IL_0008:  ldarg.1
    IL_0009:  stfld      valuetype Thrift.Transport.TTransportException/ExceptionType Thrift.Transport.TTransportException::'type'
    IL_000e:  ret
  } // end of method TTransportException::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string message) cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  call       instance void Thrift.TException::.ctor(string)
    IL_0007:  ret
  } // end of method TTransportException::.ctor

  .method public hidebysig specialname instance valuetype Thrift.Transport.TTransportException/ExceptionType 
          get_Type() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      valuetype Thrift.Transport.TTransportException/ExceptionType Thrift.Transport.TTransportException::'type'
    IL_0006:  ret
  } // end of method TTransportException::get_Type

  .property instance valuetype Thrift.Transport.TTransportException/ExceptionType
          Type()
  {
    .get instance valuetype Thrift.Transport.TTransportException/ExceptionType Thrift.Transport.TTransportException::get_Type()
  } // end of property TTransportException::Type
} // end of class Thrift.Transport.TTransportException

.class public auto ansi beforefieldinit Thrift.Transport.TTransportFactory
       extends [mscorlib]System.Object
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  } // end of method TTransportFactory::.ctor

  .method public hidebysig newslot virtual 
          instance class Thrift.Transport.TTransport 
          GetTransport(class Thrift.Transport.TTransport trans) cil managed
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  ret
  } // end of method TTransportFactory::GetTransport

} // end of class Thrift.Transport.TTransportFactory

.class public auto ansi beforefieldinit Thrift.Transport.TMemoryBuffer
       extends Thrift.Transport.TTransport
{
  .field private initonly class [mscorlib]System.IO.MemoryStream byteStream
  .field private bool _IsDisposed
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  newobj     instance void [mscorlib]System.IO.MemoryStream::.ctor()
    IL_000c:  stfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TMemoryBuffer::byteStream
    IL_0011:  ret
  } // end of method TMemoryBuffer::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(uint8[] buf) cil managed
  {
    // Code size       19 (0x13)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Thrift.Transport.TTransport::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  newobj     instance void [mscorlib]System.IO.MemoryStream::.ctor(uint8[])
    IL_000d:  stfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TMemoryBuffer::byteStream
    IL_0012:  ret
  } // end of method TMemoryBuffer::.ctor

  .method public hidebysig specialname virtual 
          instance bool  get_IsOpen() cil managed
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldc.i4.1
    IL_0001:  ret
  } // end of method TMemoryBuffer::get_IsOpen

  .method public hidebysig virtual instance void 
          Open() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TMemoryBuffer::Open

  .method public hidebysig virtual instance void 
          Close() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method TMemoryBuffer::Close

  .method public hidebysig virtual instance int32 
          Read(uint8[] buf,
               int32 'off',
               int32 len) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TMemoryBuffer::byteStream
    IL_0006:  ldarg.1
    IL_0007:  ldarg.2
    IL_0008:  ldarg.3
    IL_0009:  callvirt   instance int32 [mscorlib]System.IO.Stream::Read(uint8[],
                                                                         int32,
                                                                         int32)
    IL_000e:  ret
  } // end of method TMemoryBuffer::Read

  .method public hidebysig virtual instance void 
          Write(uint8[] buf,
                int32 'off',
                int32 len) cil managed
  {
    // Code size       15 (0xf)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TMemoryBuffer::byteStream
    IL_0006:  ldarg.1
    IL_0007:  ldarg.2
    IL_0008:  ldarg.3
    IL_0009:  callvirt   instance void [mscorlib]System.IO.Stream::Write(uint8[],
                                                                         int32,
                                                                         int32)
    IL_000e:  ret
  } // end of method TMemoryBuffer::Write

  .method public hidebysig instance uint8[] 
          GetBuffer() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TMemoryBuffer::byteStream
    IL_0006:  callvirt   instance uint8[] [mscorlib]System.IO.MemoryStream::ToArray()
    IL_000b:  ret
  } // end of method TMemoryBuffer::GetBuffer

  .method public hidebysig static uint8[] 
          Serialize(class Thrift.Protocol.TAbstractBase s) cil managed
  {
    // Code size       27 (0x1b)
    .maxstack  2
    .locals init (class Thrift.Transport.TMemoryBuffer V_0,
             class Thrift.Protocol.TBinaryProtocol V_1)
    IL_0000:  newobj     instance void Thrift.Transport.TMemoryBuffer::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  newobj     instance void Thrift.Protocol.TBinaryProtocol::.ctor(class Thrift.Transport.TTransport)
    IL_000c:  stloc.1
    IL_000d:  ldarg.0
    IL_000e:  ldloc.1
    IL_000f:  callvirt   instance void Thrift.Protocol.TAbstractBase::Write(class Thrift.Protocol.TProtocol)
    IL_0014:  ldloc.0
    IL_0015:  callvirt   instance uint8[] Thrift.Transport.TMemoryBuffer::GetBuffer()
    IL_001a:  ret
  } // end of method TMemoryBuffer::Serialize

  .method public hidebysig static !!T  DeSerialize<(Thrift.Protocol.TAbstractBase) T>(uint8[] buf) cil managed
  {
    // Code size       146 (0x92)
    .maxstack  6
    .locals init (class Thrift.Transport.TMemoryBuffer V_0,
             class Thrift.Protocol.TBinaryProtocol V_1,
             class [mscorlib]System.Reflection.MethodInfo V_2,
             !!T V_3,
             class [mscorlib]System.Reflection.MethodInfo V_4)
    IL_0000:  ldarg.0
    IL_0001:  newobj     instance void Thrift.Transport.TMemoryBuffer::.ctor(uint8[])
    IL_0006:  stloc.0
    IL_0007:  ldloc.0
    IL_0008:  newobj     instance void Thrift.Protocol.TBinaryProtocol::.ctor(class Thrift.Transport.TTransport)
    IL_000d:  stloc.1
    IL_000e:  ldtoken    Thrift.Protocol.TBase
    IL_0013:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
    IL_0018:  ldtoken    !!T
    IL_001d:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
    IL_0022:  callvirt   instance bool [mscorlib]System.Type::IsAssignableFrom(class [mscorlib]System.Type)
    IL_0027:  brfalse    IL_0062

    IL_002c:  ldtoken    !!T
    IL_0031:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
    IL_0036:  ldstr      "Read"
    IL_003b:  ldc.i4.s   20
    IL_003d:  callvirt   instance class [mscorlib]System.Reflection.MethodInfo [mscorlib]System.Type::GetMethod(string,
                                                                                                                valuetype [mscorlib]System.Reflection.BindingFlags)
    IL_0042:  stloc.2
    IL_0043:  call       !!0 [mscorlib]System.Activator::CreateInstance<!!0>()
    IL_0048:  stloc.3
    IL_0049:  ldloc.2
    IL_004a:  ldloc.3
    IL_004b:  box        !!T
    IL_0050:  ldc.i4.1
    IL_0051:  newarr     [mscorlib]System.Object
    IL_0056:  dup
    IL_0057:  ldc.i4.0
    IL_0058:  ldloc.1
    IL_0059:  stelem.ref
    IL_005a:  callvirt   instance object [mscorlib]System.Reflection.MethodBase::Invoke(object,
                                                                                        object[])
    IL_005f:  pop
    IL_0060:  ldloc.3
    IL_0061:  ret

    IL_0062:  ldtoken    !!T
    IL_0067:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
    IL_006c:  ldstr      "Read"
    IL_0071:  ldc.i4.s   24
    IL_0073:  callvirt   instance class [mscorlib]System.Reflection.MethodInfo [mscorlib]System.Type::GetMethod(string,
                                                                                                                valuetype [mscorlib]System.Reflection.BindingFlags)
    IL_0078:  stloc.s    V_4
    IL_007a:  ldloc.s    V_4
    IL_007c:  ldnull
    IL_007d:  ldc.i4.1
    IL_007e:  newarr     [mscorlib]System.Object
    IL_0083:  dup
    IL_0084:  ldc.i4.0
    IL_0085:  ldloc.1
    IL_0086:  stelem.ref
    IL_0087:  callvirt   instance object [mscorlib]System.Reflection.MethodBase::Invoke(object,
                                                                                        object[])
    IL_008c:  unbox.any  !!T
    IL_0091:  ret
  } // end of method TMemoryBuffer::DeSerialize

  .method family hidebysig virtual instance void 
          Dispose(bool disposing) cil managed
  {
    // Code size       47 (0x2f)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      bool Thrift.Transport.TMemoryBuffer::_IsDisposed
    IL_0006:  brtrue     IL_0027

    IL_000b:  ldarg.1
    IL_000c:  brfalse    IL_0027

    IL_0011:  ldarg.0
    IL_0012:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TMemoryBuffer::byteStream
    IL_0017:  brfalse    IL_0027

    IL_001c:  ldarg.0
    IL_001d:  ldfld      class [mscorlib]System.IO.MemoryStream Thrift.Transport.TMemoryBuffer::byteStream
    IL_0022:  callvirt   instance void [mscorlib]System.IO.Stream::Dispose()
    IL_0027:  ldarg.0
    IL_0028:  ldc.i4.1
    IL_0029:  stfld      bool Thrift.Transport.TMemoryBuffer::_IsDisposed
    IL_002e:  ret
  } // end of method TMemoryBuffer::Dispose

  .property instance bool IsOpen()
  {
    .get instance bool Thrift.Transport.TMemoryBuffer::get_IsOpen()
  } // end of property TMemoryBuffer::IsOpen
} // end of class Thrift.Transport.TMemoryBuffer

.class private abstract auto ansi sealed beforefieldinit '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'
       extends [mscorlib]System.Object
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .class sequential ansi sealed nested private beforefieldinit '$ArrayType=1024'
         extends [mscorlib]System.ValueType
  {
    .pack 1
    .size 1024
  } // end of class '$ArrayType=1024'

  .class sequential ansi sealed nested private beforefieldinit '$ArrayType=4'
         extends [mscorlib]System.ValueType
  {
    .pack 1
    .size 4
  } // end of class '$ArrayType=4'

  .class sequential ansi sealed nested private beforefieldinit '$ArrayType=48'
         extends [mscorlib]System.ValueType
  {
    .pack 1
    .size 48
  } // end of class '$ArrayType=48'

  .class sequential ansi sealed nested private beforefieldinit '$ArrayType=8'
         extends [mscorlib]System.ValueType
  {
    .pack 1
    .size 8
  } // end of class '$ArrayType=8'

  .field static assembly valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=1024' '$field-0' at D_00012000
  .field static assembly valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '$field-1' at D_00012400
  .field static assembly valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=48' '$field-2' at D_00012408
  .field static assembly valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=8' '$field-3' at D_00012438
  .field static assembly valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '$field-4' at D_00012440
  .field static assembly valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '$field-5' at D_00012448
  .field static assembly valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '$field-6' at D_00012450
  .field static assembly valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '$field-7' at D_00012458
  .field static assembly valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '$field-8' at D_00012460
  .field static assembly valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '$field-9' at D_00012468
  .field static assembly valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '$field-A' at D_00012470
  .field static assembly valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '$field-B' at D_00012478
  .field static assembly valuetype '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'/'$ArrayType=4' '$field-C' at D_00012480
} // end of class '<PrivateImplementationDetails>{30e236ee-f36e-40a5-890e-ca4230b25402}'


// =============================================================

.data D_00012000 = bytearray (
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF 3E 00 00 00  // ............>...
                 FF FF FF FF FF FF FF FF FF FF FF FF 3F 00 00 00  // ............?...
                 34 00 00 00 35 00 00 00 36 00 00 00 37 00 00 00  // 4...5...6...7...
                 38 00 00 00 39 00 00 00 3A 00 00 00 3B 00 00 00  // 8...9...:...;...
                 3C 00 00 00 3D 00 00 00 FF FF FF FF FF FF FF FF  // <...=...........
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF 00 00 00 00 01 00 00 00 02 00 00 00
                 03 00 00 00 04 00 00 00 05 00 00 00 06 00 00 00
                 07 00 00 00 08 00 00 00 09 00 00 00 0A 00 00 00
                 0B 00 00 00 0C 00 00 00 0D 00 00 00 0E 00 00 00
                 0F 00 00 00 10 00 00 00 11 00 00 00 12 00 00 00
                 13 00 00 00 14 00 00 00 15 00 00 00 16 00 00 00
                 17 00 00 00 18 00 00 00 19 00 00 00 FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF 1A 00 00 00 1B 00 00 00 1C 00 00 00
                 1D 00 00 00 1E 00 00 00 1F 00 00 00 20 00 00 00  // ............ ...
                 21 00 00 00 22 00 00 00 23 00 00 00 24 00 00 00  // !..."...#...$...
                 25 00 00 00 26 00 00 00 27 00 00 00 28 00 00 00  // %...&...'...(...
                 29 00 00 00 2A 00 00 00 2B 00 00 00 2C 00 00 00  // )...*...+...,...
                 2D 00 00 00 2E 00 00 00 2F 00 00 00 30 00 00 00  // -......./...0...
                 31 00 00 00 32 00 00 00 33 00 00 00 FF FF FF FF  // 1...2...3.......
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
                 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF) 
.data D_00012400 = bytearray (
                 5C 75 30 30)                                     // \u00
.data D_00012404 = int8[4]
.data D_00012408 = bytearray (
                 00 00 00 00 00 00 00 00 62 74 6E 00 66 72 00 00  // ........btn.fr..
                 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
                 01 01 22 01 01 01 01 01 01 01 01 01 01 01 01 01) // ..".............
.data D_00012438 = bytearray (
                 22 5C 2F 08 0C 0A 0D 09)                         // "\/.....
.data D_00012440 = bytearray (
                 69 31 36 00)                                     // i16.
.data D_00012444 = int8[4]
.data D_00012448 = bytearray (
                 69 33 32 00)                                     // i32.
.data D_0001244C = int8[4]
.data D_00012450 = bytearray (
                 69 36 34 00)                                     // i64.
.data D_00012454 = int8[4]
.data D_00012458 = bytearray (
                 64 62 6C 00)                                     // dbl.
.data D_0001245C = int8[4]
.data D_00012460 = bytearray (
                 72 65 63 00)                                     // rec.
.data D_00012464 = int8[4]
.data D_00012468 = bytearray (
                 73 74 72 00)                                     // str.
.data D_0001246C = int8[4]
.data D_00012470 = bytearray (
                 6D 61 70 00)                                     // map.
.data D_00012474 = int8[4]
.data D_00012478 = bytearray (
                 6C 73 74 00)                                     // lst.
.data D_0001247C = int8[4]
.data D_00012480 = bytearray (
                 73 65 74 00)                                     // set.
// *********** DISASSEMBLY COMPLETE ***********************
// WARNING: Created Win32 resource file Thrift.res

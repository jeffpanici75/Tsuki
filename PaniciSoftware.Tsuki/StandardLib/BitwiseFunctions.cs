//
// Tsuki
//
// The MIT License (MIT)
// 
// Copyright (c) 2014 Jeff Panici
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.Linq;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.StandardLib
{
    public static class BitwiseFunctions
    {
        public static void ImportBitwise32Functions(this Table t)
        {
            LuaExportAttribute.AssignExportedFunctions(typeof (BitwiseFunctions), t);
        }

        [LuaExport(Name = "bit32.arshift")]
        internal static Int32 ArthemeticRightShift(Int32 x, Int32 disp)
        {
            return x >> disp;
        }

        [LuaExport(Name = "bit32.band")]
        internal static Int32 BinaryAnd(params Int32[] parms)
        {
            return parms.Aggregate(0, (current, each) => current & each);
        }

        [LuaExport(Name = "bit32.bnot")]
        internal static Int32 BinaryNot(Int32 x)
        {
            return ~x;
        }

        [LuaExport(Name = "bit32.bor")]
        internal static Int32 BinaryOr(params Int32[] parms)
        {
            return parms.Aggregate(0, (current, each) => current | each);
        }

        [LuaExport(Name = "bit32.btest")]
        internal static bool BinaryTest(params Int32[] parms)
        {
            return parms.Aggregate(0, (current, each) => current & each) != 0;
        }

        [LuaExport(Name = "bit32.bxor")]
        internal static Int32 BinaryExclusiveOr(params Int32[] parms)
        {
            return parms.Aggregate(0, (current, each) => current ^ each);
        }

        [LuaExport(Name = "bit32.extract")]
        internal static Int32 Extract(Int32 n, Int32 field, Int32 width = 1)
        {
            return 0;
        }

        [LuaExport(Name = "bit32.replace")]
        internal static Int32 Replace(Int32 n, Int32 v, Int32 width = 1)
        {
            return 0;
        }

        [LuaExport(Name = "bit32.lrotate")]
        internal static UInt32 LeftRotate(UInt32 x, Int32 disp)
        {
            return (x << disp) | (x >> (32 - disp));
        }

        [LuaExport(Name = "bit32.rrotate")]
        internal static UInt32 RightRotate(UInt32 x, Int32 disp)
        {
            return (x >> disp) | (x << (32 - disp));
        }

        [LuaExport(Name = "bit32.lshift")]
        internal static UInt32 LeftShift(UInt32 x, Int32 disp)
        {
            return x << disp;
        }

        [LuaExport(Name = "bit32.rshift")]
        internal static UInt32 RightShift(UInt32 x, Int32 disp)
        {
            return x >> disp;
        }
    }
}
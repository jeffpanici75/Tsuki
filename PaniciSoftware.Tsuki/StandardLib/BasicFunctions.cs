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
using System.Reflection;
using PaniciSoftware.Tsuki.Common;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.StandardLib
{
    public static class BasicFunctions
    {
        public static void ImportBasicFunctions(this Table t)
        {
            t["_G"] = t;
            t["_VERSION"] = "Lua 5.2";
            LuaExportAttribute.AssignExportedFunctions(typeof (BasicFunctions), t);
        }

        [LuaExport(Name = "int")]
        internal static int ToInt(object o)
        {
            return Convert.ToInt32(o);
        }

        [LuaExport(Name = "long")]
        internal static long ToLong(object o)
        {
            return Convert.ToInt64(o);
        }

        [LuaExport(Name = "double")]
        internal static double ToDouble(object o)
        {
            return Convert.ToDouble(o);
        }

        [LuaExport(Name = "float")]
        internal static float ToFloat(object o)
        {
            return Convert.ToSingle(o);
        }

        [LuaExport(Name = "print")]
        internal static void Print(object o)
        {
            Console.Out.Write(o);
        }

        [LuaExport(Name = "println")]
        internal static void PrintLn(object o)
        {
            Console.Out.WriteLine(o);
        }

        [LuaExport(Name = "assert")]
        internal static void Assert(
            object v,
            string message = "assertion failed!")
        {
            var flag = false;
            if (v != null)
            {
                try
                {
                    flag = Convert.ToBoolean(v);
                }
                catch (Exception)
                {
                    throw new LuaRuntimeException(message);
                }
            }
            if (!flag) throw new LuaRuntimeException(message);
        }

        [LuaExport(Name = "collectgarbage")]
        internal static object CollectGarbage(string opt)
        {
            if (string.IsNullOrWhiteSpace(opt)) return null;
            var lowerOpt = opt.ToLowerInvariant();
            switch (lowerOpt)
            {
                case "step":
                case "collect":
                    GC.Collect();
                    break;
                case "count":
                    var memoryUsed = GC.GetTotalMemory(false);
                    return ReturnList.New(memoryUsed, memoryUsed/1024);
                case "isrunning":
                    return false;
            }
            return null;
        }

        [LuaExport(Name = "dofile")]
        internal static object DoFile(string name = "")
        {
            return null;
        }

        [LuaExport(Name = "error")]
        internal static void Error(
            string message,
            int level = 1)
        {
            throw new LuaRuntimeException(message, level);
        }

        [LuaExport(Name = "ipairs")]
        internal static object IntegerPairs(Table t)
        {
            return ReturnList.New(
                typeof (BasicFunctions).GetMethod("NextIndex", BindingFlags.Static | BindingFlags.NonPublic),
                t,
                0M);
        }

        [LuaExport(Name = "load")]
        internal static object Load(
            object ld,
            object source = null,
            string mode = "bt",
            Table env = null)
        {
            return null;
        }

        [LuaExport(Name = "loadfile")]
        internal static object LoadFile()
        {
            return null;
        }

        [LuaExport(Name = "next")]
        internal static object Next(Table t, object index = null)
        {
            object k, v;
            return t.Next(index, out k, out v) ? ReturnList.New(k, v) : null;
        }

        [LuaExport(Name = "pairs")]
        internal static object Pairs(Table t)
        {
            return ReturnList.New(
                typeof (BasicFunctions).GetMethod("Next", BindingFlags.Static | BindingFlags.NonPublic),
                t,
                null);
        }

        [LuaExport(Name = "pcall")]
        internal static object ProtectedCall(
            object o,
            params object[] parms)
        {
            return null;
        }

        [LuaExport(Name = "xpcall")]
        internal static object ExtendedProtectedCall(
            object o,
            params object[] parms)
        {
            return null;
        }

        [LuaExport(Name = "rawequal")]
        internal static bool RawEqual(
            object v1,
            object v2)
        {
            return v1 == v2;
        }

        [LuaExport(Name = "rawget")]
        internal static object RawGet(
            Table t,
            object index)
        {
            object value;
            t.RawGetValue(index, out value);
            return value;
        }

        [LuaExport(Name = "rawlen")]
        internal static decimal RawLen(object o)
        {
            var s = o as string;
            if (!string.IsNullOrEmpty(s))
            {
                return s.Length;
            }
            var t = o as Table;
            return t != null ? t.SequenceLength : 0M;
        }

        [LuaExport(Name = "rawset")]
        internal static void RawSet(
            Table t,
            object index,
            object value)
        {
            t.RawSetValue(index, value);
        }

        [LuaExport(Name = "tonumber")]
        internal static object ToNumber(
            object s,
            object radix = null)
        {
            return NumericHelper.ToNumber(s, radix);
        }

        [LuaExport(Name = "tostring")]
        internal static string ToString(object o)
        {
            return o.ToString();
        }

        [LuaExport(Name = "type")]
        internal static string Type(object o)
        {
            if (o == null) return "nil";
            if (o is string) return "string";
            if (o is bool) return "boolean";
            if (o is Table) return "table";
            if (o is int || o is decimal || o is double) return "number";
            if (o is Thread) return "thread";
            if (o is UserData) return "userdata";
            if (TypeHelper.IsCallable(o)) return "function";

            return "native";
        }

// ReSharper disable UnusedMember.Local
        private static object NextIndex(Table t, decimal index = 0)
        {
// ReSharper restore UnusedMember.Local
            index++;
            object value;
            return t.RawGetValue(index, out value) ? ReturnList.New(index, value) : null;
        }
    }
}
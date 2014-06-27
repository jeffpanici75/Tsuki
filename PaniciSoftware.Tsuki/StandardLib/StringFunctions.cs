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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.StandardLib
{
    public static class StringFunctions
    {
        public static void ImportStringFunctions(this Table t)
        {
            LuaExportAttribute.AssignExportedFunctions(typeof (StringFunctions), t);
        }

        [LuaExport(Name = "string.byte")]
        internal static object GetByte(
            string s,
            int i = 1,
            int j = 1)
        {
            return ReturnList.New();
        }

        [LuaExport(Name = "string.char")]
        internal static string GetChars(params int[] codes)
        {
            if (codes == null) return string.Empty;

            var builder = new StringBuilder();
            foreach (var x in codes)
                builder.Append(Char.ConvertFromUtf32(x));

            return builder.ToString();
        }

        [LuaExport(Name = "string.dump")]
        internal static byte[] Dump(object func)
        {
            throw new NotImplementedException();
        }

        [LuaExport(Name = "string.find")]
        internal static object Find(
            string value,
            string pattern,
            int init = 1,
            bool plain = false)
        {
            return ReturnList.New();
        }

        [LuaExport(Name = "string.format")]
        internal static string Format(
            string format,
            params object[] parms)
        {
            return string.Empty;
        }

        [LuaExport(Name = "string.gmatch")]
        internal static object GlobalMatch(
            string value,
            string pattern)
        {
            return null;
        }

        [LuaExport(Name = "string.gsub")]
        internal static object GlobalSubstitution(
            string value,
            string pattern,
            string repl,
            int? n)
        {
            return null;
        }

        [LuaExport(Name = "string.len")]
        internal static int Length(string s)
        {
            // TODO: Metatable magick!
            return s.Length;
        }

        [LuaExport(Name = "string.lower")]
        internal static string Lower(string s)
        {
            return s.ToLower();
        }

        [LuaExport(Name = "string.upper")]
        internal static string Upper(string s)
        {
            return s.ToUpper();
        }

        [LuaExport(Name = "string.match")]
        internal static object Match(
            string s,
            string pattern,
            int init = 1)
        {
            return ReturnList.New();
        }

        [LuaExport(Name = "string.rep")]
        internal static string Repeat(
            string s,
            int n,
            string sep = "")
        {
            if (s == null || n == 0) return null;

            var builder = new StringBuilder();

            for (var i = 0; i < n; i++)
            {
                builder.Append(s);

                if (!string.IsNullOrEmpty(sep))
                    builder.Append(sep);
            }

            return builder.ToString();
        }

        [LuaExport(Name = "string.reverse")]
        internal static string Reverse(string s)
        {
            return s.ReverseGraphemeClusters();
        }

        [LuaExport(Name = "string.sub")]
        internal static string Substring(
            string s,
            int i = 1,
            int j = -1)
        {
            return string.Empty;
        }

        private static IEnumerable<string> GraphemeClusters(this string s)
        {
            var enumerator = StringInfo.GetTextElementEnumerator(s);

            while (enumerator.MoveNext())
                yield return (string)enumerator.Current;
        }

        private static string ReverseGraphemeClusters(this string s)
        {
            return string.Join(string.Empty, s.GraphemeClusters().Reverse().ToArray());
        }
    }
}
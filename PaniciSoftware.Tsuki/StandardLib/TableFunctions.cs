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

using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.StandardLib
{
    public static class TableFunctions
    {
        public static void ImportTableFunctions(this Table t)
        {
            var table = new Table();
            table["concat"] = null;
            table["insert"] = null;
            table["pack"] = null;
            table["remove"] = null;
            table["sort"] = null;
            table["unpack"] = null;
            t["table"] = table;
        }

        [LuaExport(Name = "table.concat")]
        internal static string Concat(
            Table list,
            string sep,
            int? i,
            int? j)
        {
            return string.Empty;
        }

        [LuaExport(Name = "table.insert")]
        internal static void Insert(
            Table list,
            int pos,
            object value)
        {
        }

        [LuaExport(Name = "table.pack")]
        internal static Table Pack(params object[] parms)
        {
            return new Table();
        }

        [LuaExport(Name = "table.remove")]
        internal static void Remove(
            Table list,
            int pos)
        {
        }

        [LuaExport(Name = "table.sort")]
        internal static void Sort(
            Table list,
            object func)
        {
        }

        [LuaExport(Name = "table.unpack")]
        internal static object Unpack(
            Table t,
            int i,
            int j)
        {
            return ReturnList.New();
        }
    }
}
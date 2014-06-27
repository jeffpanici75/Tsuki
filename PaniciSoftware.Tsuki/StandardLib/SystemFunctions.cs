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
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.StandardLib
{
    public static class SystemFunctions
    {
        public static void ImportSystemFunctions(this Table t)
        {
            LuaExportAttribute.AssignExportedFunctions(typeof (SystemFunctions), t);
        }

        [LuaExport(Name = "os.clock")]
        internal static object Clock()
        {
            return 0;
        }

        [LuaExport(Name = "os.date")]
        internal static object Date()
        {
            return null;
        }

        [LuaExport(Name = "os.difftime")]
        internal static int DiffTime(object t2, object t1)
        {
            return 0;
        }

        [LuaExport(Name = "os.execute")]
        internal static object Execute(string command)
        {
            return ReturnList.New();
        }

        [LuaExport(Name = "os.exit")]
        internal static object Exit(object code, bool close)
        {
            return ReturnList.New();
        }

        [LuaExport(Name = "os.getenv")]
        internal static object GetEnv(string name)
        {
            return System.Environment.GetEnvironmentVariable(name);
        }

        [LuaExport(Name = "os.remove")]
        internal static object Remove(string fileName)
        {
            return ReturnList.New();
        }

        [LuaExport(Name = "os.rename")]
        internal static object Rename(string oldFileName, string newFileName)
        {
            return ReturnList.New();
        }

        [LuaExport(Name = "os.setlocale")]
        internal static object SetLocale(string locale, string category)
        {
            return ReturnList.New();
        }

        [LuaExport(Name = "os.time")]
        internal static object Time(object table)
        {
            return ReturnList.New();
        }

        [LuaExport(Name = "os.tmpname")]
        internal static string TmpName()
        {
            return string.Empty;
        }
    }
}
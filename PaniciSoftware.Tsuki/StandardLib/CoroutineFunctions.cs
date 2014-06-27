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

using PaniciSoftware.Tsuki.Common;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.StandardLib
{
    public static class CoroutineFunctions
    {
        public static void ImportCoroutineFunctions(this Table t)
        {
            LuaExportAttribute.AssignExportedFunctions(typeof (CoroutineFunctions), t);
        }

        [LuaExport(Name = "coroutine.create")]
        internal static Thread Create(object func)
        {
            throw new LuaRuntimeException("Not implemented!");
        }

        [LuaExport(Name = "coroutine.resume")]
        internal static object Resume(
            Thread cr,
            params object[] parms)
        {
            return ReturnList.New(false);
        }

        [LuaExport(Name = "coroutine.running")]
        internal static object Running()
        {
            return ReturnList.New(null, false);
        }

        // running
        // suspended
        // normal
        // dead
        [LuaExport(Name = "coroutine.status")]
        internal static object Status(Thread cr)
        {
            return "dead";
        }

        [LuaExport(Name = "coroutine.wrap")]
        internal static object Wrap(object func)
        {
            return ReturnList.New();
        }

        [LuaExport(Name = "coroutine.yield")]
        internal static object Yield(params object[] parms)
        {
            return ReturnList.New();
        }
    }
}
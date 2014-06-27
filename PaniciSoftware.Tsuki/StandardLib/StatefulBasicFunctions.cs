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

using System.Runtime.Remoting.Channels;
using PaniciSoftware.Tsuki.Common;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.StandardLib
{
    public static class StatefulBasicFunctions
    {
        public static void ImportStatefulBasicFunctions(
            this Table t,
            LuaRuntime runtime,
            StaticMetaTables st)
        {
            LuaExportAttribute.AssignExportedFunctions(
                typeof (HiddenStatefulBasicFunctions),
                t,
                new HiddenStatefulBasicFunctions(runtime, st));
        }
    }

    public class HiddenStatefulBasicFunctions
    {
        private readonly StaticMetaTables _metaTables;

        private readonly LuaRuntime _runtime;

        public HiddenStatefulBasicFunctions(
            LuaRuntime runtime, 
            StaticMetaTables metaTables)
        {
            _runtime = runtime;
            _metaTables = metaTables;
        }

        [LuaExport(Name = "getfenv")]
        internal object GetEnvironment()
        {
            return _runtime.Environment;
        }

        [LuaExport(Name = "setfenv")]
        internal void SetEnvironment(object f, Table t)
        {
            if (f is int)
            {
                var funcIndex = (int) f;
                if (funcIndex == 0)
                    _runtime.Environment = t;
            }
        }

        [LuaExport(Name = "getmetatable")]
        internal Table GetMetaTable(object o)
        {
            var t = o as Table;
            if (t != null)
                return t.MetaTable;

            var u = o as UserData;
            if (u != null)
                return u.MetaTable;

            if (o is string)
                return _metaTables.String;

            if (NumericHelper.IsNumeric(o))
                return _metaTables.Numeric;

            if (o is bool)
                return _metaTables.Boolean;

            if (o is Thread)
                return _metaTables.Thread;

            if (TypeHelper.IsCallable(o))
                return _metaTables.Function;

            return null;
        }

        [LuaExport(Name = "setmetatable")]
        internal void SetMetaTable(
            object o,
            Table mt)
        {
            var t = o as Table;
            if (t != null)
            {
                t.MetaTable = new MetaTable(mt);
                return;
            }

            var ud = o as UserData;
            if (ud != null)
            {
                ud.MetaTable = new MetaTable(mt);
                return;
            }

            var thread = o as Thread;
            if (thread != null)
                thread.MetaTable = new MetaTable(mt);
        }
    }
}
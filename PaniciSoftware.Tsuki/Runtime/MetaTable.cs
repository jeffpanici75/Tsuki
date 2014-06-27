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
using System.Linq.Expressions;
using PaniciSoftware.Tsuki.Common;

namespace PaniciSoftware.Tsuki.Runtime
{
    public class MetaTable : Table
    {
        public MetaTable()
        {
        }

        public MetaTable(IEnumerable<KeyValuePair<object, object>> t)
        {
            foreach (var kv in t)
                Add(kv.Key, kv.Value);
        }

        public void SetAdd(Func<object, object, object> handler)
        {
            RawSetValue("__add", handler);
        }

        public void SetSub(Func<object, object, object> handler)
        {
            RawSetValue("__sub", handler);
        }

        public void SetMul(Func<object, object, object> handler)
        {
            RawSetValue("__mul", handler);
        }

        public void SetDiv(Func<object, object, object> handler)
        {
            RawSetValue("__div", handler);
        }

        public void SetMod(Func<object, object, object> handler)
        {
            RawSetValue("__mod", handler);
        }

        public void SetPow(Func<object, object, object> handler)
        {
            RawSetValue("__pow", handler);
        }

        public void SetConcat(Func<object, object, object> handler)
        {
            RawSetValue("__concat", handler);
        }

        public void SetLength(Func<object, object, string> handler)
        {
            RawSetValue("__len", handler);
        }

        public void SetEqual(Func<object, object, bool> handler)
        {
            RawSetValue("__eq", handler);
        }

        public void SetLessThan(Func<object, object, bool> handler)
        {
            RawSetValue("__lt", handler);
        }

        public void SetLessThanOrEqual(Func<object, object, bool> handler)
        {
            RawSetValue("__le", handler);
        }

        public void SetUnary(Func<object, object> handler)
        {
            RawSetValue("__unm", handler);
        }

        public void SetIndex(Func<object, object, object> handler)
        {
            RawSetValue("__index", handler);
        }

        public void SetNewIndex(Func<object, object, object> handler)
        {
            RawSetValue("__newindex", handler);
        }

        public void SetCall(Func<object, VarArgs, object> handler)
        {
            RawSetValue("__call", handler);
        }

        public static Table GetMetaTable(StaticMetaTables metaTables, object o)
        {
            if (o == null)
                return null;

            var table = o as Table;

            if (table != null)
                return table.MetaTable;

            if (metaTables == null)
                return null;

            if (o is string)
                return metaTables.String;

            if (o is int
                || o is double
                || o is long
                || o is Int16
                || o is decimal)
                return metaTables.Numeric;

            if (TypeHelper.IsCallable(o))
                return metaTables.Function;

            if (o is Thread)
                return metaTables.Thread;

            if (o is Boolean)
                return metaTables.Boolean;

            return null;
        }

        public static string GetHandlerNameFromExpressionType(ExpressionType t)
        {
            switch (t)
            {
                case ExpressionType.Add:
                    return "__add";
                case ExpressionType.Subtract:
                    return "__sub";
                case ExpressionType.Multiply:
                    return "__mul";
                case ExpressionType.Divide:
                    return "__div";
                case ExpressionType.Modulo:
                    return "__mod";
                case ExpressionType.Power:
                    return "__pow";
                case ExpressionType.Negate:
                    return "__unm";
                case ExpressionType.Equal:
                    return "__eq";
                case ExpressionType.LessThan:
                    return "__lt";
                case ExpressionType.LessThanOrEqual:
                    return "__le";
            }

            return null;
        }
    }
}
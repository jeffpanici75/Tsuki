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

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using PaniciSoftware.Tsuki.Common;

namespace PaniciSoftware.Tsuki.Runtime
{
    public enum RValueExpandMode
    {
        ExpandLast,
        NarrowIntermediate
    }

    public abstract class RValueList
    {
        protected RValueExpandMode Mode;

        public static Expression EmitNarrow(Expression exp)
        {
            var info = typeof (RValueList).GetMethod("Narrow", BindingFlags.Static | BindingFlags.NonPublic);
            return Expression.Call(info, RuntimeHelper.EnsureObjectResult(exp));
        }

        public static Expression EmitHandleFirst(Expression exp)
        {
            var info = typeof (RValueList).GetMethod("HandleFirst", BindingFlags.Static | BindingFlags.NonPublic);
            return Expression.Call(info, RuntimeHelper.EnsureObjectResult(exp));
        }

        public static Expression EmitHandleRest(Expression exp, int index)
        {
            var info = typeof (RValueList).GetMethod("HandleRest", BindingFlags.Static | BindingFlags.NonPublic);
            return Expression.Call(
                info,
                RuntimeHelper.EnsureObjectResult(exp),
                Expression.Constant(index, typeof (int)));
        }

        protected static object HandleFirst(object exp)
        {
            var rl = exp as IExpandable;
            return rl == null ? exp : rl.TryGetValue(0);
        }

        protected static object HandleRest(object exp, int index)
        {
            var rl = exp as IExpandable;
            return rl == null ? null : rl.TryGetValue(index);
        }

        protected static object Narrow(object exp)
        {
            var rl = exp as IExpandable;
            return rl == null ? exp : rl.TryGetValue(0);
        }

        protected static List<object> Expand(object exp)
        {
            var rl = exp as IExpandable;
            return rl == null ? new List<object> {exp} : rl.ToList();
        }
    }
}
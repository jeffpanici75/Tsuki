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

using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using PaniciSoftware.Tsuki.Common;

namespace PaniciSoftware.Tsuki.Runtime
{
    public class ConcatenationBinder : DynamicMetaObjectBinder
    {
        private readonly StaticMetaTables _metaTables;

        public ConcatenationBinder(StaticMetaTables metaTables)
        {
            _metaTables = metaTables;
        }

        public static ConcatenationBinder New(StaticMetaTables metaTables)
        {
            return new ConcatenationBinder(metaTables);
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            var rhs = args[0];

            if (!target.HasValue || !rhs.HasValue)
                return Defer(target, args);

            var rules = RuntimeHelper.DefaultTypeCheck(target, rhs);

            if ((NumericHelper.IsNumeric(target.LimitType) || target.LimitType == typeof (string))
                && (NumericHelper.IsNumeric(rhs.LimitType) || rhs.LimitType == typeof (string)))
            {
                var add = Expression.Add(
                    RuntimeHelper.EnsureObjectResult(Expression.Convert(target.Expression, target.LimitType)),
                    RuntimeHelper.EnsureObjectResult(Expression.Convert(rhs.Expression, rhs.LimitType)),
                    typeof (string).GetMethod(
                        "Concat",
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        new[]
                        {
                            typeof (object),
                            typeof (object)
                        },
                        null));

                return new DynamicMetaObject(
                    RuntimeHelper.EnsureObjectResult(add),
                    rules);
            }

            var h = Expression.Parameter(typeof (object));

            var metaCheckCond = Expression.Condition(
                RuntimeHelper.EmitGetBinHandler(
                    _metaTables,
                    target.Expression,
                    Expression.Constant(null, typeof (object)),
                    "__concat",
                    h),
                Expression.Dynamic(
                    InvokeBinder.New(_metaTables, new CallInfo(2)),
                    typeof (object),
                    h,
                    target.Expression,
                    Expression.Constant(null, typeof (object))),
                RuntimeHelper.EmitError(),
                typeof (object));

            var block = Expression.Block(
                typeof (object),
                new[]
                {
                    h
                },
                metaCheckCond);

            return new DynamicMetaObject(
                RuntimeHelper.EnsureObjectResult(block),
                rules);
        }
    }
}
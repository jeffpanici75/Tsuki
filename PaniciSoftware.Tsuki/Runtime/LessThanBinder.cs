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
    public class LessThanBinder : BinaryOperationBinder
    {
        private readonly MethodInfo _info;

        private readonly StaticMetaTables _metaTables;

        public LessThanBinder(StaticMetaTables metaTables) : base(ExpressionType.LessThan)
        {
            _metaTables = metaTables;
            _info = typeof (Compare).GetMethod("ApplyLessThan");
        }

        public static LessThanBinder New(StaticMetaTables st)
        {
            return new LessThanBinder(st);
        }

        public override DynamicMetaObject FallbackBinaryOperation(
            DynamicMetaObject target,
            DynamicMetaObject arg,
            DynamicMetaObject errorSuggestion)
        {
            var rules = RuntimeHelper.MatchTypeOrNull(target, target.LimitType)
                .Merge(RuntimeHelper.MatchTypeOrNull(arg, arg.LimitType));

            if ((NumericHelper.IsNumeric(target.LimitType) && NumericHelper.IsNumeric(arg.LimitType))
                || (target.LimitType == typeof (string) && arg.LimitType == typeof (string)))
            {
                return new DynamicMetaObject(
                    Expression.LessThan(
                        RuntimeHelper.EnsureObjectResult(target.Expression),
                        RuntimeHelper.EnsureObjectResult(arg.Expression),
                        false,
                        _info),
                    rules);
            }

            var h = Expression.Parameter(typeof (object));

            var cond = Expression.Condition(
                RuntimeHelper.EmitGetBinHandler(_metaTables, target.Expression, arg.Expression, "__lt", h),
                Expression.Dynamic(
                    InvokeBinder.New(_metaTables, new CallInfo(2)),
                    typeof (object),
                    h,
                    RuntimeHelper.EnsureObjectResult(target.Expression),
                    RuntimeHelper.EnsureObjectResult(arg.Expression)),
                RuntimeHelper.EmitError());

            return new DynamicMetaObject(
                Expression.Block(
                    typeof (object),
                    new[]
                    {
                        h
                    },
                    cond),
                rules);
        }
    }
}
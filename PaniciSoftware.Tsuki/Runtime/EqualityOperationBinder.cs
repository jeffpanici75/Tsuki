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

namespace PaniciSoftware.Tsuki.Runtime
{
    public class EqualityOperationBinder : BinaryOperationBinder
    {
        private readonly MethodInfo _info;

        private readonly StaticMetaTables _metaTables;

        public EqualityOperationBinder(StaticMetaTables metaTables) : base(ExpressionType.Equal)
        {
            _metaTables = metaTables;
            _info = typeof (Compare).GetMethod("ApplyEqualBool");
        }

        public static EqualityOperationBinder New(StaticMetaTables metaTables)
        {
            return new EqualityOperationBinder(metaTables);
        }

        public override DynamicMetaObject FallbackBinaryOperation(
            DynamicMetaObject target,
            DynamicMetaObject arg,
            DynamicMetaObject errorSuggestion)
        {
            var rules = RuntimeHelper.MatchTypeOrNull(target, target.LimitType)
                .Merge(RuntimeHelper.MatchTypeOrNull(arg, arg.LimitType));

            var h = Expression.Parameter(typeof (object));

            var cond = Expression.Convert(
                Expression.Condition(
                    Expression.Equal(target.Expression, arg.Expression, false, _info),
                    Expression.Constant(true, typeof (bool)),
                    Expression.Condition(
                        RuntimeHelper.EmitGetEqualHandler(
                            _metaTables,
                            RuntimeHelper.EnsureObjectResult(target.Expression),
                            RuntimeHelper.EnsureObjectResult(arg.Expression),
                            h),
                        RuntimeHelper.EmitToBool(
                            Expression.Dynamic(
                                InvokeBinder.New(_metaTables, new CallInfo(2)),
                                typeof (object),
                                h,
                                RuntimeHelper.EnsureObjectResult(target.Expression),
                                RuntimeHelper.EnsureObjectResult(arg.Expression))),
                        Expression.Constant(false, typeof (bool)))),
                typeof (object));

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
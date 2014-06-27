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
using PaniciSoftware.Tsuki.Common;

namespace PaniciSoftware.Tsuki.Runtime
{
    public class UnaryOperationBinder : System.Dynamic.UnaryOperationBinder
    {
        private readonly StaticMetaTables _metaTables;

        public UnaryOperationBinder(StaticMetaTables metaTables, ExpressionType operation) : base(operation)
        {
            _metaTables = metaTables;
        }

        public static UnaryOperationBinder New(StaticMetaTables metaTables, ExpressionType type)
        {
            return new UnaryOperationBinder(metaTables, type);
        }

        public override DynamicMetaObject FallbackUnaryOperation(
            DynamicMetaObject target,
            DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue)
                return Defer(target);

            var rules = target.Restrictions
                .Merge(
                    RuntimeHelper.MatchTypeOrNull(
                        target,
                        target.LimitType));

            var targetType = BinderHelper.GuessType(target.LimitType, target.Value);

            var o = Expression.Parameter(typeof (object));

            var targetToNumber = NumericHelper.EmitToNumber(target.Expression);

            var assignO = Expression.Assign(o, targetToNumber);

            var nullCheck = RuntimeHelper.NotNullCheck(RuntimeHelper.EnsureObjectResult(o));

            Expression applyUnary;
            if (NumericHelper.IsNumeric(targetType))
            {
                applyUnary = Expression.MakeUnary(
                    Operation,
                    Expression.Convert(
                        o,
                        target.LimitType),
                    typeof (object));
            }
            else
            {
                applyUnary = Expression.Constant(null, typeof (object));
            }

            var h = Expression.Parameter(typeof (object));

            var metaCheckCond = Expression.Condition(
                RuntimeHelper.EmitGetBinHandler(
                    _metaTables,
                    target.Expression,
                    Expression.Constant(null, typeof (object)),
                    Operation,
                    h),
                RValueList.EmitNarrow(
                    Expression.Dynamic(
                        InvokeBinder.New(_metaTables, new CallInfo(2)),
                        typeof (object),
                        h,
                        target.Expression,
                        Expression.Constant(null, typeof (object)))),
                RuntimeHelper.EmitError(),
                typeof (object));

            var topCond = Expression.Condition(
                nullCheck,
                RuntimeHelper.EnsureObjectResult(applyUnary),
                RuntimeHelper.EnsureObjectResult(metaCheckCond));

            var rootBlock = Expression.Block(
                typeof (object),
                new[]
                {
                    o,
                    h
                },
                assignO,
                topCond);

            return new DynamicMetaObject(rootBlock, rules);
        }
    }
}
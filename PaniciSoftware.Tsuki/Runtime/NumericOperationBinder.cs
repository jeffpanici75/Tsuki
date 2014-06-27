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
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using PaniciSoftware.Tsuki.Common;

namespace PaniciSoftware.Tsuki.Runtime
{
    public class NumericOperationBinder : BinaryOperationBinder
    {
        private readonly StaticMetaTables _metaTables;

        public NumericOperationBinder(StaticMetaTables metaTables, ExpressionType operation) : base(operation)
        {
            _metaTables = metaTables;
        }

        public static NumericOperationBinder New(StaticMetaTables metaTables, ExpressionType op)
        {
            return new NumericOperationBinder(metaTables, op);
        }

        public override DynamicMetaObject FallbackBinaryOperation(
            DynamicMetaObject target,
            DynamicMetaObject arg,
            DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue || !arg.HasValue)
                return Defer(target, arg);

            var rules = target.Restrictions.Merge(arg.Restrictions)
                .Merge(RuntimeHelper.MatchTypeOrNull(target, target.LimitType))
                .Merge(RuntimeHelper.MatchTypeOrNull(arg, arg.LimitType));

            var targetType = BinderHelper.GuessType(target.LimitType, target.Value);

            var argType = BinderHelper.GuessType(arg.LimitType, arg.Value);

            var winning = Operation == ExpressionType.Power
                ? typeof (double)
                : BinderHelper.FindWinningNumericType(targetType, argType);

            var o1 = Expression.Parameter(typeof (object));

            var o2 = Expression.Parameter(typeof (object));

            var targetToNumber = NumericHelper.EmitToNumber(target.Expression);

            var argToNumber = NumericHelper.EmitToNumber(arg.Expression);

            var assignO1 = Expression.Assign(o1, targetToNumber);

            var assignO2 = Expression.Assign(o2, argToNumber);

            var and = Expression.And(
                RuntimeHelper.NotNullCheck(RuntimeHelper.EnsureObjectResult(o1)),
                RuntimeHelper.NotNullCheck(RuntimeHelper.EnsureObjectResult(o2)));

            MethodInfo targetInfo;
            if (!BinderHelper.GetConverter(winning, targetType, out targetInfo))
            {
                return RuntimeHelper.CreateThrow(
                    target,
                    new[]
                    {
                        arg
                    },
                    rules,
                    typeof (InvalidOperationException),
                    string.Format("Unable to find converter for {0}", targetType));
            }

            MethodInfo argInfo;
            if (!BinderHelper.GetConverter(winning, argType, out argInfo))
            {
                return RuntimeHelper.CreateThrow(
                    target,
                    new[]
                    {
                        arg
                    },
                    rules,
                    typeof (InvalidOperationException),
                    string.Format("Unable to find converter for {0}", targetType));
            }

            var actualO1 = Expression.Convert(o1, targetType);

            var actualO2 = Expression.Convert(o2, argType);

            Expression applyOperation;
            if (winning == null)
                applyOperation = Expression.Constant(null, typeof (object));
            else
            {
                applyOperation = Expression.MakeBinary(
                    Operation,
                    Expression.Convert(actualO1, winning, targetInfo),
                    Expression.Convert(actualO2, winning, argInfo));
            }

            var h = Expression.Parameter(typeof (object));

            var metaCheckCond = Expression.Condition(
                RuntimeHelper.EmitGetBinHandler(
                    _metaTables,
                    target.Expression,
                    arg.Expression,
                    Operation,
                    h),
                RValueList.EmitNarrow(
                    Expression.Dynamic(
                        InvokeBinder.New(_metaTables, new CallInfo(2)),
                        typeof (object),
                        h,
                        target.Expression,
                        arg.Expression)),
                RuntimeHelper.EmitError(),
                typeof (object));

            var andCond = Expression.Condition(
                and,
                RuntimeHelper.EnsureObjectResult(applyOperation),
                RuntimeHelper.EnsureObjectResult(metaCheckCond),
                typeof (object));

            var rootBlock = Expression.Block(
                typeof (object),
                new[]
                {
                    o1,
                    o2,
                    h
                },
                assignO1,
                assignO2,
                andCond);

            return new DynamicMetaObject(rootBlock, rules);
        }
    }
}
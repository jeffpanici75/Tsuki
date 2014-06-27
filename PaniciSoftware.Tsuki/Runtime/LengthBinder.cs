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

namespace PaniciSoftware.Tsuki.Runtime
{
    public class LengthBinder : DynamicMetaObjectBinder
    {
        private readonly StaticMetaTables _metaTables;

        public LengthBinder(StaticMetaTables metaTables)
        {
            _metaTables = metaTables;
        }

        public static LengthBinder New(StaticMetaTables metaTables)
        {
            return new LengthBinder(metaTables);
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (!target.HasValue)
                return Defer(target, args);

            var rules = RuntimeHelper.MatchTypeOrNull(target, target.LimitType);

            if (target.LimitType == typeof (string))
            {
                return new DynamicMetaObject(
                    RuntimeHelper.EnsureObjectResult(
                        Expression.Property(
                            Expression.Convert(target.Expression, target.LimitType),
                            "Length")),
                    BindingRestrictions.GetInstanceRestriction(
                        target.Expression,
                        target.Value));
            }

            var h = Expression.Parameter(typeof (object));

            var cond = Expression.Condition(
                RuntimeHelper.EmitGetBinHandler(
                    _metaTables,
                    target.Expression,
                    Expression.Constant(null, typeof (object)),
                    "__len",
                    h),
                Expression.Dynamic(
                    InvokeBinder.New(_metaTables, new CallInfo(2)),
                    typeof (object),
                    h,
                    target.Expression),
                Expression.Condition(
                    Expression.TypeIs(target.Expression, typeof (Table)),
                    RuntimeHelper.EnsureObjectResult(
                        Expression.Property(
                            Expression.Convert(target.Expression, target.LimitType),
                            "SequenceLength")),
                    RuntimeHelper.EmitError()),
                typeof (object));

            var block = Expression.Block(
                typeof (object),
                new[]
                {
                    h
                },
                cond);

            return new DynamicMetaObject(block, rules);
        }
    }
}
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
    public class GetMemberBinder : System.Dynamic.GetMemberBinder
    {
        private readonly StaticMetaTables _metaTables;

        public GetMemberBinder(StaticMetaTables metaTables, string name) : base(name, false)
        {
            _metaTables = metaTables;
        }

        public static GetMemberBinder New(StaticMetaTables metaTables, string name)
        {
            return new GetMemberBinder(metaTables, name);
        }

        public override DynamicMetaObject FallbackGetMember(
            DynamicMetaObject target,
            DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue) return Defer(target);

            if (target.Value == null)
            {
                return errorSuggestion ??
                       RuntimeHelper.CreateThrow(
                           target,
                           null,
                           BindingRestrictions.GetExpressionRestriction(
                               Expression.Equal(
                                   target.Expression,
                                   Expression.Constant(null, typeof (object)))),
                           typeof (InvalidOperationException),
                           "Attempted to get member from a null value.");
            }

            if (NumericHelper.IsNumeric(target.LimitType)
                || target.LimitType == typeof (string)
                || target.LimitType == typeof (UserData)
                || target.LimitType == typeof (Thread))
            {
                var h = Expression.Parameter(typeof (object));

                var cond = Expression.Condition(
                    RuntimeHelper.EmitGetBinHandler(
                        _metaTables,
                        target.Expression,
                        Expression.Constant(null, typeof (object)),
                        "__index",
                        h),
                    Expression.Condition(
                        TypeHelper.EmitIsCallable(h),
                        RValueList.EmitNarrow(
                            Expression.Dynamic(
                                InvokeBinder.New(_metaTables, new CallInfo(2)),
                                typeof (object),
                                h,
                                target.Expression)),
                        Expression.Dynamic(
                            New(_metaTables, Name),
                            typeof (object),
                            h)),
                    RuntimeHelper.EmitError());

                var block = Expression.Block(
                    typeof (object),
                    new[]
                    {
                        h
                    },
                    cond);

                return new DynamicMetaObject(block, RuntimeHelper.MatchTypeOrNull(target, target.LimitType));
            }

            // Find our own binding.
            const BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;

            if (target.Value == null)
            {
                return errorSuggestion ??
                       RuntimeHelper.CreateThrow(
                           target,
                           null,
                           BindingRestrictions.GetExpressionRestriction(
                               Expression.Equal(
                                   target.Expression,
                                   Expression.Constant(null, typeof (object)))),
                           typeof (InvalidOperationException),
                           "Attempted to get member from a null value.");
            }

            var members = target.LimitType.GetMember(Name, flags);

            if (members.Length == 1)
            {
                // Don't need restriction test for name since this
                // rule is only used where binder is used, which is
                // only used in sites with this binder.Name.
                return new DynamicMetaObject(
                    Expression.MakeMemberAccess(
                        Expression.Convert(target.Expression, members[0].DeclaringType),
                        members[0]),
                    RuntimeHelper.MatchTypeOrNull(target, target.LimitType));
            }

            return errorSuggestion ??
                   RuntimeHelper.CreateThrow(
                       target,
                       null,
                       RuntimeHelper.MatchTypeOrNull(target, target.LimitType),
                       typeof (MissingMemberException),
                       "cannot bind member, " + Name +
                       ", on object " + target.Value);
        }
    }
}
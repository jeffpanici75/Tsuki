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
    public class SetMemberBinder : System.Dynamic.SetMemberBinder
    {
        private readonly StaticMetaTables _metaTables;

        public SetMemberBinder(StaticMetaTables metaTables, string name) : base(name, false)
        {
            _metaTables = metaTables;
        }

        public static SetMemberBinder New(StaticMetaTables metaTables, string name)
        {
            return new SetMemberBinder(metaTables, name);
        }

        public override DynamicMetaObject FallbackSetMember(
            DynamicMetaObject target,
            DynamicMetaObject value,
            DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue) return Defer(target);

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
                        "__newindex",
                        h),
                    Expression.Condition(
                        TypeHelper.EmitIsCallable(h),
                        RValueList.EmitNarrow(
                            Expression.Dynamic(
                                InvokeBinder.New(_metaTables, new CallInfo(2)),
                                typeof (object),
                                h,
                                target.Expression,
                                value.Expression)),
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
                return new DynamicMetaObject(
                    block,
                    BindingRestrictions.GetTypeRestriction(
                        target.Expression,
                        target.LimitType));
            }

            const BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;

            var members = target.LimitType.GetMember(Name, flags);

            if (members.Length == 1)
            {
                var mem = members[0];

                Expression val;

                // Should check for member domain type being Type and value being
                // TypeModel, similar to FitSuppliedArgsToRequiredArgs, and building an
                // expression like GetRuntimeTypeMoFromModel.

                if (mem.MemberType == MemberTypes.Property)
                    val = Expression.Convert(value.Expression, ((PropertyInfo) mem).PropertyType);
                else if (mem.MemberType == MemberTypes.Field)
                    val = Expression.Convert(value.Expression, ((FieldInfo) mem).FieldType);
                else
                    return (errorSuggestion ??
                            RuntimeHelper.CreateThrow(
                                target,
                                null,
                                RuntimeHelper.MatchTypeOrNull(target, target.LimitType),
                                typeof (InvalidOperationException),
                                "Tsuki only supports setting Properties and fields at this time."));

                return new DynamicMetaObject(
                    RuntimeHelper.EnsureObjectResult(
                        Expression.Assign(
                            Expression.MakeMemberAccess(
                                Expression.Convert(target.Expression, members[0].DeclaringType),
                                members[0]),
                            val)),
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
            }

            return errorSuggestion ??
                   RuntimeHelper.CreateThrow(
                       target,
                       null,
                       RuntimeHelper.MatchTypeOrNull(target, target.LimitType),
                       typeof (MissingMemberException),
                       "IDynObj member name conflict.");
        }
    }
}
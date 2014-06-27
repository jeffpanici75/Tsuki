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
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PaniciSoftware.Tsuki.Common;

namespace PaniciSoftware.Tsuki.Runtime
{
    public class InvokeBinder : System.Dynamic.InvokeBinder
    {
        private readonly StaticMetaTables _metaTables;

        public InvokeBinder(StaticMetaTables metaTables, CallInfo callInfo) : base(callInfo)
        {
            _metaTables = metaTables;
        }

        public static InvokeBinder New(StaticMetaTables metaTables, CallInfo info)
        {
            return new InvokeBinder(metaTables, info);
        }

        public override DynamicMetaObject FallbackInvoke(
            DynamicMetaObject target,
            DynamicMetaObject[] args,
            DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue || args.Any(a => !a.HasValue))
            {
                var deferArgs = new DynamicMetaObject[args.Length + 1];

                for (var i = 0; i < args.Length; i++)
                    deferArgs[i + 1] = args[i];

                deferArgs[0] = target;

                return Defer(deferArgs);
            }

            if (target.Value == null)
            {
                return errorSuggestion ??
                       RuntimeHelper.CreateThrow(
                           target,
                           args,
                           BindingRestrictions.GetExpressionRestriction(
                               Expression.Equal(
                                   target.Expression,
                                   Expression.Constant(null, typeof (object)))),
                           typeof (InvalidOperationException),
                           "Attempted to invoke null value.");
            }

            ParameterInfo[] ps;
            Func<Expression[], Expression> makeInvoke;

            if (target.LimitType == typeof (NativeFunction))
            {
                var f = (NativeFunction) target.Value;

                var info = f.Info;

                if (f.Instance == null && !info.IsStatic)
                {
                    return errorSuggestion ??
                           RuntimeHelper.CreateThrow(
                               target,
                               args,
                               BindingRestrictions.GetExpressionRestriction(
                                   Expression.Equal(
                                       target.Expression,
                                       Expression.Constant(null, typeof (object)))),
                               typeof (InvalidOperationException),
                               "Attempted to invoke non static method via a NativeFunction without an instance provided.");
                }

                ps = info.GetParameters();

                if (info.IsStatic)
                {
                    makeInvoke = a => Expression.Call(info, a);
                }
                else
                {
                    makeInvoke = a => Expression.Call(
                        Expression.Constant(
                            f.Instance,
                            info.DeclaringType ?? typeof (object)),
                        info,
                        a);
                }
            }
            else if (target.LimitType == typeof (MethodInfo) || target.LimitType.IsSubclassOf(typeof (MethodInfo)))
            {
                var info = (MethodInfo) target.Value;

                if (!info.IsStatic)
                {
                    return errorSuggestion ??
                           RuntimeHelper.CreateThrow(
                               target,
                               args,
                               BindingRestrictions.GetExpressionRestriction(
                                   Expression.Equal(
                                       target.Expression,
                                       Expression.Constant(null, typeof (object)))),
                               typeof (InvalidOperationException),
                               "Attempted to invoke non static method via a NativeFunction without an instance provided.");
                }

                ps = info.GetParameters();

                makeInvoke = a => Expression.Call(info, a);
            }
            else if (target.LimitType.IsSubclassOf(typeof (Delegate)))
            {
                ps = target.LimitType.GetMethod("Invoke").GetParameters();

                makeInvoke = a => Expression.Invoke(
                    Expression.Convert(target.Expression, target.LimitType),
                    a);
            }
            else
            {
                var h = Expression.Parameter(typeof (object));

                var invokeArgs = new List<Expression> {h, target.Expression};

                invokeArgs.AddRange(args.Select(i => i.Expression));

                var cond = Expression.Condition(
                    RuntimeHelper.EmitGetBinHandler(
                        _metaTables,
                        target.Expression,
                        Expression.Constant(null, typeof (object)),
                        "__call",
                        h),
                    Expression.Dynamic(
                        New(_metaTables, new CallInfo(args.Length + 2)),
                        typeof (object),
                        invokeArgs.ToArray()),
                    RuntimeHelper.EmitError());

                return new DynamicMetaObject(
                    Expression.Block(
                        typeof (object),
                        new[]
                        {
                            h
                        },
                        cond),
                    BindingRestrictions.GetTypeRestriction(
                        target.Expression,
                        target.LimitType));
            }

            if (ps.Length < 1)
            {
                //no expected params including var args so just eval any args that did come in and discard them
                var argExpressions = args.Select(a => a.Expression).ToList();

                argExpressions.Add(RuntimeHelper.EnsureObjectResult(makeInvoke(new Expression[0])));

                return new DynamicMetaObject(
                    Expression.Block(typeof (object), argExpressions),
                    BindingRestrictions.GetTypeRestriction(
                        target.Expression,
                        target.LimitType));
            }

            var callArgs = new Expression[ps.Length];

            var hasVarArg = ps.Length > 0 && ps[ps.Length - 1].ParameterType == typeof (VarArgs);

            var scratch = Expression.Parameter(typeof (object));

            var rValues = new RuntimeRValueList(args, scratch);

            var setupArgs = new List<ParameterExpression> {scratch};

            Expression invokeExpression;

            if (hasVarArg)
            {
                var oneToOneCount = ps.Length - 1;

                var runOff = new List<Expression>();

                for (var i = 0; i < rValues.Count; i++)
                {
                    var e = rValues.Next();
                    if (i >= oneToOneCount)
                    {
                        runOff.Add(e);
                    }
                    else
                    {
                        callArgs[i] = Expression.Convert(e, ps[i].ParameterType);
                    }
                }

                callArgs[callArgs.Length - 1] = FillArray(runOff);

                invokeExpression = Expression.Block(
                    typeof (object),
                    setupArgs,
                    RuntimeHelper.EnsureObjectResult(makeInvoke(callArgs)));
            }
            else
            {
                var initBlock = new List<Expression>();

                for (var i = 0; i < ps.Length; i++)
                {
                    var tmp = Expression.Parameter(typeof (object));

                    setupArgs.Add(tmp);

                    Expression n;

                    if (rValues.TryNext(out n))
                    {
                        var init = Expression.Assign(tmp, n);

                        initBlock.Add(init);

                        callArgs[i] = Expression.Convert(tmp, ps[i].ParameterType);
                    }
                    else if (ps[i].DefaultValue != DBNull.Value)
                    {
                        callArgs[i] = Expression.Constant(ps[i].DefaultValue, ps[i].ParameterType);
                    }
                    else
                    {
                        callArgs[i] = Expression.Default(ps[i].ParameterType);
                    }
                }

                var rest = rValues.EvalRestAndDiscard();

                invokeExpression = Expression.Block(
                    typeof (object),
                    setupArgs,
                    Expression.Block(initBlock),
                    rest,
                    RuntimeHelper.EnsureObjectResult(makeInvoke(callArgs)));
            }

            return new DynamicMetaObject(
                invokeExpression,
                BindingRestrictions.GetTypeRestriction(
                    target.Expression,
                    target.LimitType));
        }

        private static Expression FillArray(List<Expression> items)
        {
            if (items == null || !items.Any())
                return Expression.Constant(null, typeof (VarArgs));

            var info = typeof (VarArgs).GetConstructor(new Type[0]);
            if (info == null)
                return Expression.Empty();

            var table = Expression.Parameter(typeof (VarArgs));
            var expressions = new List<Expression>
            {
                Expression.Assign(table, Expression.New(info))
            };

            var addInfo = typeof (VarArgs).GetMethod(
                "Add",
                new[]
                {
                    typeof (object),
                    typeof (object)
                });

            var arrayIndex = 1M;

            foreach (var value in items)
            {
                var add = Expression.Call(
                    table,
                    addInfo,
                    Expression.Convert(Expression.Constant(arrayIndex), typeof (object)),
                    RuntimeHelper.EnsureObjectResult(value));

                expressions.Add(add);

                arrayIndex++;
            }

            expressions.Add(table);

            return Expression.Block(
                typeof (VarArgs),
                new[]
                {
                    table
                },
                expressions);
        }
    }
}
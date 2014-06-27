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

namespace PaniciSoftware.Tsuki.Runtime
{
    public static class RuntimeHelper
    {
        public static Expression EmitToBool(Expression exp)
        {
            var scratch = Expression.Parameter(typeof (object), "ToBoolScratch");

            return Expression.Block(
                typeof (bool),
                new[]
                {
                    scratch
                },
                new Expression[]
                {
                    Expression.Assign(scratch, Expression.Convert(exp, typeof (object))),
                    Expression.Condition(
                        Expression.TypeIs(scratch, typeof (bool)),
                        Expression.Convert(scratch, typeof (bool)),
                        Expression.Convert(
                            Expression.NotEqual(
                                scratch,
                                Expression.Constant(
                                    null,
                                    typeof (object))),
                            typeof (bool)))
                });
        }

        public static Expression EmitError()
        {
            return Expression.Constant(null, typeof (object));
        }

        public static object Error()
        {
            return null;
        }

        public static Expression EmitGetEqualHandler(
            StaticMetaTables metaTables, 
            Expression op1, 
            Expression op2, 
            ParameterExpression outParam)
        {
            var info = typeof (RuntimeHelper).GetMethod("GetEqualHandler", BindingFlags.Static | BindingFlags.Public);

            return Expression.Call(
                info,
                Expression.Constant(metaTables, typeof (StaticMetaTables)),
                op1,
                op2,
                outParam);
        }

        public static Type LuaType(object o)
        {
            return o == null ? null : o.GetType();
        }

        public static bool GetEqualHandler(
            StaticMetaTables metaTables, 
            object op1, 
            object op2, 
            out object handler)
        {
            if (op1 == null && op2 == null)
            {
                handler = null;
                return false;
            }

            if (LuaType(op1) != LuaType(op2) || (!(op1 is Table) && !(op1 is UserData)))
            {
                handler = null;
                return false;
            }

            var mm1 = MetaTable.GetMetaTable(metaTables, op1);

            var mm2 = MetaTable.GetMetaTable(metaTables, op2);

            if (mm1 == mm2)
            {
                handler = mm1;
                return true;
            }

            handler = null;

            return false;
        }

        public static Expression EmitGetBinHandler(
            StaticMetaTables metaTables,
            Expression op1,
            Expression op2,
            ExpressionType t,
            ParameterExpression outParam)
        {
            var info = typeof (RuntimeHelper).GetMethod("GetBinHandlerET", BindingFlags.Static | BindingFlags.Public);

            return Expression.Call(
                info,
                Expression.Constant(metaTables, typeof (StaticMetaTables)),
                op1,
                op2,
                Expression.Constant(t, typeof (ExpressionType)),
                outParam);
        }

        public static Expression EmitGetBinHandler(
            StaticMetaTables metaTables,
            Expression op1,
            Expression op2,
            string t,
            ParameterExpression outParam)
        {
            var info = typeof (RuntimeHelper).GetMethod("GetBinHandler", BindingFlags.Static | BindingFlags.Public);

            return Expression.Call(
                info,
                Expression.Constant(metaTables, typeof (StaticMetaTables)),
                op1,
                op2,
                Expression.Constant(t, typeof (string)),
                outParam);
        }

        public static bool GetBinHandlerET(
            StaticMetaTables metaTables,
            object op1,
            object op2,
            ExpressionType t,
            out object handler)
        {
            return GetBinHandler(metaTables, op1, op2, MetaTable.GetHandlerNameFromExpressionType(t), out handler);
        }

        public static bool GetBinHandler(
            StaticMetaTables metaTables,
            object op1,
            object op2,
            string t,
            out object handler)
        {
            var mt = MetaTable.GetMetaTable(metaTables, op1) ?? MetaTable.GetMetaTable(metaTables, op2);
            if (mt == null)
            {
                handler = null;
                return false;
            }

            if (t == null)
            {
                handler = null;
                return false;
            }

            return mt.RawGetValue(t, out handler);
        }

        public static Expression NullCheck(Expression e)
        {
            return Expression.Equal(e, Expression.Constant(null, typeof (object)));
        }

        public static Expression NotNullCheck(Expression e)
        {
            return Expression.Not(Expression.Equal(e, Expression.Constant(null, typeof (object))));
        }

        public static BindingRestrictions DefaultTypeCheck(DynamicMetaObject lhs, DynamicMetaObject rhs)
        {
            return MatchTypeOrNull(lhs, lhs.LimitType).Merge(MatchTypeOrNull(rhs, rhs.LimitType));
        }

        public static BindingRestrictions MatchTypeOrNull(DynamicMetaObject o, Type t)
        {
            if (o.HasValue
                && o.LimitType == typeof (object)
                && o.Value == null)
            {
                return BindingRestrictions.GetExpressionRestriction(
                    Expression.Equal(
                        o.Expression,
                        Expression.Constant(null, typeof (object))));
            }

            return BindingRestrictions.GetExpressionRestriction(Expression.TypeIs(o.Expression, t));
        }

        public static BindingRestrictions GetTargetArgsRestrictions(
            DynamicMetaObject target,
            DynamicMetaObject[] args,
            bool instanceRestrictionOnTarget)
        {
            // Important to add existing restriction first because the
            // DynamicMetaObjects (and possibly values) we're looking at depend
            // on the pre-existing restrictions holding true.
            var restrictions = target.Restrictions.Merge(BindingRestrictions.Combine(args));

            if (instanceRestrictionOnTarget)
            {
                restrictions = restrictions.Merge(
                    BindingRestrictions.GetInstanceRestriction(
                        target.Expression,
                        target.Value));
            }
            else
            {
                restrictions = restrictions.Merge(
                    BindingRestrictions.GetTypeRestriction(
                        target.Expression,
                        target.LimitType));
            }

            for (var i = 0; i < args.Length; i++)
            {
                BindingRestrictions r;

                if (args[i].HasValue && args[i].Value == null)
                {
                    r = BindingRestrictions.GetInstanceRestriction(
                        args[i].Expression,
                        null);
                }
                else
                {
                    r = BindingRestrictions.GetTypeRestriction(
                        args[i].Expression,
                        args[i].LimitType);
                }

                restrictions = restrictions.Merge(r);
            }

            return restrictions;
        }

        public static bool ParametersMatchArguments(
            ParameterInfo[] parameters,
            DynamicMetaObject[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var paramType = parameters[i].ParameterType;

                if (!paramType.IsAssignableFrom(args[i].LimitType))
                    return false;
            }

            return true;
        }

        public static Expression EnsureObjectResult(Expression e)
        {
            if (!e.Type.IsValueType)
                return e;

            if (e.Type == typeof (void))
                return Expression.Block(e, Expression.Constant(null, typeof (object)));

            return Expression.Convert(e, typeof (object));
        }

        public static DynamicMetaObject CreateThrow(
            DynamicMetaObject target,
            DynamicMetaObject[] args,
            BindingRestrictions moreTests,
            Type exception,
            params object[] exceptionArgs)
        {
            Expression[] argExprs = null;

            var argTypes = Type.EmptyTypes;

            if (exceptionArgs != null)
            {
                var i = exceptionArgs.Length;

                argExprs = new Expression[i];

                argTypes = new Type[i];

                i = 0;

                foreach (var o in exceptionArgs)
                {
                    Expression e = Expression.Constant(o);

                    argExprs[i] = e;

                    argTypes[i] = e.Type;

                    i += 1;
                }
            }

            var constructor = exception.GetConstructor(argTypes);
            if (constructor == null)
                throw new ArgumentException("Type doesn't have constructor with a given signature");

            // Force expression to be type object so that DLR CallSite
            // code things only type object flows out of the CallSite.
            return new DynamicMetaObject(
                Expression.Throw(
                    Expression.New(constructor, argExprs),
                    typeof (object)),
                moreTests);
        }
    }
}
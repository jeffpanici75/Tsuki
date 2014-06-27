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
using PaniciSoftware.Tsuki.Common;

namespace PaniciSoftware.Tsuki.Runtime
{
    public class TableMetaObject : DynamicMetaObject
    {
        private StaticMetaTables _metaTables;

        public TableMetaObject(
            Expression expression,
            BindingRestrictions restrictions) : base(expression, restrictions)
        {
        }

        public TableMetaObject(
            Expression expression,
            BindingRestrictions restrictions,
            Table value) : base(expression, restrictions, value)
        {
        }

        public Table Table
        {
            get { return (Table) Value; }
        }

        public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
        {
            return base.BindBinaryOperation(binder, arg);
        }

        public override DynamicMetaObject BindConvert(System.Dynamic.ConvertBinder binder)
        {
            return base.BindConvert(binder);
        }

        public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
        {
            return base.BindCreateInstance(binder, args);
        }

        public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return base.BindDeleteIndex(binder, indexes);
        }

        public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
        {
            return base.BindDeleteMember(binder);
        }

        public override DynamicMetaObject BindGetIndex(System.Dynamic.GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return BindGetOrInvokeMember(
                indexes[0].Expression,
                null);
        }


        public override DynamicMetaObject BindGetMember(System.Dynamic.GetMemberBinder binder)
        {
            return BindGetOrInvokeMember(
                Expression.Constant(binder.Name, typeof (string)),
                null);
        }

        public override DynamicMetaObject BindInvoke(System.Dynamic.InvokeBinder binder, DynamicMetaObject[] args)
        {
            return base.BindInvoke(binder, args);
        }

        public override DynamicMetaObject BindInvokeMember(System.Dynamic.InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            Func<DynamicMetaObject, DynamicMetaObject> invokeStep = value =>
            {
                var fullArgs = new List<DynamicMetaObject>
                {
                    new DynamicMetaObject(Expression.Constant(Table), BindingRestrictions.Empty)
                };
                fullArgs.AddRange(args);
                return binder.FallbackInvoke(value, fullArgs.ToArray(), null);
            };

            return BindGetOrInvokeMember(
                Expression.Constant(binder.Name, typeof (string)),
                invokeStep);
        }

        public override DynamicMetaObject BindSetIndex(System.Dynamic.SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            var key = RuntimeHelper.EnsureObjectResult(indexes[0].Expression);
            var v = RuntimeHelper.EnsureObjectResult(value.Expression);
            return InternalBindSetMember(key, v);
        }

        public override DynamicMetaObject BindSetMember(System.Dynamic.SetMemberBinder binder, DynamicMetaObject value)
        {
            var name = binder.Name;
            var nameExp = Expression.Constant(name, typeof (string));
            var v = RuntimeHelper.EnsureObjectResult(value.Expression);
            return InternalBindSetMember(nameExp, v);
        }

        public override DynamicMetaObject BindUnaryOperation(System.Dynamic.UnaryOperationBinder binder)
        {
            return base.BindUnaryOperation(binder);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Table.Keys.Select(key => key.ToString()).ToList();
        }

        private Expression EmitRawGetValue(Expression table, Expression key, out ParameterExpression outP)
        {
            outP = Expression.Parameter(typeof (object));

            var info = typeof (Table).GetMethod("RawGetValue");

            Expression rawGetValue = Expression.Call(
                table,
                info,
                RuntimeHelper.EnsureObjectResult(key),
                RuntimeHelper.EnsureObjectResult(outP));

            return rawGetValue;
        }

        private DynamicMetaObject InternalBindSetMember(Expression key, Expression value)
        {
            var table = Table;

            var tableInstance = Expression.Constant(table, typeof (Table));

            ParameterExpression outP;
            var rawGetValue = EmitRawGetValue(tableInstance, key, out outP);

            var info = typeof (Table).GetMethod(
                "set_Item",
                new[]
                {
                    typeof (object),
                    typeof (object)
                });

            var call = Expression.Call(
                tableInstance,
                info,
                RuntimeHelper.EnsureObjectResult(key),
                RuntimeHelper.EnsureObjectResult(value));

            var block = Expression.Block(typeof (object), call, value);

            var h = Expression.Parameter(typeof (object));

            var tryMetaTable = Expression.Condition(
                RuntimeHelper.EmitGetBinHandler(
                    null,
                    tableInstance,
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
                            tableInstance,
                            key)),
                    Expression.Dynamic(
                        GetIndexBinder.New(_metaTables, new CallInfo(1)),
                        typeof (object),
                        h,
                        key)),
                block);

            var cond = Expression.Condition(rawGetValue, tryMetaTable, block);

            var condBlock = Expression.Block(
                typeof (object),
                new[]
                {
                    h,
                    outP
                },
                cond);

            return new DynamicMetaObject(
                condBlock,
                BindingRestrictions.GetExpressionRestriction(
                    Expression.TypeIs(tableInstance, typeof (Table))));
        }

        private DynamicMetaObject BindGetOrInvokeMember(
            Expression key,
            Func<DynamicMetaObject, DynamicMetaObject> fallbackInvoke)
        {
            var table = Table;

            var tableInstance = Expression.Constant(table, typeof (Table));

            ParameterExpression outP;
            var rawGetValue = EmitRawGetValue(tableInstance, key, out outP);

            var result = new DynamicMetaObject(outP, BindingRestrictions.Empty);
            if (fallbackInvoke != null)
                result = fallbackInvoke(result);

            var h = Expression.Parameter(typeof (object));

            var tryMetaTable = Expression.Condition(
                RuntimeHelper.EmitGetBinHandler(
                    null,
                    tableInstance,
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
                            tableInstance,
                            key)),
                    Expression.Dynamic(
                        GetIndexBinder.New(_metaTables, new CallInfo(1)),
                        typeof (object),
                        h,
                        key)),
                Expression.Constant(null, typeof (object)));

            return new DynamicMetaObject(
                Expression.Block(
                    new[]
                    {
                        outP,
                        h
                    },
                    Expression.Condition(
                        rawGetValue,
                        result.Expression,
                        tryMetaTable,
                        typeof (object))),
                result.Restrictions.Merge(
                    BindingRestrictions.GetExpressionRestriction(
                        Expression.TypeIs(tableInstance, typeof (Table)))));
        }
    }
}
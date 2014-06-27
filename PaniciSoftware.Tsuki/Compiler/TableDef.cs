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
using System.Linq.Expressions;
using System.Reflection;
using Antlr.Runtime.Tree;
using PaniciSoftware.Tsuki.Common;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.Compiler
{
    public class TableDef : Generator
    {
        protected override Expression OnGenerate()
        {
            var hasFields = Tree.Children != null && Tree.Children.Count > 0;

            var info = typeof (Table).GetConstructor(new Type[0]);
            if (info == null)
            {
                Errors.CompileError("Table consttructor not found.");
                return Expression.Empty();
            }

            if (!hasFields)
                return Expression.New(info);

            var table = Expression.Parameter(typeof (Table));

            var expressions = new List<Expression>
            {
                Expression.Assign(table, Expression.New(info))
            };

            var addInfo = typeof (Table).GetMethod(
                "Add",
                new[]
                {
                    typeof (object),
                    typeof (object)
                });

            var arrayIndex = 1M;

            for (var i = 0; i < Tree.Children.Count; i++)
            {
                var field = (CommonTree) Tree.Children[i];

                switch (field.Type)
                {
                    case ChunkParser.NamedKVField:
                    {
                        var name = field.Children[0].Text;

                        var expTree = field.Children[1];

                        var exp = RValueList.EmitNarrow(Gen<Exp>(expTree));

                        var add = Expression.Call(
                            table,
                            addInfo,
                            Expression.Constant(
                                name,
                                typeof (string)),
                            RuntimeHelper.EnsureObjectResult(exp));

                        expressions.Add(add);

                        break;
                    }
                    case ChunkParser.KVField:
                    {
                        var keyTree = field.Children[0];

                        var valueTree = field.Children[1];

                        var key = RValueList.EmitNarrow(Gen<Exp>(keyTree));

                        var value = RValueList.EmitNarrow(Gen<Exp>(valueTree));

                        var add = Expression.Call(
                            table,
                            addInfo,
                            RuntimeHelper.EnsureObjectResult(key),
                            RuntimeHelper.EnsureObjectResult(value));

                        expressions.Add(add);

                        break;
                    }
                    case ChunkParser.ArrayField:
                    {
                        var valueTree = field.Children[0];

                        var value = Gen<Exp>(valueTree);

                        if (i == Tree.Children.Count - 1)
                        {
                            var tryFillInfo = typeof (TableDef).GetMethod(
                                "TryFillFromExpandable", 
                                BindingFlags.NonPublic | BindingFlags.Static);

                            expressions.Add(
                                Expression.Call(
                                    tryFillInfo,
                                    table,
                                    Expression.Constant(arrayIndex, typeof (decimal)),
                                    RuntimeHelper.EnsureObjectResult(value)));
                        }
                        else
                        {
                            var add = Expression.Call(
                                table,
                                addInfo,
                                Expression.Convert(Expression.Constant(arrayIndex, typeof (decimal)), typeof (object)),
                                RuntimeHelper.EnsureObjectResult(RValueList.EmitNarrow(value)));

                            expressions.Add(add);

                            arrayIndex++;
                        }

                        break;
                    }
                }
            }

            expressions.Add(table);

            return Expression.Block(
                typeof (Table),
                new[]
                {
                    table
                },
                expressions);
        }

        private static void TryFillFromExpandable(Table t, decimal index, object o)
        {
            var rl = o as IExpandable;

            if (rl == null)
                t.Add(index, o);
            else
            {
                var list = rl.ToList();

                foreach (var i in list)
                {
                    t.Add(index, i);
                    index++;
                }
            }
        }
    }
}
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
using Antlr.Runtime.Tree;
using PaniciSoftware.Tsuki.Common;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.Compiler
{
    public class Statement : Generator
    {
        protected override Expression OnGenerate()
        {
            switch (Tree.Type)
            {
                case ChunkLexer.SemiColon:
                {
                    return Expression.Empty();
                }
                case ChunkParser.Assign:
                {
                    return Gen<Assign>();
                }
                case ChunkParser.Defun:
                {
                    return Gen<Defun>();
                }
                case ChunkParser.LocalVar:
                {
                    return Gen<LocalVar>();
                }
                case ChunkParser.Scope:
                {
                    return Gen<DoBlock>();
                }
                case ChunkParser.While:
                {
                    return Gen<While>();
                }
                case ChunkParser.Repeat:
                {
                    return Gen<Repeat>();
                }
                case ChunkParser.Range:
                {
                    return Gen<Range>();
                }
                case ChunkParser.Iter:
                {
                    return Gen<Iter>();
                }
                case ChunkParser.If:
                {
                    return Gen<If>();
                }
                case ChunkParser.Standalone:
                case ChunkParser.NestedStandalone:
                {
                    return Gen<FunctionCall>();
                }
                case ChunkParser.Return:
                {
                    LabelTarget returnLabel;

                    if (Scope.TryFindNearestFunctionReturn(out returnLabel))
                    {
                        Expression returnValue;

                        var hasReturnValues = Tree.Children != null && Tree.Children.Count > 0;

                        if (hasReturnValues)
                        {
                            if (Tree.Children.Count == 1)
                            {
                                returnValue = RuntimeHelper.EnsureObjectResult(Gen<Exp>(Tree.Children[0]));
                            }
                            else
                            {
                                var info = typeof (ReturnList).GetConstructor(new Type[0]);
                                if (info == null)
                                {
                                    Errors.CompileError("ReturnList ctor not found.");
                                    return Expression.Empty();
                                }

                                var rList = Expression.Parameter(typeof (ReturnList));
                                var expressions = new List<Expression>
                                {
                                    Expression.Assign(rList, Expression.New(info))
                                };

                                var addInfo = typeof (ReturnList).GetMethod(
                                    "PushBack",
                                    new[]
                                    {
                                        typeof (object)
                                    });

                                foreach (CommonTree rVal in Tree.Children)
                                {
                                    var exp = Gen<Exp>(rVal);
                                    var add = Expression.Call(
                                        rList,
                                        addInfo,
                                        new[]
                                        {
                                            RuntimeHelper.EnsureObjectResult(exp)
                                        });
                                    expressions.Add(add);
                                }

                                expressions.Add(rList);

                                returnValue = Expression.Block(
                                    typeof (ReturnList),
                                    new[]
                                    {
                                        rList
                                    },
                                    expressions);
                            }
                        }
                        else
                        {
                            returnValue = Expression.Constant(null, typeof (object));
                        }

                        return Expression.Goto(returnLabel, returnValue);
                    }

                    Errors.InvalidJumpStatement(Tree.Token.Line, Tree.Token.StartIndex);

                    return Expression.Empty();
                }
                case ChunkParser.Break:
                {
                    LabelTarget breakLabel;

                    if (Scope.TryFindNearestBreak(out breakLabel))
                        return Expression.Break(breakLabel);

                    Errors.InvalidJumpStatement(Tree.Token.Line, Tree.Token.StartIndex);

                    return Expression.Empty();
                }
                case ChunkParser.Continue:
                {
                    LabelTarget continueLabel;

                    if (Scope.TryFindNearestConintue(out continueLabel))
                        return Expression.Continue(continueLabel);

                    Errors.InvalidJumpStatement(Tree.Token.Line, Tree.Token.StartIndex);

                    return Expression.Empty();
                }
            }

            throw new InvalidOperationException(string.Format("Unknown Statement type: {0}", Tree.Type));
        }
    }
}
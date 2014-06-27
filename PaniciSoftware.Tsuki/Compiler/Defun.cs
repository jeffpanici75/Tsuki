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
    public class Defun : Generator
    {
        protected override Expression OnGenerate()
        {
            Scope = Scope.NewFunctionScope();

            var block = (CommonTree) Tree.Children[0];
            var name = (CommonTree) Tree.Children[1];
            var hasParams = Tree.Children.Count > 2;

            var impliedSelf = name.Type == ChunkParser.ImpliedSelfFuncName;

            CommonTree list = null;
            if (hasParams)
                list = (CommonTree) Tree.Children[2];

            List<ParameterExpression> parameters;
            var functionType = ExpressionHelper.PreProcessFunction(list, out parameters, impliedSelf);
            foreach (var p in parameters)
                Scope.Locals[p.Name] = p;

            var blockExp = Gen<Block>(block);

            var returnFrame = Expression.Block(
                typeof (object),
                blockExp,
                Expression.Label(
                    Scope.ReturnTarget,
                    Expression.Constant(null, typeof (object))));

            var func = Expression.Lambda(functionType, returnFrame, true, parameters);

            var lvalue = new LValue();
            Expression origin;
            Errors.AddRange(lvalue.Generate(StaticTables, Scope, name.Children[0], out origin));
            switch (lvalue.SuffixType)
            {
                case SuffixType.Property:
                {
                    return
                        Expression.Dynamic(
                            SetMemberBinder.New(StaticTables, lvalue.Name),
                            typeof (void),
                            origin,
                            func);
                }
                case SuffixType.Root:
                {
                    return Expression.Dynamic(
                        SetMemberBinder.New(StaticTables, lvalue.Name),
                        typeof (void),
                        Scope.Env.GlobalParameter,
                        //global scope thing here.
                        func);
                }
                default:
                {
                    throw new InvalidOperationException(string.Format("Invalid lvalue type:{0}", lvalue.SuffixType));
                }
            }
        }
    }
}
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

using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Antlr.Runtime.Tree;
using PaniciSoftware.Tsuki.Runtime;
using SetIndexBinder = PaniciSoftware.Tsuki.Runtime.SetIndexBinder;
using SetMemberBinder = PaniciSoftware.Tsuki.Runtime.SetMemberBinder;

namespace PaniciSoftware.Tsuki.Compiler
{
    public class Assign : Generator
    {
        protected override Expression OnGenerate()
        {
            var lhs = (CommonTree) Tree.Children[0];
            var targetCount = lhs.Children.Count;
            var assignments = new List<Expression>();
            var scratch = Expression.Parameter(typeof (object));
            var rValues = new CompileTimeRValueList(Tree.Children[1], tree => Gen<Exp>(tree), scratch);

            for (var i = 0; i < targetCount; i++)
            {
                if (CheckError(lhs.Children[i]))
                {
                    assignments.Add(Expression.Empty());
                    continue;
                }

                var rvalue = rValues.Next();
                var lvalueGen = new LValue();

                Expression lvalue;
                lvalueGen.Generate(StaticTables, Scope, lhs.Children[i], out lvalue);

                switch (lvalueGen.SuffixType)
                {
                    case SuffixType.Index:
                    {
                        assignments.Add(
                            Expression.Dynamic(
                                SetIndexBinder.New(new CallInfo(1)),
                                typeof (void),
                                lvalue,
                                lvalueGen.Index,
                                rvalue));
                        break;
                    }
                    case SuffixType.Property:
                    {
                        assignments.Add(
                            Expression.Dynamic(
                                SetMemberBinder.New(StaticTables, lvalueGen.Name),
                                typeof (void),
                                lvalue,
                                rvalue));
                        break;
                    }
                    case SuffixType.Root:
                    {
                        assignments.Add(
                            Expression.Dynamic(
                                SetMemberBinder.New(StaticTables, lvalueGen.Name),
                                typeof (void),
                                Scope.Env.GlobalParameter,
                                //global scope thing here.
                                rvalue));
                        break;
                    }
                    case SuffixType.Local:
                    {
                        assignments.Add(Expression.Assign(lvalue, rvalue));
                        break;
                    }
                }
            }
            assignments.Add(rValues.EvalRestAndDiscard());
            return Expression.Block(
                typeof (void),
                new[]
                {
                    scratch
                },
                assignments);
        }
    }
}
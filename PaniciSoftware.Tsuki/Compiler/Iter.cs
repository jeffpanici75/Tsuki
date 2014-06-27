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
using InvokeBinder = PaniciSoftware.Tsuki.Runtime.InvokeBinder;

namespace PaniciSoftware.Tsuki.Compiler
{
    public class Iter : Generator
    {
        protected override Expression OnGenerate()
        {
            Scope = Scope.NewForEachScope();

            var f = Expression.Parameter(typeof (object));
            var s = Expression.Parameter(typeof (object));
            var seedVar = Expression.Parameter(typeof (object));

            var topScratch = Expression.Parameter(typeof (object));
            var rValues = new CompileTimeRValueList(Tree.Children[2], tree => Gen<Exp>(tree), topScratch);

            var assignF = Expression.Assign(f, rValues.Next());
            var assignS = Expression.Assign(s, rValues.Next());
            var initSeed = Expression.Assign(seedVar, rValues.Next());

            var fResult = Expression.Parameter(typeof (object));
            var invoke = Expression.Dynamic(InvokeBinder.New(StaticTables, new CallInfo(2)), typeof (object), f, s, seedVar);
            var assignFResult = Expression.Assign(fResult, invoke);

            var assignLocal = new List<Expression>();
            var parameters = new List<ParameterExpression> {fResult};
            var names = (CommonTree) Tree.Children[1];

            for (var i = 0; i < names.Children.Count; i++)
            {
                var name = names.Children[i].Text;
                var rValue = i == 0 ? RValueList.EmitHandleFirst(fResult) : RValueList.EmitHandleRest(fResult, i);
                ParameterExpression varN;
                assignLocal.Add(GenerateDefineLocal(name, rValue, out varN));
                parameters.Add(varN);
            }

            var checkNull = Expression.Dynamic(
                EqualityOperationBinder.New(StaticTables),
                typeof (object),
                parameters[1],
                Expression.Constant(null, typeof (object)));

            var checkCondition = Expression.IfThen(ToBool(checkNull), Expression.Goto(Scope.BreakTarget));

            var assignSeed = Expression.Assign(seedVar, parameters[1]);

            var blockTree = Tree.Children[0];
            var block = Gen<Block>(blockTree);

            var loopBody = Expression.Block(
                typeof (void),
                parameters,
                assignFResult,
                Expression.Block(typeof (void), assignLocal),
                checkCondition,
                assignSeed,
                block);

            var loop = Expression.Loop(loopBody, Scope.BreakTarget, Scope.ContinueTarget);

            var enclosingBlock = Expression.Block(
                typeof (void),
                new[]
                {
                    f,
                    s,
                    seedVar,
                    topScratch
                },
                assignF,
                assignS,
                initSeed,
                loop);

            return enclosingBlock;
        }
    }
}
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

using System.Linq.Expressions;
using Antlr.Runtime.Tree;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.Compiler
{
    public class Range : Generator
    {
        protected override Expression OnGenerate()
        {
            if (CheckError(Tree.Children[1]))
                return Expression.Empty();

            var name = Tree.Children[1].Text;

            var initTree = (CommonTree) Tree.Children[2];

            var limitTree = (CommonTree) Tree.Children[3];

            var blockTree = (CommonTree) Tree.Children[0];

            var hasStep = Tree.Children.Count > 4;

            Scope = Scope.NewRangeScope();

            var var = Expression.Parameter(typeof (object));
            var limit = Expression.Parameter(typeof (object));
            var step = Expression.Parameter(typeof (object));

            var assignVar = Expression.Assign(var, RuntimeHelper.EnsureObjectResult(Gen<Exp>(initTree)));
            var assignLimit = Expression.Assign(limit, RuntimeHelper.EnsureObjectResult(Gen<Exp>(limitTree)));
            Expression assignStep;

            if (hasStep)
            {
                var stepTree = (CommonTree) Tree.Children[4];
                assignStep = Expression.Assign(step, RuntimeHelper.EnsureObjectResult(Gen<Exp>(stepTree)));
            }
            else
            {
                assignStep = Expression.Assign(step, RuntimeHelper.EnsureObjectResult(Expression.Constant(1, typeof (int))));
            }

            //do null check here

            var aboveZero = Expression.Dynamic(
                LessThanOrEqualBinder.New(StaticTables),
                typeof (object),
                Expression.Constant(0, typeof (int)),
                step);

            var varLessThanEqualLimit = Expression.Dynamic(
                LessThanOrEqualBinder.New(StaticTables),
                typeof (object),
                var,
                limit);

            var and1 = Expression.And(ToBool(aboveZero), ToBool(varLessThanEqualLimit));

            var belowOrZero = Expression.Dynamic(
                LessThanOrEqualBinder.New(StaticTables),
                typeof (object),
                step,
                Expression.Constant(0, typeof (int)));

            var varGreaterThanOrEqualLimit = Expression.Dynamic(
                LessThanOrEqualBinder.New(StaticTables),
                typeof (object),
                limit,
                var);

            var and2 = Expression.And(ToBool(belowOrZero), ToBool(varGreaterThanOrEqualLimit));

            var or = Expression.Or(and1, and2);

            var cond = Expression.IfThen(Expression.Not(or), Expression.Break(Scope.BreakTarget));

            var v = Expression.Parameter(typeof (object), name);

            Scope.Locals[name] = v;

            var assignV = Expression.Assign(v, var);

            var block = Gen<Block>(blockTree);

            var updateVar = Expression.Dynamic(
                NumericOperationBinder.New(
                    StaticTables,
                    ExpressionType.Add),
                typeof (object),
                var,
                step);

            var assignUpdate = Expression.Assign(var, updateVar);

            var innerBlock = Expression.Block(
                typeof (void),
                new[]
                {
                    v
                },
                cond,
                assignV,
                block,
                Expression.Label(Scope.ContinueTarget),
                assignUpdate);

            var loop = Expression.Loop(innerBlock, Scope.BreakTarget);

            var outBlock = Expression.Block(
                typeof (void),
                new[]
                {
                    var,
                    limit,
                    step
                },
                assignVar,
                assignLimit,
                assignStep,
                loop);

            return outBlock;
        }
    }
}
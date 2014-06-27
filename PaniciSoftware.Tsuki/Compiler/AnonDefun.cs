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
using System.Linq.Expressions;
using Antlr.Runtime.Tree;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.Compiler
{
    public class AnonDefun : Generator
    {
        protected override Expression OnGenerate()
        {
            Scope = Scope.NewFunctionScope();

            var hasParams = Tree.Children.Count > 1;

            CommonTree list = null;
            if (hasParams)
                list = (CommonTree) Tree.Children[1];

            List<ParameterExpression> parameters;
            var type = ExpressionHelper.PreProcessFunction(list, out parameters);
            parameters.ForEach(p => Scope.Locals[p.Name] = p);

            var blockTree = (CommonTree) Tree.Children[0];
            var blockExp = Gen<Block>(blockTree);

            var returnFrame = Expression.Block(
                typeof (object),
                blockExp,
                Expression.Label(
                    Scope.ReturnTarget,
                    Expression.Constant(null, typeof (object))));

            return Expression.Lambda(type, returnFrame, true, parameters);
        }
    }
}
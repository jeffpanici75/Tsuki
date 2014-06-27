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
using PaniciSoftware.Tsuki.Common;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.Compiler
{
    public class If : Generator
    {
        protected override Expression OnGenerate()
        {
            var ifCondTree = Tree.Children[0];
            var ifBlockTree = Tree.Children[1];

            var ifCond = ToBool(Wrap(Gen<Exp>(ifCondTree)));
            var ifBlock = Gen<Block>(ifBlockTree);

            var hasElse = Tree.Children.Count > 2 && Tree.Children[2].Type == ChunkParser.Else;
            var elseIfStart = hasElse ? 3 : 2;
            var elseBranch = hasElse ? Gen<Block>(((CommonTree) Tree.Children[2]).Children[0]) : Expression.Empty();
            var prev = elseBranch;

            for (var i = Tree.Children.Count - 1; i >= elseIfStart; i--)
            {
                var eiNode = (CommonTree) Tree.Children[i];
                var eiCondTree = eiNode.Children[0];
                var eiBlockTree = eiNode.Children[1];
                var cond = ToBool(Wrap(Gen<Exp>(eiCondTree)));
                var block = Expression.Condition(
                    cond,
                    Gen<Block>(eiBlockTree),
                    prev);
                prev = block;
            }

            return Expression.Condition(ifCond, ifBlock, prev);
        }

        protected static Expression Wrap(Expression exp)
        {
            return RValueList.EmitNarrow(exp);
        }
    }
}
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

namespace PaniciSoftware.Tsuki.Runtime
{
    public class CompileTimeRValueList : RValueList
    {
        private readonly Func<ITree, Expression> _makeExp;
        private readonly ParameterExpression _scratch;
        private readonly CommonTree _tree;
        private int _expandIndex = -1;
        private int _current = -1;

        public CompileTimeRValueList(ITree rValues, Func<ITree, Expression> makeExpression, ParameterExpression scratch)
        {
            _tree = (CommonTree) rValues;
            _makeExp = makeExpression;
            _scratch = scratch;
            Mode = RValueExpandMode.NarrowIntermediate;
        }

        public Expression Next()
        {
            switch (Mode)
            {
                case RValueExpandMode.ExpandLast:
                    return ExpandLast();
                case RValueExpandMode.NarrowIntermediate:
                    return NarrowIntermediate();
            }

            throw new InvalidOperationException("Invalid state");
        }

        public Expression EvalRestAndDiscard()
        {
            if (Mode == RValueExpandMode.ExpandLast)
                return Expression.Empty();

            _current++;

            var list = new List<Expression>();

            for (var i = _current; i < _tree.Children.Count; i++)
                list.Add(_makeExp(_tree.Children[i]));

            return Expression.Block(list);
        }

        private Expression NarrowIntermediate()
        {
            _current++;

            if (_current >= (_tree.Children.Count - 1))
            {
                Mode = RValueExpandMode.ExpandLast;

                var last = _makeExp(_tree.Children[_current]);

                var assignBlock = Expression.Block(
                    typeof (object),
                    Expression.Assign(
                        _scratch,
                        RuntimeHelper.EnsureObjectResult(last)),
                    ExpandLast());

                return assignBlock;
            }

            return EmitNarrow(_makeExp(_tree.Children[_current]));
        }

        private Expression ExpandLast()
        {
            _expandIndex++;

            return _expandIndex == 0
                ? EmitHandleFirst(_scratch)
                : EmitHandleRest(_scratch, _expandIndex);
        }
    }
}
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
    public class RuntimeRValueList : RValueList
    {
        private readonly List<DynamicMetaObject> _expressions;
        private readonly ParameterExpression _scratch;
        private int _expandIndex = -1;
        private int _current = -1;

        public RuntimeRValueList(
            DynamicMetaObject[] objs,
            ParameterExpression scratch)
        {
            _expressions = objs.Length < 1 ? new List<DynamicMetaObject>() : new List<DynamicMetaObject>(objs);

            _scratch = scratch;

            Mode = RValueExpandMode.NarrowIntermediate;

            if (_expressions.Count < 1)
                Count = 0;
            else if (_expressions.Last().LimitType.IsSubclassOf(typeof (IExpandable)))
                Count = ((IExpandable) _expressions.Last()).Count + (_expressions.Count - 1);
            else
                Count = _expressions.Count;
        }

        public int Count { get; private set; }

        public bool TryNext(out Expression e)
        {
            switch (Mode)
            {
                case RValueExpandMode.ExpandLast:
                {
                    e = ExpandLast();
                    return true;
                }
                case RValueExpandMode.NarrowIntermediate:
                {
                    return NarrowIntermediate(out e);
                }
            }

            throw new InvalidOperationException("Invalid state");
        }

        public Expression Next()
        {
            Expression e;
            TryNext(out e);
            return e;
        }

        public Expression EvalRestAndDiscard()
        {
            if (Mode == RValueExpandMode.ExpandLast)
                return Expression.Empty();

            _current++;

            var l = new List<Expression>();

            for (var i = _current; i < _expressions.Count; i++)
                l.Add(_expressions[i].Expression);

            return Expression.Block(l);
        }

        private bool NarrowIntermediate(out Expression o)
        {
            if (_expressions.Count < 1)
            {
                o = Expression.Constant(null, typeof (object));
                return false;
            }

            _current++;

            if (_current >= (_expressions.Count - 1))
            {
                Mode = RValueExpandMode.ExpandLast;

                var last = _expressions.Last();

                var assignBlock = Expression.Block(
                    typeof (object),
                    Expression.Assign(_scratch, RuntimeHelper.EnsureObjectResult(last.Expression)),
                    ExpandLast());

                o = assignBlock;

                return true;
            }

            var current = _expressions[_current];
            if (current.Value == null)
            {
                o = Expression.Constant(null, typeof (object));
                return false;
            }

            var isExpandable = current.LimitType.IsSubclassOf(typeof (IExpandable));
            if (isExpandable)
            {
                o = EmitNarrow(current.Expression);
                return true;
            }

            o = current.Expression;

            return true;
        }

        private Expression ExpandLast()
        {
            _expandIndex++;

            return _expandIndex == 0 ? EmitHandleFirst(_scratch) : EmitHandleRest(_scratch, _expandIndex);
        }
    }
}
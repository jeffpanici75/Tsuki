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

namespace PaniciSoftware.Tsuki.Compiler
{
    public enum ScopeType
    {
        IfBlock,
        IfElseBlock,
        ElseBlock,
        DoBlock,
        ForEach,
        Range,
        While,
        Repeat,
        Function,
        Top
    }

    public class Scope
    {
        private Scope()
        {
            Locals = new Dictionary<string, ParameterExpression>();
        }

        public Scope Parent { get; set; }

        public ScopeType Type { get; private set; }

        public Environment Env { get; set; }

        public LabelTarget BreakTarget { get; private set; }

        public LabelTarget ReturnTarget { get; private set; }

        public LabelTarget ContinueTarget { get; internal set; }

        public Dictionary<string, ParameterExpression> Locals { get; private set; }

        public static Scope NewTopLevelScop()
        {
            return new Scope
            {
                Parent = null, 
                Env = new Environment(), 
                Type = ScopeType.Top, 
                ReturnTarget = Expression.Label(typeof (object))
            };
        }

        public Scope NewFunctionScope()
        {
            return new Scope
            {
                Parent = this, 
                Env = Env, 
                Type = ScopeType.Function, 
                ReturnTarget = Expression.Label(typeof (object))
            };
        }

        public Scope NewDoBlockScope()
        {
            return new Scope
            {
                Parent = this, 
                Env = Env, 
                Type = ScopeType.DoBlock
            };
        }

        public Scope NewForEachScope()
        {
            return new Scope
            {
                Parent = this,
                Env = Env,
                Type = ScopeType.ForEach,
                BreakTarget = Expression.Label(),
                ContinueTarget = Expression.Label()
            };
        }

        public Scope NewRangeScope()
        {
            return new Scope
            {
                Parent = this,
                Env = Env,
                Type = ScopeType.Range,
                BreakTarget = Expression.Label(),
                ContinueTarget = Expression.Label()
            };
        }

        public Scope NewWhileScope()
        {
            return new Scope
            {
                Parent = this,
                Env = Env,
                Type = ScopeType.While,
                BreakTarget = Expression.Label("break_label"),
                ContinueTarget = Expression.Label()
            };
        }

        public Scope NewRepeatScope()
        {
            return new Scope
            {
                Parent = this,
                Env = Env,
                Type = ScopeType.Repeat,
                BreakTarget = Expression.Label(),
                ContinueTarget = Expression.Label()
            };
        }

        public bool TryFindNearestFunctionReturn(out LabelTarget t)
        {
            var current = this;

            while (current != null)
            {
                if (current.SupportsReturn())
                {
                    t = current.ReturnTarget;
                    return true;
                }

                current = current.Parent;
            }

            t = null;

            return false;
        }

        public bool TryFindNearestBreak(out LabelTarget t)
        {
            var current = this;

            while (current != null && !current.IsTopLevel())
            {
                if (current.SupportsBreak())
                {
                    t = current.BreakTarget;
                    return true;
                }

                current = current.Parent;
            }

            t = null;

            return false;
        }

        public bool TryFindNearestConintue(out LabelTarget t)
        {
            var current = this;

            while (current != null)
            {
                if (current.SupportsContinue())
                {
                    t = current.ContinueTarget;
                    return true;
                }

                current = current.Parent;
            }

            t = null;

            return false;
        }

        public bool IsTopLevel()
        {
            switch (Type)
            {
                case ScopeType.Function:
                case ScopeType.Top:
                    return true;
                default:
                    return false;
            }
        }

        public bool SupportsBreak()
        {
            switch (Type)
            {
                case ScopeType.DoBlock:
                case ScopeType.Function:
                case ScopeType.Top:
                    return false;
                default:
                    return true;
            }
        }

        public bool SupportsContinue()
        {
            switch (Type)
            {
                case ScopeType.DoBlock:
                case ScopeType.Function:
                case ScopeType.Top:
                    return false;
                default:
                    return true;
            }
        }

        public bool SupportsReturn()
        {
            switch (Type)
            {
                case ScopeType.Top:
                case ScopeType.Function:
                    return true;
                default:
                    return false;
            }
        }
    }
}
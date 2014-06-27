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
    public abstract class Generator
    {
        protected Scope Scope { get; set; }

        protected CommonTree Tree { get; set; }

        protected ErrorList Errors { get; set; }

        protected StaticMetaTables StaticTables { get; set; }

        public ErrorList Generate(StaticMetaTables staticTables, Scope ctx, ITree ast, out Expression exp)
        {
            Tree = (CommonTree) ast;
            Scope = ctx;
            StaticTables = staticTables;
            Errors = new ErrorList();
            exp = CheckError(Tree) ? Expression.Empty() : OnGenerate();
            return Errors;
        }

        public ErrorList Generate(StaticMetaTables staticTables, Scope ctx, ITree ast, out Expression[] exps)
        {
            StaticTables = staticTables;
            Tree = (CommonTree) ast;
            Scope = ctx;
            Errors = new ErrorList();
            exps = CheckError(Tree) ? new Expression[0] : OnGenerateArray();
            return Errors;
        }

        protected static Expression EmitPanic()
        {
            return Expression.Empty();
        }

        protected static Expression EmitToNumber(Expression e)
        {
            return Expression.Empty();
        }

        protected static Expression ToBool(Expression exp)
        {
            return RuntimeHelper.EmitToBool(exp);
        }

        protected static Expression CastTo<T>(Expression exp)
        {
            return Expression.Convert(exp, typeof (T));
        }

        protected Expression Gen<T>(ITree t, Scope ctx = null) where T : Generator, new()
        {
            var g = new T();
            Expression exp;
            Errors.AddRange(g.Generate(StaticTables, ctx ?? Scope, t, out exp));
            return exp;
        }

        protected Expression Gen<T>(Scope ctx = null) where T : Generator, new()
        {
            return Gen<T>(Tree, ctx);
        }

        protected Expression[] GenArray<T>(ITree t, Scope ctx = null) where T : Generator, new()
        {
            var g = new T();
            Expression[] exp;
            Errors.AddRange(g.Generate(StaticTables, ctx ?? Scope, t, out exp));
            return exp;
        }

        protected Expression[] GenArray<T>(Scope ctx = null) where T : Generator, new()
        {
            return GenArray<T>(Tree, ctx);
        }

        protected bool CheckError(ITree t)
        {
            if (t.Type == 0)
            {
                Errors.ASTError((CommonErrorNode) t);
                return true;
            }
            return false;
        }

        protected abstract Expression OnGenerate();

        protected virtual Expression[] OnGenerateArray()
        {
            return new Expression[0];
        }

        public bool IsLocalVar(string name, out ParameterExpression local)
        {
            var current = Scope;

            while (current != null)
            {
                if (current.Locals.TryGetValue(name, out local))
                    return true;

                current = current.Parent;
            }

            local = null;

            return false;
        }

        protected Expression GenerateDefineLocal(string name, Expression e)
        {
            if (Scope.Locals.ContainsKey(name))
            {
                Errors.RedefinedLocalInSameScope(name);
                return Expression.Empty();
            }

            var p = Expression.Parameter(typeof (object), name);
            Scope.Locals[name] = p;

            return Expression.Assign(p, RuntimeHelper.EnsureObjectResult(e));
        }

        protected Expression GenerateDefineLocal(string name, Expression e, out ParameterExpression p)
        {
            if (Scope.Locals.ContainsKey(name))
            {
                Errors.RedefinedLocalInSameScope(name);
                p = null;
                return Expression.Empty();
            }

            p = Expression.Parameter(typeof (object), name);
            Scope.Locals[name] = p;

            return Expression.Assign(p, RuntimeHelper.EnsureObjectResult(e));
        }

        protected Expression GenerateDefineLocal(string name, out ParameterExpression p)
        {
            if (Scope.Locals.ContainsKey(name))
            {
                Errors.RedefinedLocalInSameScope(name);
                p = null;
                return Expression.Empty();
            }

            p = Expression.Parameter(typeof (object), name);
            Scope.Locals[name] = p;

            return Expression.Assign(p, Expression.Constant(null, typeof (object)));
        }
    }
}
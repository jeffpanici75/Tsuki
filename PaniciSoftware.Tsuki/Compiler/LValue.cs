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
using PaniciSoftware.Tsuki.Common;
using GetIndexBinder = PaniciSoftware.Tsuki.Runtime.GetIndexBinder;
using GetMemberBinder = PaniciSoftware.Tsuki.Runtime.GetMemberBinder;
using InvokeBinder = PaniciSoftware.Tsuki.Runtime.InvokeBinder;
using InvokeMemberBinder = PaniciSoftware.Tsuki.Runtime.InvokeMemberBinder;

namespace PaniciSoftware.Tsuki.Compiler
{
    public enum SuffixType
    {
        Property,
        Index,
        Root,
        Local
    }

    public class LValue : Generator
    {
        public string Name { get; set; }
        public Expression Index { get; set; }
        public SuffixType SuffixType { get; set; }

        protected override Expression OnGenerate()
        {
            switch (Tree.Type)
            {
                case ChunkParser.Rooted:
                {
                    return RootedVar();
                }
                case ChunkParser.Nested:
                {
                    return NestedVar();
                }
                default:
                {
                    Errors.UnknownTokenType(Tree.Type);
                    return Expression.Empty();
                }
            }
        }

        private Expression RootedVar()
        {
            var name = Tree.Children[0].Text;

            var hasSuffixes = Tree.Children.Count > 1;

            ParameterExpression root;
            Expression origin;

            if (IsLocalVar(name, out root))
            {
                if (!hasSuffixes)
                {
                    SuffixType = SuffixType.Local;
                    return root;
                }
                origin = root;
            }
            else
            {
                if (!hasSuffixes)
                {
                    SuffixType = SuffixType.Root;
                    Name = name;
                    return null;
                }
                origin = Expression.Dynamic(
                    GetMemberBinder.New(StaticTables, name),
                    typeof (object),
                    Scope.Env.GlobalParameter);
            }

            return ChainSuffixes(origin, Tree.Children);
        }

        private Expression NestedVar()
        {
            var nestedTree = (CommonTree) Tree.Children[0];
            var nestedExp = Gen<Exp>(nestedTree);
            return ChainSuffixes(nestedExp, Tree.Children);
        }

        private Expression ChainSuffixes(Expression origin, IList<ITree> suffixes)
        {
            for (var i = 1; i < suffixes.Count - 1; i++)
            {
                var suffix = (CommonTree) Tree.Children[i];

                ChainNameAndArgs(origin, suffix.Children, out origin);

                switch (suffix.Type)
                {
                    case ChunkParser.IndexedSuffix:
                    {
                        var index = Gen<Exp>(suffix.Children[0]);
                        var args = new List<Expression> {origin, index};
                        origin = Expression.Dynamic(
                            GetIndexBinder.New(StaticTables, new CallInfo(1)),
                            typeof (object),
                            args.ToArray());
                        break;
                    }
                    case ChunkParser.PropertySuffix:
                    {
                        var memberName = suffix.Children[0].Text;
                        origin = Expression.Dynamic(
                            GetMemberBinder.New(StaticTables, memberName),
                            typeof (object),
                            origin);
                        break;
                    }
                }
            }

            var last = (CommonTree) suffixes[Tree.Children.Count - 1];

            ChainNameAndArgs(origin, last.Children, out origin);

            switch (last.Type)
            {
                case ChunkParser.IndexedSuffix:
                {
                    SuffixType = SuffixType.Index;
                    Index = Gen<Exp>(last.Children[0]);
                    break;
                }
                case ChunkParser.PropertySuffix:
                {
                    SuffixType = SuffixType.Property;
                    Name = last.Children[0].Text;
                    break;
                }
            }

            return origin;
        }

        private void ChainNameAndArgs(
            Expression root,
            IList<ITree> children,
            out Expression current)
        {
            current = root;

            for (var i = 1; i < children.Count; i++)
            {
                var invokeSite = (CommonTree) children[i];
                switch (invokeSite.Type)
                {
                    case ChunkParser.MethodInvoke:
                    {
                        var name = invokeSite.Children[0].Text;
                        var node = (CommonTree) invokeSite.Children[1];
                        var args = new List<Expression> {current}; //this param
                        args.AddRange(GenArray<Args>(node));
                        current = Expression.Dynamic(
                            InvokeMemberBinder.New(StaticTables, name, new CallInfo(args.Count)),
                            typeof (object),
                            args);
                        break;
                    }
                    case ChunkParser.Invoke:
                    {
                        var node = (CommonTree) invokeSite.Children[0];
                        var args = new List<Expression> {current}; //object to be invoked.
                        args.AddRange(GenArray<Args>(node));
                        current = Expression.Dynamic(
                            InvokeBinder.New(StaticTables, new CallInfo(args.Count)),
                            typeof (object),
                            args);
                        break;
                    }
                }
            }
        }
    }
}
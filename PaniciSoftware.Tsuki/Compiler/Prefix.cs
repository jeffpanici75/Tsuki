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
using PaniciSoftware.Tsuki.Runtime;
using InvokeBinder = PaniciSoftware.Tsuki.Runtime.InvokeBinder;
using InvokeMemberBinder = PaniciSoftware.Tsuki.Runtime.InvokeMemberBinder;

namespace PaniciSoftware.Tsuki.Compiler
{
    public class Prefix : Generator
    {
        protected override Expression OnGenerate()
        {
            Expression current;

            switch (Tree.Type)
            {
                case ChunkParser.Prefix:
                {
                    var varTree = (CommonTree) Tree.Children[0];
                    current = Gen<Var>(varTree);
                    break;
                }
                case ChunkParser.NestedPrefix:
                {
                    var nestedTree = (CommonTree) Tree.Children[0];
                    current = RValueList.EmitNarrow(Gen<Exp>(nestedTree));
                    break;
                }
                default:
                {
                    Errors.UnknownTokenType(Tree.Type);
                    return Expression.Empty();
                }
            }

            for (var i = 1; i < Tree.Children.Count; i++)
            {
                var invokeSite = (CommonTree) Tree.Children[i];

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

            return current;
        }
    }
}
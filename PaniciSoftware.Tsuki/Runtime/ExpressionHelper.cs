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
using PaniciSoftware.Tsuki.Common;

namespace PaniciSoftware.Tsuki.Runtime
{
    public static class ExpressionHelper
    {
        public static Type PreProcessFunction(
            CommonTree parameters,
            out List<ParameterExpression> paramList,
            bool impliedSelf = false)
        {
            var hasVarArgs = parameters != null && parameters.Type == ChunkParser.VarArgs;

            paramList = new List<ParameterExpression>();

            var argTypesLength = 1 /*return value*/ + (parameters == null ? 0 : parameters.ChildCount)
                                 + (impliedSelf ? 1 : 0)
                                 + (hasVarArgs ? 1 : 0);

            var argTypes = new Type[argTypesLength];

            var start = impliedSelf ? 1 : 0;
            if (impliedSelf)
            {
                argTypes[0] = typeof (object);
                paramList.Add(Expression.Parameter(typeof (object), "self"));
            }

            var mod = 1 + (hasVarArgs ? 1 : 0);

            for (var i = start; i < argTypes.Length - mod; i++)
            {
                // parameters is null only when no explicit parameters are passed in. 
                // In the case of no explicit parameters this loop is skipped.

                paramList.Add(Expression.Parameter(typeof (object), parameters.Children[i].Text));

                argTypes[i] = typeof (object);
            }

            if (hasVarArgs)
            {
                paramList.Add(Expression.Parameter(typeof (VarArgs), "args"));
                argTypes[argTypes.Length - 2] = typeof (VarArgs);
            }

            argTypes[argTypes.Length - 1] = typeof (object); //return value type

            return Expression.GetFuncType(argTypes);
        }
    }
}
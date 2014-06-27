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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Antlr.Runtime;
using PaniciSoftware.Tsuki.Common;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.Compiler
{
    public class Chunk : Generator
    {
        public static LuaResult Compile(
            StaticMetaTables staticTables,
            string source,
            dynamic globals = null,
            string name = null)
        {
            var result = new LuaResult();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var stream = new ANTLRStringStream(source);
                var lexer = new ChunkLexer(stream);
                var tokenStream = new CommonTokenStream(lexer);
                var parser = new ChunkParser(tokenStream);

                var r = parser.chunk();
                result.Errors.AddRange(parser.Errors);

                Expression e;
                var scope = Scope.NewTopLevelScop();
                var chunk = new Chunk();
                result.Errors.AddRange(chunk.Generate(staticTables, scope, r.Tree, out e));

                var fnExp = Expression.Lambda<Func<Table, object>>(e, scope.Env.GlobalParameter);
                result.Chunk = new CompiledChunk(fnExp.Compile(), globals ?? new Table(), name);
            }
            finally
            {
                stopwatch.Stop();
                result.ElapsedTime = stopwatch.Elapsed;
                result.Success = !result.Errors.ContainsError();
            }
            return result;
        }

        protected override Expression OnGenerate()
        {
            if (CheckError(Tree))
            {
                return Expression.Block(
                    typeof (object),
                    Expression.Empty(),
                    Expression.Constant(null, typeof (object)));
            }

            if (Tree.Children == null)
            {
                return Expression.Block(
                    typeof (object),
                    Expression.Empty(),
                    Expression.Constant(null, typeof (object)));
            }
            var blocks = Tree.Children;
            var list = blocks.Select(t => Gen<Statement>(t)).ToList();
            var block = Expression.Block(typeof (void), Scope.Locals.Values, list);
            return Expression.Block(
                typeof (object),
                block,
                Expression.Label(
                    Scope.ReturnTarget,
                    Expression.Constant(null, typeof (object))));
        }
    }
}
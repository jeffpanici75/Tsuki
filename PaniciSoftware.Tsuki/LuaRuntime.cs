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

using PaniciSoftware.Tsuki.Compiler;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki
{
    public class LuaRuntime
    {
        private Table _environment = new Table();

        public LuaRuntime(Table environment = null)
        {
            if (environment != null)
                _environment = environment;

            StaticMetaTables = new StaticMetaTables();
        }

        public object this[object key]
        {
            get { return _environment[key]; }
            set { _environment[key] = value; }
        }

        public Table Environment
        {
            get { return _environment; }
            set { _environment = value; }
        }

        public StaticMetaTables StaticMetaTables { get; set; }

        public LuaResult Compile(
            string source,
            Table environment = null,
            string name = null)
        {
            return Chunk.Compile(
                StaticMetaTables,
                source,
                environment ?? _environment,
                name);
        }
    }
}
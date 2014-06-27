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

using System.IO;
using NUnit.Framework;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.Test
{
    public static class TestHelper
    {
        public static string GetEmbeddedScript(string name)
        {
            var fullName = "PaniciSoftware.Tsuki.Test.Regression_Scripts." + name;
            try
            {
                var assembly = typeof (TestHelper).Assembly;
                var textStreamReader = new StreamReader(assembly.GetManifestResourceStream(fullName));
                return textStreamReader.ReadToEnd();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static dynamic AssertedExecuteScript(
            this LuaRuntime runtime,
            string source,
            string name = null,
            Table environment = null)
        {
            var result = runtime.Compile(source, name: name);
            Assert.IsEmpty(result.Errors);
            result = result.Chunk.Execute();
            Assert.IsEmpty(result.Errors);
            return result.Results;
        }

        public static dynamic AssertedExecuteScriptExpectingCompileErrors(
            this LuaRuntime runtime,
            string source,
            string name = null,
            Table environment = null)
        {
            var result = runtime.Compile(source, name: name);
            Assert.IsNotEmpty(result.Errors);
            result = result.Chunk.Execute();
            Assert.IsEmpty(result.Errors);
            return result.Results;
        }

        public static dynamic AssertedExecuteScriptExpectingRuntimeErrors(
            this LuaRuntime runtime,
            string source,
            string name = null,
            Table environment = null)
        {
            var result = runtime.Compile(source, name: name);
            Assert.IsEmpty(result.Errors);
            result = result.Chunk.Execute();
            Assert.IsNotEmpty(result.Errors);
            return result.Results;
        }
    }
}
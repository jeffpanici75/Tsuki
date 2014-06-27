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

using NUnit.Framework;

namespace PaniciSoftware.Tsuki.Test
{
    [TestFixture]
    public class AssignmentTests
    {
        [Test]
        public void AssignNestedMultilineStringShouldSetGlobalTable_Section3_1()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = [=[This is an example of [=[a nested sort of [==[long string]==]]==]]=]");
            Assert.AreEqual("This is an example of [=[a nested sort of [==[long string]==]]==]", runtime["a"]);
        }

        [Test]
        public void AssigningMultilineStringShouldSetGlobalTable_Section3_1()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = [[This is a test of a \r\nmultiline string in Lua.]]");
            Assert.AreEqual("This is a test of a \r\nmultiline string in Lua.", runtime["a"]);
        }

        [Test]
        public void AssigningMultipleVariablesShouldSetGlobalTable_Section3_3_3()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript(
                "a = 1\r\nb = 2\r\nc = 'This is a test string.'\r\nd=\"Another test string\"\r\ne = 3.14");
            Assert.AreEqual(1, runtime["a"]);
            Assert.AreEqual(2, runtime["b"]);
            Assert.AreEqual("This is a test string.", runtime["c"]);
            Assert.AreEqual("Another test string", runtime["d"]);
            Assert.AreEqual(3.14, runtime["e"]);
        }

        [Test]
        public void AssigningVariableFromFunctionResult_Section3_3_3()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("local f = function(a, b) return a + b end\r\na = f(2, 4)");
            Assert.AreEqual(6, runtime["a"]);
        }

        [Test]
        public void AssigningVariableShouldSetGlobalTable_Section3_2()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 1");
            Assert.AreEqual(1, runtime["a"]);
        }

        [Test]
        public void AssingingVariableWithDifferentBaseValue_Section_3_1()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 0xff;");
            Assert.AreEqual(255, runtime["a"]);
        }

        [Test]
        public void MultipleAssignmentShouldSetGlboalTable_Section3_3_3()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a, b, c = 1, 3.14, 'Wow! This really works!'");
            Assert.AreEqual(1, runtime["a"]);
            Assert.AreEqual(3.14, runtime["b"]);
            Assert.AreEqual("Wow! This really works!", runtime["c"]);
        }
    }
}
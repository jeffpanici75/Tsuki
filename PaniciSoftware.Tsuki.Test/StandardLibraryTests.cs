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
using PaniciSoftware.Tsuki.StandardLib;

namespace PaniciSoftware.Tsuki.Test
{
    [TestFixture]
    public class StandardLibraryTests
    {
        [Test]
        public void AssertDoesNotRaiseErrorIfTrue_Section_6_1()
        {
            var runtime = new LuaRuntime();
            runtime.Environment.ImportBasicFunctions();
            runtime.AssertedExecuteScript("assert(a == nil)");
        }

        [Test]
        public void AssertRaisesErrorIfFalse_Section_6_1()
        {
            var runtime = new LuaRuntime();
            runtime.Environment.ImportBasicFunctions();
            runtime.AssertedExecuteScriptExpectingRuntimeErrors("assert(a ~= nil, 'a is nill and that is bad!')");
        }

        [Test]
        public void ToNumberConvertsInAnyBase_Section_6_1()
        {
            var runtime = new LuaRuntime();
            runtime.Environment.ImportBasicFunctions();
            runtime.AssertedExecuteScript(
                "a = tonumber('C000', 16)\r\nb = tonumber('49152', 10)\r\nc = tonumber(140000, 8)\r\nd = tonumber('-128',10)\r\ne = tonumber('ffff', 16)\r\nf = tonumber('123')");
            Assert.AreEqual(49152, runtime["a"]);
            Assert.AreEqual(49152, runtime["b"]);
            Assert.AreEqual(49152, runtime["c"]);
            Assert.AreEqual(-128, runtime["d"]);
            Assert.AreEqual(65535, runtime["e"]);
            Assert.AreEqual(123, runtime["f"]);
        }

        [Test]
        public void TypeReturnsPropertStringDesignation_Section_6_1()
        {
            var runtime = new LuaRuntime();
            runtime.Environment.ImportBasicFunctions();
            runtime.AssertedExecuteScript(
                "a = type(nil)\r\nb = type('test')\r\nc = type(33)\r\nd = type(function() return 666 end)\r\ne = type(tostring)\r\nf = type(true)");
            Assert.AreEqual("nil", runtime["a"]);
            Assert.AreEqual("string", runtime["b"]);
            Assert.AreEqual("number", runtime["c"]);
            Assert.AreEqual("function", runtime["d"]);
            Assert.AreEqual("function", runtime["e"]);
            Assert.AreEqual("boolean", runtime["f"]);
        }
    }
}
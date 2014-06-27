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
    public class BlockTests
    {
        [Test]
        public void ExplicitBlock_Section3_3_1()
        {
            var runtime = new LuaRuntime();
            var result = runtime.AssertedExecuteScript("local j = 100; do local j = 200; a = 1; b = 'This is an explicit block.'; return j end");
            Assert.AreEqual(1, runtime["a"]);
            Assert.AreEqual("This is an explicit block.", runtime["b"]);
            Assert.AreEqual(200, result);
        }

        [Test]
        public void ImplicitBlock_Section3_3_1()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 1; b = 'This is an implicit block.'");
            Assert.AreEqual(1, runtime["a"]);
            Assert.AreEqual("This is an implicit block.", runtime["b"]);
        }
    }
}
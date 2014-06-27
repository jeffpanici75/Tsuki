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
    public class ControlStructureTests
    {
        [Test]
        public void BasicForLoopWithNegativeStep_Section3_3_5()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("b = 101; for a = 100, 1, -2 do b = b - 2; end");
            Assert.AreEqual(1, runtime["b"]);
        }

        [Test]
        public void BasicForLoopWithPositiveStep_Section3_3_5()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("b = 1; for a = 1, 100, 2 do b = b + 2; end");
            Assert.AreEqual(101, runtime["b"]);
        }

        [Test]
        public void BasicForLoopWithoutStepEndsWithBreak_Section3_3_5()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("b = 1; for a = 1, 100 do b = b + 2; if a == 20 then break end; end");
            Assert.AreEqual(41, runtime["b"]);
        }

        [Test]
        public void BasicForLoopWithoutStepExecutesExceptDuringContinue_Section3_3_5()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("b = 1; for a = 1, 100 do if (a % 2) == 0 then continue end; b = b + 2; end");
            Assert.AreEqual(101, runtime["b"]);
        }

        [Test]
        public void BasicForLoopWithoutStep_Section3_3_5()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("b = 1; for a = 1, 100 do b = b + 2; end");
            Assert.AreEqual(201, runtime["b"]);
        }

        [Test]
        public void BasicIfControlsFlow_Section3_3_4()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 1; if a == 2 then b = 1 else b = 4 end");
            Assert.AreEqual(4, runtime["b"]);
        }

        [Test]
        public void NestedIfControlsFlow_Section3_3_4()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 1; if a == 2 then b = 1 else if a == 1 then b = 6 else b = 2 end end");
            Assert.AreEqual(6, runtime["b"]);
        }

        [Test]
        public void RepeatUntilExecutesBlock_Section3_3_4()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 1; b = 1; repeat a, b = a + 1, b + 2; until a == 100");
            Assert.AreEqual(100, runtime["a"]);
            Assert.AreEqual(199, runtime["b"]);
        }

        [Test]
        public void WhileLoopExecutesBlockExceptDuringContinue_Section3_3_4()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 1; b = 1; while a <= 100 do a = a + 1; if (a % 2) == 0 then continue end; b = b + 2; end");
            Assert.AreEqual(101, runtime["a"]);
            Assert.AreEqual(101, runtime["b"]);
        }

        [Test]
        public void WhileLoopExecutesBlock_Section3_3_4()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 1; b = 1; while a <= 100 do a, b = a + 1, b + 2 end");
            Assert.AreEqual(101, runtime["a"]);
            Assert.AreEqual(201, runtime["b"]);
        }

        [Test]
        public void WhileLoopExecutesUntilBreak_Section3_3_4()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 1; b = 1; while a <= 100 do a, b = a + 1, b + 2; if a == 50 then break end end");
            Assert.AreEqual(50, runtime["a"]);
            Assert.AreEqual(99, runtime["b"]);
        }
    }
}
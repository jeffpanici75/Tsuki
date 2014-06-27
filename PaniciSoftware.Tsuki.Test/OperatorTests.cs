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
    public class OperatorTests
    {
        [Test]
        public void AdditionOperator_Section_3_4_1()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 1 + 2\r\nb = 1 + (2 + 2)\r\nc = 3.14 + 2.71 + 9.1122\r\nd = 3.14 + 3");
            Assert.AreEqual(3, runtime["a"]);
            Assert.AreEqual(5, runtime["b"]);
            Assert.AreEqual(14.9622, runtime["c"]);
            Assert.IsTrue((decimal) runtime["d"] >= 6.139M);
        }

        [Test]
        public void ConcatenationOperator_Section_3_4_5()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 'This is a test' .. ' and I know Benny will be fixing it!' .. 1 .. 2 .. 3 .. 4");
            Assert.AreEqual("This is a test and I know Benny will be fixing it!1234", runtime["a"]);
        }

        [Test]
        public void DivisionOperator_Section_3_4_1()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 100 / 2\r\nb = (200 + 300) / 4\r\nc = 3.14 / 2");
            Assert.AreEqual(50, runtime["a"]);
            Assert.AreEqual(125, runtime["b"]);
            Assert.AreEqual(1.57, runtime["c"]);
        }

        [Test]
        public void ExponentiationOperator_Section_3_4_1()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 2 ^ 16\r\nb = 2 ^ 2.16");
            Assert.AreEqual(65536, runtime["a"]);
            Assert.IsTrue((double) runtime["b"] > 4.4469);
        }

        [Test]
        public void LengthOperator_Section_3_4_6()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript(
                "a = 'This is a test.'\r\nb = {1, 2, 3, 4, 5, 6}\r\nc = { jeff = 'Winner!'; deddie = 'Loser!' }\r\na_len = #a\r\nb_len = #b\r\nc_len = #c");
            Assert.AreEqual("This is a test.".Length, runtime["a_len"]);
            Assert.AreEqual(6, runtime["b_len"]);
            Assert.AreEqual(0, runtime["c_len"]);
        }

        [Test]
        public void LogicalOperator_Section_3_4_4()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript(
                "a = 10 or 20\r\nb = nil or \"a\"\r\nc = nil and 10\r\nd = false and false\r\ne = false and nil\r\nf = false or nil\r\ng = 10 and 20");
            Assert.AreEqual(10, runtime["a"]);
            Assert.AreEqual("a", runtime["b"]);
            Assert.IsNull(runtime["c"]);
            Assert.AreEqual(false, runtime["d"]);
            Assert.AreEqual(false, runtime["e"]);
            Assert.IsNull(runtime["f"]);
            Assert.AreEqual(20, runtime["g"]);
        }

        [Test]
        public void ModuloOperator_Section_3_4_1()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 255 % 255\r\nb = 3.14 % 2");
            Assert.AreEqual(0, runtime["a"]);
            Assert.IsTrue((decimal) runtime["b"] >= 1.140M);
        }

        [Test]
        public void MultiplicationOperator_Section_3_4_1()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 2 * 8\r\nb = 16 * (256 + 128)\r\nc = 3.14 * 100");
            Assert.AreEqual(16, runtime["a"]);
            Assert.AreEqual(6144, runtime["b"]);
            Assert.AreEqual(314, runtime["c"]);
        }

        [Test]
        public void NegateOperator_Section_3_4_1()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = -100\r\nb = -3.14");
            Assert.AreEqual(-100, runtime["a"]);
            Assert.AreEqual(-3.14, runtime["b"]);
        }

        [Test]
        public void RelationalOperators_Section_3_4_3()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript(
                "a = 1 == 1\r\nb = 1 ~= 2\r\nc = 5 > 3\r\nd = 3 < 5\r\ne = 4 >= 4\r\nf = 3 <= 3\r\ng = 3 <= 4\r\nh = 5 >= 4\r\ni = 3.14 > 1.12\r\nj = 1 < 3.14");
            Assert.AreEqual(true, runtime["a"]);
            Assert.AreEqual(true, runtime["b"]);
            Assert.AreEqual(true, runtime["c"]);
            Assert.AreEqual(true, runtime["d"]);
            Assert.AreEqual(true, runtime["e"]);
            Assert.AreEqual(true, runtime["f"]);
            Assert.AreEqual(true, runtime["g"]);
            Assert.AreEqual(true, runtime["h"]);
            Assert.AreEqual(true, runtime["i"]);
            Assert.AreEqual(true, runtime["j"]);
        }

        [Test]
        public void SubtractionOperator_Section_3_4_1()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = 10 - 2\r\nb = 100 - (50 + 25)\r\nc = 75.55 - 33.22\r\nd = 75.55 - 55");
            Assert.AreEqual(8, runtime["a"]);
            Assert.AreEqual(25, runtime["b"]);
            Assert.AreEqual(42.33, runtime["c"]);
            var d = (decimal) runtime["d"];
            Assert.IsTrue(d >= 20.549M);
        }

        [Test]
        public void TypeCoercionFromStringToNumber_Section_3_4_2()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = '1' + '2'\r\nb = '1' + ('2' + '2')\r\nc = '3.14' + '2.71' + '9.1122'\r\nd = '3.14' + '3'");
            Assert.AreEqual(3, runtime["a"]);
            Assert.AreEqual(5, runtime["b"]);
            Assert.AreEqual((decimal) 14.9622, runtime["c"]);
            var result = (decimal) runtime["d"];
            Assert.IsTrue(result >= ((decimal) 6.139));
        }

        [Test]
        public void NotOperatorReturnsBoolean_Section_3_3()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = not nil\r\nb = not false\r\nc = not 0\r\nd = not not nil");
            Assert.AreEqual(true, runtime["a"]);
            Assert.AreEqual(true, runtime["b"]);
            Assert.AreEqual(false, runtime["c"]);
            Assert.AreEqual(false, runtime["d"]);
        }
    }
}
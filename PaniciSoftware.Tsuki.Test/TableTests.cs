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
using PaniciSoftware.Tsuki.Runtime;
using PaniciSoftware.Tsuki.StandardLib;

namespace PaniciSoftware.Tsuki.Test
{
    [TestFixture]
    public class TableTests
    {
        [Test]
        public void CreateInlineTableWithMixedKeys_Section_3_4_8()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = {jeff = 'Fat', deddie = 'Not fat, but short'; 3, 4, 5, 6, 7 }");
            var table = (Table) runtime["a"];
            Assert.AreEqual(7, table.Count);
            Assert.AreEqual("Fat", table["jeff"]);
            Assert.AreEqual("Not fat, but short", table["deddie"]);
            Assert.AreEqual(4, table[2M]);
        }

        [Test]
        public void CreateInlineTable_Section_3_4_8()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = {1, 2, 3, 4, 5}");
            var table = (Table) runtime["a"];
            Assert.AreEqual(5, table.Count);
            Assert.AreEqual(5, table[5M]);
        }

        [Test]
        public void CreateNestedTables_Section_3_4_8()
        {
            var runtime = new LuaRuntime();
            runtime.AssertedExecuteScript("a = {jeff = 'Fat', deddie = 'Not fat, but short'; 3, 4, 5, 6, 7; b = {1, 2, 3, c = {4,5,6; d = {7,8, jeff = 'foo', bar = 'baz', bing = {}}}}}");
            var atable = (Table) runtime["a"];
            Assert.AreEqual(8, atable.Count);
            var btable = (Table) atable["b"];
            var ctable = (Table) btable["c"];
            var dtable = (Table) ctable["d"];
            var bingtable = (Table) dtable["bing"];
            Assert.AreEqual(0, bingtable.Count);
            Assert.AreEqual(5, dtable.Count);
            Assert.AreEqual(4, ctable.Count);
            Assert.AreEqual(4, btable.Count);
        }

        [Test]
        public void IterateIntegerKeyedTable_Section_3_3_5()
        {
            var runtime = new LuaRuntime();
            runtime.Environment.ImportBasicFunctions();
            runtime.AssertedExecuteScript("b = 0; a = {10, 20, 30, 40, 50, 60, 70, 80, 90}\r\nfor _, v in ipairs(a) do b = b + v end");
            Assert.AreEqual(450M, runtime["b"]);
        }

        [Test]
        public void IterateNamedKeysInTable_Section_3_3_5()
        {
            var runtime = new LuaRuntime();
            runtime.Environment.ImportBasicFunctions();
            runtime.AssertedExecuteScript(
                "b = 0; c = '';a = {b = 10, c = 20, d = 30, e = 40, f = 50, g = 60, h = 70, i = 80, j = 90}\r\nfor k, v in pairs(a) do b = b + v; c = c .. ' ' .. k end");
            Assert.AreEqual(450M, runtime["b"]);
        }
    }
}
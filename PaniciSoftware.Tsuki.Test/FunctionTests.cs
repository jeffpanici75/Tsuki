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
using NUnit.Framework;
using PaniciSoftware.Tsuki.Runtime;
using PaniciSoftware.Tsuki.StandardLib;

namespace PaniciSoftware.Tsuki.Test
{
    [TestFixture]
    public class FunctionTests
    {
        [Test]
        public void AssignFunctionToLocalAndCall_Section3_4_9()
        {
            var runtime = new LuaRuntime();
            var printStack = new Stack<string>();
            runtime["tostring"] = (Func<object, string>) (o => o != null ? o.ToString() : "nil");
            runtime["print"] = (Action<string>) printStack.Push;
            runtime.AssertedExecuteScript(@"local boo = print; boo('Test1!'); boo('Test2!'); boo(tostring(3))");
            Assert.AreEqual("3", printStack.Pop());
            Assert.AreEqual("Test2!", printStack.Pop());
            Assert.AreEqual("Test1!", printStack.Pop());
            Assert.IsEmpty(printStack);
        }

        [Test]
        public void FunctionCallAsStatement_Section_3_3_6()
        {
            var runtime = new LuaRuntime();
            var printStack = new Stack<string>();
            runtime["tostring"] = (Func<object, string>) (o => o != null ? o.ToString() : "nil");
            runtime["print"] = (Action<string>) printStack.Push;
            runtime.AssertedExecuteScript(@"print('Test1!'); print('Test2!'); print(tostring(3))");
            Assert.AreEqual("3", printStack.Pop());
            Assert.AreEqual("Test2!", printStack.Pop());
            Assert.AreEqual("Test1!", printStack.Pop());
            Assert.IsEmpty(printStack);
        }

        [Test]
        public void FunctionCallAsStatement_Section_3_4_Preamble()
        {
            Assert.True(true);
        }

        [Test]
        public void FunctionCallWithTable_Section_3_4_9()
        {
            var runtime = new LuaRuntime();
            var result = runtime.AssertedExecuteScript(@"local boo = function(t) return t.x + t.y end; return boo{ x=2, y=4};");
            Assert.AreEqual(6, result);
        }

        [Test]
        public void FunctionDefintionAndCall_Section_3_4_10_And_Section_3_4_9()
        {
            var runtime = new LuaRuntime();
            var result = runtime.AssertedExecuteScript(@"local boo = function(x, y) return x + y end; return boo(2, 4);");
            Assert.AreEqual(6, result);
        }

        // N.B. I believe this is broken because the magick "args" table is missing
        [Test]
        public void FunctionWithVariableArguments_3_4_10()
        {
            var runtime = new LuaRuntime();
            runtime.Environment.ImportBasicFunctions();
            var result = runtime.AssertedExecuteScript(
                @"function join(...)
                    local s = ''
                    local tbl = {args}
                    for _, v in ipairs(tbl) do
                        s = s .. tostring(v)
                    end
                    return s
                  end
                    
                  return join('jeff', ' and ', 'deddie', ' go', ' to ', 2021, ' Midwest Road, Suite 200')");
            Assert.AreEqual("jeff and deddie go to 2021 Midwest Road, Suite 200", result);
        }

        [Test]
        public void GeneratePrimeNumbers()
        {
            var runtime = new LuaRuntime();
            runtime.Environment.ImportBasicFunctions();
            runtime["tostring"] = (Func<object, string>) (o => o != null ? o.ToString() : "nil");
            runtime["print"] = (Action<string>) Console.WriteLine;
            runtime.AssertedExecuteScript(
                @"---Find primes smaller than this number:
                LIMIT=100

                ---Checks to see if a number is prime or not
                function isPrime(n)
                    print(tostring(n))
                    if n<=1 then return false end
	                if n==2 then return true end
	                if (n % 2 == 0 ) then return false end
	                for i=3, n / 2, 2 do
		                if (n % i == 0 ) then return false end
	                end
	                return true
                end

                primes = {}

                ---Print all the prime numbers within range
                for i=1, LIMIT do
	                if isPrime(i) then
                        print(tostring(i) .. ' is prime!') 
                        print('#primes = ' .. tostring(#primes))
                        primes[#primes + 1] = i
                    end
                end");
            var primes = (Table) runtime["primes"];
            Assert.AreEqual(25, primes.Count);
        }

        [Test]
        public void ProperTailCall_Section3_4_9()
        {
            var runtime = new LuaRuntime();
            var printStack = new Stack<string>();
            runtime["tostring"] = (Func<object, string>) (o => o != null ? o.ToString() : "nil");
            runtime["print"] = (Action<string>) printStack.Push;
            runtime.AssertedExecuteScript(
                @"local up; local down; local right;
                    local function left(from) print('went left from ' .. tostring(from)); return right('left') end; 
                    down = function(from) print('went down from ' .. tostring(from)); return 'end' end;
                    up = function(from) print('went up from ' .. tostring(from)); return down('up') end;
                    right = function(from) print('went right from ' .. tostring(from)); return up('right') end;
                    print(tostring(left('start')))");
            Assert.AreEqual("end", printStack.Pop());
            Assert.AreEqual("went down from up", printStack.Pop());
            Assert.AreEqual("went up from right", printStack.Pop());
            Assert.AreEqual("went right from left", printStack.Pop());
            Assert.AreEqual("went left from start", printStack.Pop());
            Assert.IsEmpty(printStack);
        }
    }
}
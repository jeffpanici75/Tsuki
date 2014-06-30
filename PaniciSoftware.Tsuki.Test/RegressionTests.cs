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
using System.Text;
using NUnit.Framework;
using PaniciSoftware.Tsuki.Runtime;
using PaniciSoftware.Tsuki.StandardLib;

namespace PaniciSoftware.Tsuki.Test
{
    [TestFixture]
    public class RegressionTests
    {
        private readonly StringBuilder _fakeOutput = new StringBuilder();

        [SetUp]
        public void SetUp()
        {
            _fakeOutput.Clear();
        }

        public void ExecuteCfScript()
        {
            const string expectedResult = @"C -20 -19 -18 -17 -16 -15 -14 -13 -12 -11 
F  -4  -2  -0   1   3   5   7   9  10  12 

C -10  -9  -8  -7  -6  -5  -4  -3  -2  -1 
F  14  16  18  19  21  23  25  27  28  30 

C   0   1   2   3   4   5   6   7   8   9 
F  32  34  36  37  39  41  43  45  46  48 

C  10  11  12  13  14  15  16  17  18  19 
F  50  52  54  55  57  59  61  63  64  66 

C  20  21  22  23  24  25  26  27  28  29 
F  68  70  72  73  75  77  79  81  82  84 

C  30  31  32  33  34  35  36  37  38  39 
F  86  88  90  91  93  95  97  99 100 102 

C  40  41  42  43  44  45  46  47  48  49 
F 104 106 108 109 111 113 115 117 118 120 

";
        }

        internal void IoWrite(VarArgs args)
        {
            foreach (var each in args.ToList())
                _fakeOutput.Append(each);
        }

        internal string Bisect_StringFormat(string format, VarArgs args)
        {
            //"after %d steps, root is %.17g with error %.1e, f=%.1e\n"
            format = format.Replace("%d", "{0}");
            format = format.Replace("%.17g", "{1:G1}");
            format = format.Replace("%.1e,", " {2:e1},");
            format = format.Replace("f=%.1e", "f={3:e1}");
            return string.Format(format, args.ToList().ToArray());
        }

        [Test]
        public void ExecuteBisectScript()
        {
            var runtime = new LuaRuntime();
            var ioTable = new Table();
            ioTable["write"] = (Action<VarArgs>) IoWrite;
            runtime["io"] = ioTable;
            var mathTable = new Table();
            mathTable["abs"] = (Func<decimal, decimal>) (Math.Abs);
            runtime["math"] = mathTable;
            var stringTable = new Table();
            stringTable["format"] = (Func<string, VarArgs, string>) Bisect_StringFormat;
            runtime["string"] = stringTable;
            var scriptSource = TestHelper.GetEmbeddedScript("bisect.lua");
            runtime.AssertedExecuteScript(scriptSource);
            Assert.IsNotEmpty(_fakeOutput.ToString());
            var output = _fakeOutput.ToString();
            Assert.AreEqual(@"0 c=1.5 a=1 b=2
1 c=1.25 a=1 b=1.5
2 c=1.375 a=1.25 b=1.5
3 c=1.3125 a=1.25 b=1.375
4 c=1.34375 a=1.3125 b=1.375
5 c=1.328125 a=1.3125 b=1.34375
6 c=1.3203125 a=1.3125 b=1.328125
7 c=1.32421875 a=1.3203125 b=1.328125
8 c=1.326171875 a=1.32421875 b=1.328125
9 c=1.3251953125 a=1.32421875 b=1.326171875
10 c=1.32470703125 a=1.32421875 b=1.3251953125
11 c=1.324951171875 a=1.32470703125 b=1.3251953125
12 c=1.3248291015625 a=1.32470703125 b=1.324951171875
13 c=1.32476806640625 a=1.32470703125 b=1.3248291015625
14 c=1.324737548828125 a=1.32470703125 b=1.32476806640625
15 c=1.3247222900390625 a=1.32470703125 b=1.324737548828125
16 c=1.32471466064453125 a=1.32470703125 b=1.3247222900390625
17 c=1.324718475341796875 a=1.32471466064453125 b=1.3247222900390625
18 c=1.3247165679931640625 a=1.32471466064453125 b=1.324718475341796875
19 c=1.32471752166748046875 a=1.3247165679931640625 b=1.324718475341796875
20 c=1.324717998504638671875 a=1.32471752166748046875 b=1.324718475341796875
after 20 steps, root is 1 with error  9.5e-007, f=1.8e-007
", output);
        }

        [Test]
        public void ExecuteFactorialScript()
        {
            const string expectedResult = @"0! = 1
1! = 1
2! = 2
3! = 6
4! = 24
5! = 120
6! = 720
7! = 5040
8! = 40320
9! = 362880
10! = 3628800
11! = 39916800
12! = 479001600
13! = 6227020800
14! = 87178291200
15! = 1307674368000
16! = 20922789888000
";

            var runtime = new LuaRuntime();
            var ioTable = new Table();
            ioTable["write"] = (Action<VarArgs>) IoWrite;
            runtime["io"] = ioTable;
            var scriptSource = TestHelper.GetEmbeddedScript("factorial.lua");
            runtime.AssertedExecuteScript(scriptSource);
            Assert.IsNotEmpty(_fakeOutput.ToString());
            Assert.AreEqual(expectedResult, _fakeOutput.ToString());
        }

        [Test]
        public void ExecuteEnv()
        {
            var runtime = new LuaRuntime();
            runtime.Environment.ImportBasicFunctions();
            runtime.Environment.ImportStatefulBasicFunctions(runtime, runtime.StaticMetaTables);
            runtime.Environment.ImportSystemFunctions();
            runtime["print"] = (Action<VarArgs>)IoWrite;

            var scriptSource = TestHelper.GetEmbeddedScript("env.lua");
            runtime.AssertedExecuteScript(scriptSource);
            Assert.IsNotEmpty(_fakeOutput.ToString());
            Console.WriteLine(_fakeOutput.ToString());
        }

        [Test]
        public void ExecuteEcho()
        {
            var runtime = new LuaRuntime();
            runtime["print"] = (Action<VarArgs>)IoWrite;

            var arg = new Table();
            arg[0] = "echo.lua";
            arg[1] = "one";
            arg[2] = "two";
            arg[3] = "three";
            runtime["arg"] = arg;

            var scriptSource = TestHelper.GetEmbeddedScript("echo.lua");
            runtime.AssertedExecuteScript(scriptSource);
            Assert.IsNotEmpty(_fakeOutput.ToString());
            Assert.AreEqual("0echo.lua1one2two3three4", _fakeOutput.ToString());
        }

        [Test]
        public void ExecuteFib()
        {
            var runtime = new LuaRuntime();
            runtime.Environment.ImportBasicFunctions();
            runtime.Environment.ImportSystemFunctions();
            runtime["print"] = (Action<VarArgs>)IoWrite;

            var arg = new Table();
            arg[1] = 24;
            runtime["arg"] = arg;

            var scriptSource = TestHelper.GetEmbeddedScript("fib.lua");
            runtime.AssertedExecuteScript(scriptSource);
            Assert.IsNotEmpty(_fakeOutput.ToString());
            Console.WriteLine(_fakeOutput.ToString());
        }
    }
}
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

using Antlr.Runtime;
using NUnit.Framework;
using PaniciSoftware.Tsuki.Common;

namespace PaniciSoftware.Tsuki.Test
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void BasicChunk_Section_3_3_2()
        {
            const string chunk = "local foo = 1;";
            var stream = new ANTLRStringStream(chunk);
            var lexer = new ChunkLexer(stream);
            var tStream = new CommonTokenStream(lexer);
            var parser = new ChunkParser(tStream);
            var result = parser.chunk();
            Assert.IsEmpty(parser.Errors);
        }

        [Test]
        public void CharactersCanBeEncodedByUnicodeEscapeSequence_Section_3_1()
        {
            const string chunk = "a = 'alo\n123\"'; b = \"alo\n123\\\"\"; c = '\\97lo\\10\\04923\"';";
            var stream = new ANTLRStringStream(chunk);
            var lexer = new ChunkLexer(stream);
            var tStream = new CommonTokenStream(lexer);
            var parser = new ChunkParser(tStream);
            var result = parser.chunk();
            Assert.IsEmpty(parser.Errors);
        }

        [Test]
        public void NumericConstantsCanBeInDecimalAndHex_Section_3_1()
        {
            const string chunk = "a = 128; b = 0x80; c = 255; d = 0xff; e = 0x0.1E; f = 0xA23p-4; g = 0X1.921FB54442D18P+1; h = 0.31416E1;";
            var stream = new ANTLRStringStream(chunk);
            var lexer = new ChunkLexer(stream);
            var tStream = new CommonTokenStream(lexer);
            var parser = new ChunkParser(tStream);
            var result = parser.chunk();
            Assert.IsEmpty(parser.Errors);
        }
    }
}
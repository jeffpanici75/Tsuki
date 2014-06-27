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
using Antlr.Runtime.Tree;

namespace PaniciSoftware.Tsuki.Common
{
    public static class ErrorListExtensions
    {
        public static void BadNumberFormat(this ErrorList l)
        {
        }

        public static void InvalidJumpStatement(this ErrorList l, int line, int col)
        {
            l.Add(
                Error.NewError(
                    "J999",
                    string.Format("Invalid jump found at line: {0} col: {1}", line, col),
                    "Jumps only valid with in loop control structures."));
        }

        public static void RedefinedLocalInSameScope(this ErrorList l, string name)
        {
            l.Add(
                Error.NewError(
                    "L999",
                    string.Format("Redefined local: {0} in scope.", name),
                    "Names must be unique within the same scope."));
        }

        public static void RuntimeError(this ErrorList l, Exception e)
        {
            l.Add(
                Error.NewError(
                    "R999",
                    e.ToString(),
                    e.StackTrace));
        }

        public static void CompileError(this ErrorList l, string s)
        {
            l.Add(
                Error.NewError(
                    "R999",
                    "Unknown compile error",
                    s));
        }

        public static void RecognitionError(this ErrorList l, string header, string message)
        {
            l.Add(
                Error.NewError(
                    "P999",
                    header,
                    message));
        }

        public static void UnsupportedEscapeFound(this ErrorList l, string name)
        {
            l.Add(
                Error.NewError(
                    "P999",
                    string.Format("Tsuki does not support esacpe sequnces of type: {0}", name),
                    "Unsupported escape sequence."));
        }

        public static void ASTError(this ErrorList l, CommonErrorNode node)
        {
            l.Add(
                Error.NewError(
                    "P999",
                    node.ToString(),
                    "Error in source file."));
        }

        public static void UnknownTokenType(this ErrorList l, int type)
        {
            l.Add(Error.NewError("T999", "Unknown token type.", type.ToString()));
        }
    }
}
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

namespace PaniciSoftware.Tsuki.Common
{
    public class Error
    {
        private readonly string _code;
        private readonly string _description;
        private readonly string _details;
        private readonly bool _isWarning;

        private Error(
            string code,
            string description,
            string details,
            bool isWarning)
        {
            _code = code;
            _details = details;
            _isWarning = isWarning;
            _description = description;
        }

        public string Code
        {
            get { return _code; }
        }

        public string Details
        {
            get { return _details; }
        }

        public bool IsWarning
        {
            get { return _isWarning; }
        }

        public string Description
        {
            get { return _description; }
        }

        public static Error NewError(
            string code,
            string description,
            Exception e)
        {
            var builder = new StringBuilder();
            builder.Append(e);
            if (e.InnerException != null)
            {
                builder.AppendFormat(
                    "\r\n\r\nInner Exception:\r\n---------------------------\r\n{0}\r\n",
                    e.InnerException);
            }
            builder.AppendFormat(
                "\r\n\r\nStack Trace:\r\n----------------------------\r\n{0}\r\n",
                e.StackTrace);
            return new Error(code, description, builder.ToString(), false);
        }

        public static Error NewError(
            string code,
            string description,
            string details)
        {
            return new Error(code, description, details, false);
        }

        public static Error NewWarning(
            string code,
            string description,
            string details)
        {
            return new Error(code, description, details, true);
        }

        public override string ToString()
        {
            return string.Format(
                "[{0}] {1}: {2}\n{3}",
                Code,
                IsWarning ? "WARNING" : "ERROR",
                Description,
                Details);
        }
    }
}
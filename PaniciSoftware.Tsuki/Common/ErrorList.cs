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
using System.Text;

namespace PaniciSoftware.Tsuki.Common
{
    public class ErrorList : List<Error>,
        ICloneable
    {
        public int ErrorCount
        {
            get { return FindAll(each => !each.IsWarning).Count; }
        }

        public int WarningCount
        {
            get { return FindAll(each => each.IsWarning).Count; }
        }

        public object Clone()
        {
            var newList = new ErrorList();
            newList.AddRange(this);
            return newList;
        }

        public bool ContainsCode(string code)
        {
            return Find(each => each.Code == code) != null;
        }

        public bool ContainsError()
        {
            return Find(each => !each.IsWarning) != null;
        }

        public bool ContainsWarning()
        {
            return Find(each => each.IsWarning) != null;
        }

        public int RemoveCode(string code)
        {
            return RemoveAll(each => each.Code == code);
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            foreach (var e in this)
            {
                buffer.AppendLine(e.ToString());
            }
            return buffer.ToString();
        }
    }
}
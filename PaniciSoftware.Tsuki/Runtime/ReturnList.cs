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

using System.Collections.Generic;
using PaniciSoftware.Tsuki.Common;

namespace PaniciSoftware.Tsuki.Runtime
{
    public class ReturnList : IExpandable
    {
        private readonly List<object> _values = new List<object>();

        public object this[int index]
        {
            get { return TryGetValue(index); }
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public List<object> ToList()
        {
            return new List<object>(_values);
        }

        public object TryGetValue(int index)
        {
            return index >= _values.Count ? null : _values[index];
        }

        public static ReturnList New(params object[] args)
        {
            var list = new ReturnList();

            foreach (var a in args)
                list.PushBack(a);

            return list;
        }

        public void PushBack(object o)
        {
            _values.Add(o);
        }

        public void AppendSelfToTable(Table t)
        {
            var index = t.SequenceLength + 1;

            foreach (var v in _values)
                t[index++] = v;
        }
    }
}
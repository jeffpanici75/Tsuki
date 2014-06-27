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
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using PaniciSoftware.Tsuki.Common;

namespace PaniciSoftware.Tsuki.Runtime
{
    public class Table : IDynamicMetaObjectProvider,
        IEnumerable<KeyValuePair<object, object>>
    {
        private readonly LinkedList<KeyValuePair<object, object>> _nextList = new LinkedList<KeyValuePair<object, object>>();

        private readonly IDictionary<object, LinkedListNode<KeyValuePair<object, object>>> _table = new Dictionary<object, LinkedListNode<KeyValuePair<object, object>>>();

        public MetaTable MetaTable { get; set; }

        public int Count
        {
            get { return _table.Count; }
        }

        public decimal SequenceLength
        {
            get
            {
                var index = 1M;

                foreach (var k in _table.Keys.Where(k => NumericHelper.IsNumeric(k) && _table[k].Value.Value != null))
                    index++;

                return index - 1;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public object this[object key]
        {
            get
            {
                LinkedListNode<KeyValuePair<object, object>> node;

                return _table.TryGetValue(key, out node) ? node.Value.Value : null;
            }
            set
            {
                if (!(key is decimal) && NumericHelper.IsNumeric(key))
                    key = Convert.ToDecimal(key);

                RawSetValue(key, value);
            }
        }

        public ICollection<object> Keys
        {
            get { return _table.Keys; }
        }

        public ICollection<object> Values
        {
            get { return _table.Values.Select(v => v.Value.Value).ToList(); }
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new TableMetaObject(parameter, BindingRestrictions.Empty, this);
        }

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            var d = _table.ToDictionary(kv => kv.Key, kv => kv.Value.Value.Value);
            return d.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Next(object key, out object nKey, out object nValue)
        {
            if (_table.Count < 1)
            {
                nKey = null;
                nValue = null;
                return false;
            }

            if (key == null)
            {
                var first = _nextList.First;
                nKey = first.Value.Key;
                nValue = first.Value.Value;
                return true;
            }

            LinkedListNode<KeyValuePair<object, object>> node;

            if (!_table.TryGetValue(key, out node))
            {
                nKey = null;
                nValue = null;
                return false;
            }

            var next = node.Next;

            if (next == null)
            {
                nKey = null;
                nValue = null;
                return false;
            }

            nKey = next.Value.Key;

            nValue = next.Value.Value;

            return true;
        }

        public void Clear()
        {
            _table.Clear();
        }

        public bool Remove(KeyValuePair<object, object> item)
        {
            return _table.Remove(item);
        }

        public bool ContainsKey(object key)
        {
            return _table.ContainsKey(key);
        }

        public void Add(
            object key,
            object value)
        {
            if (_table.ContainsKey(key))
                throw new ArgumentException(string.Format("Key: {0} already present in table.", key), "key");

            var kv = new KeyValuePair<object, object>(key, value);

            var node = _nextList.AddLast(kv);

            _table.Add(key, node);
        }

        public bool Remove(object key)
        {
            LinkedListNode<KeyValuePair<object, object>> node;

            if (_table.TryGetValue(key, out node))
            {
                _table.Remove(key);
                _nextList.Remove(node);
                return true;
            }

            return false;
        }

        public bool RawGetValue(
            object key,
            out object value)
        {
            LinkedListNode<KeyValuePair<object, object>> v;

            if (_table.TryGetValue(key, out v))
            {
                value = v.Value.Value;
                return true;
            }

            value = null;

            return false;
        }

        public void RawSetValue(
            object key,
            object value)
        {
            LinkedListNode<KeyValuePair<object, object>> extant;

            if (_table.TryGetValue(key, out extant))
                extant.Value = new KeyValuePair<object, object>(key, value);
            else
            {
                var kv = new KeyValuePair<object, object>(key, value);
                var node = _nextList.AddLast(kv);
                _table.Add(key, node);
            }
        }
    }
}
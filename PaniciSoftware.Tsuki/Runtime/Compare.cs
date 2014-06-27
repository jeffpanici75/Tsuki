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
using PaniciSoftware.Tsuki.Common;

namespace PaniciSoftware.Tsuki.Runtime
{
    public static class Compare
    {
        public static object ApplyLessThanOrEqual(object lhs, object rhs)
        {
            return Apply(lhs, rhs) <= 0;
        }

        public static object ApplyLessThan(object lhs, object rhs)
        {
            return Apply(lhs, rhs) < 0;
        }

        public static object ApplyEqual(object lhs, object rhs)
        {
            return Apply(lhs, rhs) == 0;
        }

        public static bool ApplyLessThanOrEqualBool(object lhs, object rhs)
        {
            return Apply(lhs, rhs) <= 0;
        }

        public static bool ApplyLessThanBool(object lhs, object rhs)
        {
            return Apply(lhs, rhs) < 0;
        }

        public static bool ApplyEqualBool(object lhs, object rhs)
        {
            return Apply(lhs, rhs) == 0;
        }

        public static int Apply(object lhs, object rhs)
        {
            if (lhs == null)
                return rhs == null ? 0 : -1;

            if (rhs == null)
                return -1;

            if (NumericHelper.IsNumeric(lhs.GetType()) && NumericHelper.IsNumeric(rhs.GetType()))
            {
                if (lhs is double)
                    return ((double) lhs).CompareTo(Convert.ToDouble(rhs));

                if (rhs is double)
                    return Convert.ToDouble(lhs).CompareTo((double) rhs);

                if (lhs is decimal)
                    return ((decimal) lhs).CompareTo(Convert.ToDecimal(rhs));

                if (rhs is decimal)
                    return Convert.ToDecimal(lhs).CompareTo((decimal) rhs);

                if (lhs is Int64)
                    return ((Int64) lhs).CompareTo(Convert.ToInt64(rhs));

                if (rhs is Int64)
                    return Convert.ToInt64(lhs).CompareTo((Int64) rhs);

                if (lhs is Int32)
                    return ((Int32) lhs).CompareTo(Convert.ToInt32(rhs));

                if (rhs is Int32)
                    return Convert.ToInt32(lhs).CompareTo((Int32) rhs);

                if (lhs is Int16)
                    return ((Int16) lhs).CompareTo(Convert.ToInt16(rhs));

                if (rhs is Int16)
                    return Convert.ToInt16(lhs).CompareTo((Int16) rhs);

                if (lhs is byte)
                    return ((byte) lhs).CompareTo(Convert.ToByte(rhs));

                if (rhs is byte)
                    return Convert.ToByte(lhs).CompareTo((byte) rhs);
            }

            if (lhs is string && rhs is string)
                return string.CompareOrdinal(((string) lhs), (string) rhs);

            if (lhs is bool && rhs is bool)
                return (bool) lhs == (bool) rhs ? 0 : -1;

            if (lhs.Equals(rhs))
                return 0;

            return -1;
        }
    }
}
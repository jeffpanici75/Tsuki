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
using System.Reflection;

namespace PaniciSoftware.Tsuki.Runtime
{
    public static class BinderHelper
    {
        public static bool GetConverter(
            Type t,
            Type source,
            out MethodInfo info)
        {
            if (t == null)
            {
                info = null;
                return true;
            }

            if (t == source)
            {
                info = null;
                return true;
            }

            if (t == typeof (double))
            {
                info = typeof (Convert).GetMethod(
                    "ToDouble",
                    new[]
                    {
                        source
                    });
            }
            else if (t == typeof (int))
            {
                info = typeof (Convert).GetMethod(
                    "ToInt32",
                    new[]
                    {
                        source
                    });
            }
            else if (t == typeof (Int64))
            {
                info = typeof (Convert).GetMethod(
                    "ToInt64",
                    new[]
                    {
                        source
                    });
            }
            else if (t == typeof (decimal))
            {
                info = typeof (Convert).GetMethod(
                    "ToDecimal",
                    new[]
                    {
                        source
                    });
            }
            else if (t == typeof (float))
            {
                info = typeof (Convert).GetMethod(
                    "ToSingle",
                    new[]
                    {
                        source
                    });
            }
            else
            {
                info = null;
                return false;
            }

            return true;
        }

        public static Type FindWinningNumericType(Type rhs, Type lhs)
        {
            if (rhs == typeof (decimal) || lhs == typeof (decimal))
                return typeof (decimal);

            if (rhs == typeof (double) || lhs == typeof (double))
                return typeof (double);

            if (rhs == typeof (long) || lhs == typeof (long))
                return typeof (long);

            if (rhs == typeof (int) || lhs == typeof (int))
                return typeof (int);

            return null;
        }

        public static Type GuessType(Type limitType, object value)
        {
            if (limitType == typeof (string))
            {
                var str = (string) value;

                if (str.StartsWith("0X") || str.StartsWith("0x"))
                    return str.Contains("P") || str.Contains("p") ? typeof (double) : typeof (decimal);

                return str.Contains("E") || str.Contains("e") ? typeof (double) : typeof (decimal);
            }

            return limitType;
        }
    }
}
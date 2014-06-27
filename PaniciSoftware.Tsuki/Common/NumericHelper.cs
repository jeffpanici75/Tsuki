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
using System.Linq.Expressions;
using System.Reflection;

namespace PaniciSoftware.Tsuki.Common
{
    public static class NumericHelper
    {
        public static bool IsNumeric(object s)
        {
            return s is int
                   || s is long
                   || s is Int16
                   || s is byte
                   || s is double
                   || s is decimal;
        }

        public static bool IsNumeric(Type s)
        {
            return s == typeof (int)
                   || s == typeof (long)
                   || s == typeof (Int16)
                   || s == typeof (byte)
                   || s == typeof (double)
                   || s == typeof (decimal);
        }

        public static Expression EmitToNumber(Expression ex)
        {
            var info = typeof (NumericHelper).GetMethod(
                "ToNumber",
                BindingFlags.Static | BindingFlags.Public);
            return Expression.Call(
                info,
                ex,
                Expression.Constant(
                    null,
                    typeof (object)));
        }

        public static object ToNumber(object s, object radix)
        {
            if (s == null) return null;

            int workingBase;
            if (radix == null)
            {
                if (IsNumeric(s))
                {
                    return s;
                }
                var str = s as string;
                return str != null ? ConvertFromLuaString(str) : null;
            }

            string workingValue;
            if (IsNumeric(s) || s is string)
            {
                workingValue = s.ToString();
            }
            else
            {
                return null;
            }

            try
            {
                workingBase = Convert.ToInt32(radix);
            }
            catch
            {
                return null;
            }

            var isNegative = workingValue[0] == '-';
            var hasPadding = workingValue[0] == '-' || workingValue[0] == '+';

            decimal top;
            if (!ToDecimalFromBase(
                workingValue.Substring(hasPadding ? 1 : 0),
                workingBase,
                out top))
            {
                return null;
            }
            return isNegative ? -top : top;
        }

        public static object ConvertFromLuaString(string str)
        {
            bool isHex;
            if (str.StartsWith("0X") || str.StartsWith("0x"))
            {
                isHex = true;
            }
            else
            {
                isHex = false;
            }

            if (str.Contains("."))
            {
                if (str.Contains(isHex ? "P" : "E")
                    || str.Contains(isHex ? "p" : "e"))
                {
                    double d;
                    if (isHex && ToDoubleFromHexExponent(str, out d))
                    {
                        return d;
                    }
                    if (double.TryParse(str, out d))
                    {
                        return d;
                    }
                    return null;
                }
                {
                    decimal d;
                    if (isHex && ToDecimalFromHexFloat(str, out d))
                    {
                        return d;
                    }
                    if (decimal.TryParse(str, out d))
                    {
                        return d;
                    }
                    return null;
                }
            }

            if (isHex)
            {
                try
                {
                    return Convert.ToDecimal(
                        Convert.ToInt32(
                            str,
                            16));
                }
                catch (OverflowException ex)
                {
                    try
                    {
                        return Convert.ToDecimal(
                            Convert.ToInt64(
                                str,
                                16));
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }

            {
                decimal d;
                if (decimal.TryParse(str, out d))
                {
                    return d;
                }
                return null;
            }
        }

        public static bool ToDecimalFromHexFloat(string raw, out decimal d)
        {
            try
            {
                var parts = raw.Split(
                    new[]
                    {
                        '.'
                    });
                var up = (decimal) Convert.ToInt32(parts[0], 16);
                var down = (decimal) Convert.ToInt32(parts[1], 16);
                while (down > 1) down /= 10;
                d = up + down;
                return true;
            }
            catch (Exception e)
            {
                d = 0;
                return false;
            }
        }

        public static bool ToDoubleFromHexExponent(string raw, out double d)
        {
            try
            {
                var mainParts = raw.Split(
                    raw.Contains("P")
                        ? new[]
                        {
                            'P'
                        }
                        : new[]
                        {
                            'p'
                        });
                var mantisaStr = mainParts[0];
                double mantisa;
                if (mantisaStr.Contains("."))
                {
                    var parts = mantisaStr.Split(
                        new[]
                        {
                            '.'
                        });
                    var up = (double) Convert.ToInt64(parts[0], 16);
                    var down = (double) Convert.ToInt64(parts[1], 16);
                    while (down > 1) down /= 10;
                    mantisa = up + down;
                }
                else
                {
                    mantisa = Convert.ToInt32(mantisaStr, 16);
                }
                var exponentText = mainParts[1];
                var sign = exponentText.StartsWith("+") || !exponentText.StartsWith("-"); //true for positive
                if (exponentText.StartsWith("+") || exponentText.StartsWith("-"))
                {
                    exponentText = exponentText.Substring(1);
                }

                double exponent;
                if (sign)
                {
                    exponent = +Convert.ToInt64(exponentText, 16);
                }
                else
                {
                    exponent = -Convert.ToInt64(exponentText, 16);
                }

                d = Math.Pow(mantisa, exponent);
                return true;
            }
            catch (Exception)
            {
                d = 0;
                return false;
            }
        }

        private static bool ToDecimalFromBase(string str, int radix, out decimal r)
        {
            var accumulator = 0;
            var exp = 0;
            for (var i = str.Length - 1; i >= 0; i--)
            {
                var digitChar = str[i];
                var n = (int) digitChar;
                int adj;
                if (0x0030 <= n && n <= 0x0039)
                {
                    //0-9
                    adj = n - 0x0030;
                }
                else if (0x0041 <= n && n <= 0x005A)
                {
                    //upper case
                    adj = n - 0x0037;
                }
                else if (0x0061 <= n && n <= 0x007A)
                {
                    //lower case
                    adj = n - 0x0057;
                }
                else
                {
                    r = 0;
                    return false;
                }
                accumulator += ((int) Math.Pow(radix, exp))*adj;
                exp++;
            }
            r = Convert.ToDecimal(accumulator);
            return true;
        }
    }
}
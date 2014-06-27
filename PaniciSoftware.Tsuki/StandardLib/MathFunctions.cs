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
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.StandardLib
{
    public static class MathFunctions
    {
        public static void ImportMathFunctions(this Table t)
        {
            var table = new Table();
            table["huge"] = decimal.MaxValue;
            table["math"] = table;
            LuaExportAttribute.AssignExportedFunctions(typeof (MathFunctions), t);
        }

        [LuaExport(Name = "math.abs")]
        internal static object Abs(object o)
        {
            if (o is decimal) return Math.Abs((decimal) o);
            if (o is double) return Math.Abs((double) o);
            if (o is int) return Math.Abs((int) o);
            return Math.Abs(Convert.ToDecimal(o));
        }

        [LuaExport(Name = "math.acos")]
        internal static double Acos(object o)
        {
            return Math.Acos(Convert.ToDouble(o));
        }

        [LuaExport(Name = "math.asin")]
        internal static double Asin(object o)
        {
            return Math.Asin(Convert.ToDouble(o));
        }

        [LuaExport(Name = "math.atan")]
        internal static double Atan(object o)
        {
            return Math.Atan(Convert.ToDouble(o));
        }

        [LuaExport(Name = "math.atan2")]
        internal static double Atan2(object y, object x)
        {
            return Math.Atan2(Convert.ToDouble(y), Convert.ToDouble(x));
        }

        [LuaExport(Name = "math.ceil")]
        internal static object Ceil(object o)
        {
            if (o is decimal) return Math.Ceiling((decimal) o);
            if (o is double) return Math.Ceiling((double) o);
            return Math.Ceiling(Convert.ToDecimal(o));
        }

        [LuaExport(Name = "math.cos")]
        internal static double Cos(object o)
        {
            return Math.Cos(Convert.ToDouble(o));
        }

        [LuaExport(Name = "math.cosh")]
        internal static double Cosh(object o)
        {
            return Math.Cosh(Convert.ToDouble(o));
        }

        [LuaExport(Name = "math.deg")]
        internal static double Deg(object o)
        {
            return RadianToDegree(Convert.ToDouble(o));
        }

        [LuaExport(Name = "math.exp")]
        internal static double Exp(object o)
        {
            return Math.Exp(Convert.ToDouble(o));
        }

        [LuaExport(Name = "math.floor")]
        internal static object Floor(object o)
        {
            if (o is decimal) return Math.Floor((decimal) o);
            if (o is double) return Math.Floor((double) o);
            return Math.Floor(Convert.ToDecimal(o));
        }

        [LuaExport(Name = "math.fmod")]
        internal static object Fmod(object x, object y)
        {
            return Math.IEEERemainder(Convert.ToDouble(x), Convert.ToDouble(y));
        }

        private static double DegreeToRadian(double angle)
        {
            return Math.PI*angle/180.0;
        }

        private static double RadianToDegree(double angle)
        {
            return angle*(180.0/Math.PI);
        }
    }
}
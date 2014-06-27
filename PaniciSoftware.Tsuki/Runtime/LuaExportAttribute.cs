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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using PaniciSoftware.Tsuki.Common;

namespace PaniciSoftware.Tsuki.Runtime
{
    public class LuaExportAttribute : Attribute
    {
        public string Name { get; set; }

        public static void AssignExportedFunctions(Type type, Table table, object instance = null)
        {
            var exportInfos = GetExportInfos(type);

            foreach (var each in exportInfos)
            {
                foreach (var eachAttribute in each.Attributes)
                {
                    var currentTable = table;

                    var namespaceParts = eachAttribute.GetNamespaceParts();
                    for (var i = 0; i < namespaceParts.Count - 1; i++)
                    {
                        var eachTableSpace = namespaceParts[i];

                        if (currentTable.ContainsKey(eachTableSpace))
                        {
                            currentTable = (Table) currentTable[eachTableSpace];
                            continue;
                        }

                        var newTable = new Table();

                        currentTable[eachTableSpace] = newTable;

                        currentTable = newTable;
                    }

                    if (!each.Method.IsStatic && instance == null)
                        throw new ArgumentException("Unable to load non static method with out an instance.");

                    currentTable[namespaceParts.Last()] = new NativeFunction
                    {
                        Info = each.Method, 
                        Instance = instance
                    };
                }
            }
        }

        public static ReadOnlyCollection<LuaExportInfo> GetExportInfos(Assembly assembly)
        {
            var functionInfos = new List<LuaExportInfo>();

            foreach (var eachType in assembly.GetTypes())
                functionInfos.AddRange(GetExportInfos(eachType));

            return functionInfos.AsReadOnly();
        }

        public static ReadOnlyCollection<LuaExportInfo> GetExportInfos(Type type)
        {
            var functionInfos = new List<LuaExportInfo>();

            foreach (var eachMethod in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var descriptionAttribute = (DescriptionAttribute) GetCustomAttribute(
                    eachMethod,
                    typeof (DescriptionAttribute));

                var obsoleteAttribute = (ObsoleteAttribute) GetCustomAttribute(
                    eachMethod,
                    typeof (ObsoleteAttribute));

                var kernelFunctionAttributes = (LuaExportAttribute[]) GetCustomAttributes(
                    eachMethod,
                    typeof (LuaExportAttribute));

                if (kernelFunctionAttributes.Length > 0)
                {
                    var info = new LuaExportInfo
                    {
                        Method = eachMethod,
                        Description = descriptionAttribute != null ? descriptionAttribute.Description : null,
                        DeprecationWarning = obsoleteAttribute != null ? obsoleteAttribute.Message : null
                    };

                    info.InnerAttributes.AddRange(kernelFunctionAttributes);

                    functionInfos.Add(info);
                }
            }

            return functionInfos.AsReadOnly();
        }

        public ReadOnlyCollection<string> GetNamespaceParts()
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(Name))
            {
                var nameParts = Name.Split(
                    ".".ToCharArray(),
                    StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < nameParts.Length; i++)
                    parts.Add(nameParts[i]);
            }

            return parts.AsReadOnly();
        }
    }
}
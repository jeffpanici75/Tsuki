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
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PaniciSoftware.Tsuki.Runtime
{
    public class InvokeMemberBinder : System.Dynamic.InvokeMemberBinder
    {
        private readonly StaticMetaTables _metaTables;

        public InvokeMemberBinder(
            StaticMetaTables metaTables, 
            string name, 
            CallInfo callInfo) : base(name, false, callInfo)
        {
            _metaTables = metaTables;
        }

        public static InvokeMemberBinder New(
            StaticMetaTables metaTables, 
            string name, 
            CallInfo info)
        {
            return new InvokeMemberBinder(metaTables, name, info);
        }

        public override DynamicMetaObject FallbackInvokeMember(
            DynamicMetaObject target, 
            DynamicMetaObject[] args, 
            DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue || args.Any((a) => !a.HasValue))
            {
                var deferArgs = new DynamicMetaObject[args.Length + 1];

                for (var i = 0; i < args.Length; i++)
                    deferArgs[i + 1] = args[i];

                deferArgs[0] = target;

                return Defer(deferArgs);
            }

            // Find our own binding.
            // Could consider allowing invoking static members from an instance.
            const BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public;

            var members = target.LimitType.GetMember(Name, flags);
            if ((members.Length == 1) && (members[0] is PropertyInfo || members[0] is FieldInfo))
            {
                // NEED TO TEST, should check for delegate value too
                var mem = members[0];
                throw new NotImplementedException();
                //return new DynamicMetaObject(
                //    Expression.Dynamic(
                //        new SymplInvokeBinder(new CallInfo(args.Length)),
                //        typeof(object),
                //        args.Select(a => a.Expression).AddFirst(
                //               Expression.MakeMemberAccess(this.Expression, mem)));

                // Don't test for eventinfos since we do nothing with them now.
            }
            else
            {
                // Get MethodInfos with right arg counts.
                var methodInfos = members.
                    Select(m => m as MethodInfo).
                    Where(m => m.GetParameters().Length == args.Length);

                // Get MethodInfos with param types that work for args.  This works
                // except for value args that need to pass to reftype params. 
                // We could detect that to be smarter and then explicitly StrongBox
                // the args.
                var res = new List<MethodInfo>();

                foreach (var mem in methodInfos)
                {
                    if (RuntimeHelper.ParametersMatchArguments(mem.GetParameters(), args))
                        res.Add(mem);
                }

                // False below means generate a type restriction on the MO.
                // We are looking at the members target's Type.
                var restrictions = RuntimeHelper.GetTargetArgsRestrictions(
                    target,
                    args,
                    false);

                if (res.Count == 0)
                {
                    return errorSuggestion ??
                           RuntimeHelper.CreateThrow(
                               target,
                               args,
                               restrictions,
                               typeof (MissingMemberException),
                               "Can't bind member invoke -- " + args.ToString());
                }

                // Could have tried just letting Expr.Call factory do the work,
                // but if there is more than one applicable method using just
                // assignablefrom, Expr.Call throws.  It does not pick a "most
                // applicable" method or any method.

                var callArgs = new Expression[0];
                return new DynamicMetaObject(
                    RuntimeHelper.EnsureObjectResult(
                        Expression.Call(
                            Expression.Convert(target.Expression, target.LimitType),
                            res[0],
                            callArgs)),
                    restrictions);
            }
        }

        public override DynamicMetaObject FallbackInvoke(
            DynamicMetaObject target,
            DynamicMetaObject[] args,
            DynamicMetaObject errorSuggestion)
        {
            var argexprs = new Expression[args.Length + 1];

            for (var i = 0; i < args.Length; i++)
                argexprs[i + 1] = args[i].Expression;

            argexprs[0] = target.Expression;

            return new DynamicMetaObject(
                Expression.Dynamic(
                    InvokeBinder.New(_metaTables, new CallInfo(argexprs.Length)),
                    typeof (object),
                    argexprs),
                target.Restrictions);
        }
    }
}
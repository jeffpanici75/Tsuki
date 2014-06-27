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
using Antlr.Runtime.Tree;
using PaniciSoftware.Tsuki.Common;
using PaniciSoftware.Tsuki.Runtime;

namespace PaniciSoftware.Tsuki.Compiler
{
    public class Exp : Generator
    {
        protected override Expression OnGenerate()
        {
            return OrExp(Tree);
        }

        private Expression OrExp(CommonTree tree)
        {
            if (CheckError(tree))
                return Expression.Empty();

            Expression e;
            if (TryPrimary(tree, out e))
                return e;

            switch (tree.Type)
            {
                case ChunkParser.OrOp:
                {
                    var param = Expression.Parameter(typeof (object));
                    var lhs = Wrap(OrExp((CommonTree) tree.Children[0]));
                    var assignTo = Expression.Assign(param, lhs);
                    var rhs = Wrap(OrExp((CommonTree) tree.Children[1]));
                    var cond = Expression.Condition(ToBool(param), CastTo<object>(param), CastTo<object>(rhs));
                    return Expression.Block(
                        typeof (object),
                        new[]
                        {
                            param
                        },
                        assignTo,
                        cond);
                }
                default:
                {
                    return AndExp(tree);
                }
            }
        }

        private Expression AndExp(CommonTree tree)
        {
            if (CheckError(tree))
                return Expression.Empty();

            Expression e;
            if (TryPrimary(tree, out e))
                return e;

            switch (tree.Type)
            {
                case ChunkParser.AndOp:
                {
                    var param = Expression.Parameter(typeof (object));
                    var lhs = Wrap(AndExp((CommonTree) tree.Children[0]));
                    var assignTo = Expression.Assign(param, lhs);
                    var rhs = Wrap(AndExp((CommonTree) tree.Children[1]));
                    var cond = Expression.Condition(ToBool(param), CastTo<object>(rhs), CastTo<object>(param));
                    return Expression.Block(
                        typeof (object),
                        new[]
                        {
                            param
                        },
                        assignTo,
                        cond);
                }
                default:
                {
                    return Equality(tree);
                }
            }
        }

        private Expression Equality(CommonTree tree)
        {
            if (CheckError(tree))
                return Expression.Empty();

            Expression e;
            if (TryPrimary(tree, out e))
                return e;

            switch (tree.Type)
            {
                case ChunkParser.NEQ:
                {
                    var lhs = Wrap(Equality((CommonTree) tree.Children[0]));
                    var rhs = Wrap(Equality((CommonTree) tree.Children[1]));
                    return Expression.Not(
                        ToBool(
                            Expression.Dynamic(
                                EqualityOperationBinder.New(StaticTables),
                                typeof (object),
                                lhs,
                                rhs)));
                }
                case ChunkParser.EQ:
                {
                    var lhs = Wrap(Equality((CommonTree) tree.Children[0]));
                    var rhs = Wrap(Equality((CommonTree) tree.Children[1]));
                    return Expression.Dynamic(
                        EqualityOperationBinder.New(StaticTables),
                        typeof (object),
                        lhs,
                        rhs);
                }
                default:
                {
                    return Relational(tree);
                }
            }
        }

        private Expression Relational(CommonTree tree)
        {
            if (CheckError(tree))
                return Expression.Empty();

            Expression e;
            if (TryPrimary(tree, out e))
                return e;

            switch (tree.Type)
            {
                case ChunkParser.LE:
                {
                    var lhs = Wrap(Relational((CommonTree) tree.Children[0]));
                    var rhs = Wrap(Relational((CommonTree) tree.Children[1]));
                    return Expression.Dynamic(
                        LessThanOrEqualBinder.New(StaticTables),
                        typeof (object),
                        lhs,
                        rhs);
                }
                case ChunkParser.GE:
                {
                    var lhs = Wrap(Relational((CommonTree) tree.Children[0]));
                    var rhs = Wrap(Relational((CommonTree) tree.Children[1]));
                    return Expression.Dynamic(
                        LessThanOrEqualBinder.New(StaticTables),
                        typeof (object),
                        rhs,
                        lhs);
                }
                case ChunkParser.LT:
                {
                    var lhs = Wrap(Relational((CommonTree) tree.Children[0]));
                    var rhs = Wrap(Relational((CommonTree) tree.Children[1]));
                    return Expression.Dynamic(
                        LessThanBinder.New(StaticTables),
                        typeof (object),
                        lhs,
                        rhs);
                }
                case ChunkParser.GT:
                {
                    var lhs = Wrap(Relational((CommonTree) tree.Children[0]));
                    var rhs = Wrap(Relational((CommonTree) tree.Children[1]));
                    return Expression.Dynamic(
                        LessThanBinder.New(StaticTables),
                        typeof (object),
                        rhs,
                        lhs);
                }
                default:
                {
                    return Concatenation(tree);
                }
            }
        }

        private Expression Concatenation(CommonTree tree)
        {
            if (CheckError(tree))
                return Expression.Empty();

            Expression e;
            if (TryPrimary(tree, out e))
                return e;

            switch (tree.Type)
            {
                case ChunkParser.ConcatenateOp:
                {
                    var lhs = Wrap(Concatenation((CommonTree) tree.Children[0]));
                    var rhs = Wrap(Concatenation((CommonTree) tree.Children[1]));
                    return Expression.Dynamic(
                        ConcatenationBinder.New(StaticTables),
                        typeof (object),
                        lhs,
                        rhs);
                }
                default:
                {
                    return Add(tree);
                }
            }
        }

        private Expression Add(CommonTree tree)
        {
            if (CheckError(tree))
                return Expression.Empty();

            Expression e;
            if (TryPrimary(tree, out e))
                return e;

            switch (tree.Type)
            {
                case ChunkParser.AddOp:
                case ChunkParser.MinusOp:
                {
                    var lhs = Wrap(Add((CommonTree) tree.Children[0]));
                    var rhs = Wrap(Add((CommonTree) tree.Children[1]));
                    return Expression.Dynamic(
                        NumericOperationBinder.New(StaticTables, TokenToType(tree.Type)),
                        typeof (object),
                        lhs,
                        rhs);
                }
                default:
                {
                    return Mul(tree);
                }
            }
        }

        private Expression Mul(CommonTree tree)
        {
            if (CheckError(tree))
                return Expression.Empty();

            Expression e;
            if (TryPrimary(tree, out e))
                return e;

            switch (tree.Type)
            {
                case ChunkParser.ModOp:
                case ChunkParser.MultiplyOp:
                case ChunkParser.DivOp:
                {
                    var lhs = Wrap(Mul((CommonTree) tree.Children[0]));
                    var rhs = Wrap(Mul((CommonTree) tree.Children[1]));
                    return Expression.Dynamic(
                        NumericOperationBinder.New(StaticTables, TokenToType(tree.Type)),
                        typeof (object),
                        lhs,
                        rhs);
                }
                default:
                {
                    return Unary(tree);
                }
            }
        }

        private Expression Unary(CommonTree tree)
        {
            if (CheckError(tree))
                return Expression.Empty();

            Expression e;
            if (TryPrimary(tree, out e))
                return e;

            switch (tree.Type)
            {
                case ChunkParser.NotOp:
                {
                    var operand = Wrap(Unary((CommonTree) tree.Children[0]));
                    return Expression.Dynamic(
                        UnaryOperationBinder.New(StaticTables, TokenToType(tree.Type)),
                        typeof (bool),
                        operand);
                }
                case ChunkParser.UnaryMinusOp:
                {
                    var operand = Wrap(Unary((CommonTree) tree.Children[0]));
                    return Expression.Dynamic(
                        UnaryOperationBinder.New(StaticTables, TokenToType(tree.Type)),
                        typeof (object),
                        operand);
                }
                case ChunkParser.LengthOp:
                {
                    var operand = Wrap(Unary((CommonTree) tree.Children[0]));
                    return Expression.Dynamic(
                        LengthBinder.New(StaticTables),
                        typeof (object),
                        operand);
                }
                default:
                {
                    return Power(tree);
                }
            }
        }

        private Expression Power(CommonTree tree)
        {
            if (CheckError(tree))
                return Expression.Empty();

            Expression e;
            if (TryPrimary(tree, out e))
                return e;

            switch (tree.Type)
            {
                case ChunkParser.PowerOp:
                {
                    var lhs = Wrap(Power((CommonTree) tree.Children[0]));
                    var rhs = Wrap(Power((CommonTree) tree.Children[1]));
                    return Expression.Dynamic(
                        NumericOperationBinder.New(StaticTables, ExpressionType.Power),
                        typeof (object),
                        lhs,
                        rhs);
                }
                default:
                {
                    throw new InvalidOperationException(string.Format("Unknown operator type: {0}", tree.Type));
                }
            }
        }

        private bool TryPrimary(CommonTree tree, out Expression e)
        {
            switch (tree.Type)
            {
                case ChunkParser.Nil:
                {
                    e = Expression.Constant(null, typeof (object));
                    return true;
                }
                case ChunkParser.True:
                {
                    e = Expression.Constant(true, typeof (bool));
                    return true;
                }
                case ChunkParser.False:
                {
                    e = Expression.Constant(false, typeof (bool));
                    return true;
                }
                case ChunkParser.CharString:
                case ChunkParser.String:
                {
                    var str = tree.Text;
                    e = Expression.Constant(str, typeof (string));
                    return true;
                }
                case ChunkParser.LongBracket:
                {
                    var str = tree.Text;
                    var trimmed = TrimLongBracket(str);
                    e = Expression.Constant(trimmed, typeof (string));
                    return true;
                }
                case ChunkParser.Int:
                {
                    e = Expression.Constant(decimal.Parse(tree.Text), typeof (decimal));
                    return true;
                }
                case ChunkParser.Exponent:
                {
                    e = Expression.Constant(double.Parse(tree.Text), typeof (double));
                    return true;
                }
                case ChunkParser.Float:
                {
                    e = Expression.Constant(decimal.Parse(tree.Text), typeof (decimal));
                    return true;
                }
                case ChunkParser.Hex:
                {
                    try
                    {
                        e = Expression.Constant(
                            Convert.ToDecimal(
                                Convert.ToInt32(
                                    Tree.Text.Substring(2),
                                    16)),
                            typeof (decimal));
                    }
                    catch (OverflowException)
                    {
                        e = Expression.Constant(
                            Convert.ToDecimal(
                                Convert.ToInt64(
                                    Tree.Text.Substring(2),
                                    16)),
                            typeof (decimal));
                    }
                    return true;
                }
                case ChunkParser.HexExponent:
                {
                    double d;
                    if (NumericHelper.ToDoubleFromHexExponent(Tree.Text, out d))
                    {
                        e = Expression.Constant(d, typeof (double));
                        return true;
                    }
                    Errors.BadNumberFormat();
                    e = Expression.Constant(
                        null,
                        typeof (object));
                    return true;
                }
                case ChunkParser.HexFloat:
                {
                    decimal d;
                    if (NumericHelper.ToDecimalFromHexFloat(Tree.Text, out d))
                    {
                        e = Expression.Constant(
                            NumericHelper.ToDecimalFromHexFloat(Tree.Text, out d),
                            typeof (decimal));
                        return true;
                    }
                    Errors.BadNumberFormat();
                    e = Expression.Constant(null, typeof (object));
                    return true;
                }
                case ChunkParser.AnonDefun:
                {
                    e = Gen<AnonDefun>(tree);
                    return true;
                }
                case ChunkParser.TableDef:
                {
                    e = Gen<TableDef>(tree);
                    return true;
                }
                case ChunkParser.Prefix:
                {
                    e = Gen<Prefix>(tree);
                    return true;
                }
                case ChunkParser.NestedPrefix:
                {
                    e = Gen<Prefix>(tree);
                    return true;
                }
                default:
                {
                    e = null;
                    return false;
                }
            }
        }

        private static string TrimLongBracket(string trimmed)
        {
            if (trimmed.StartsWith("\n"))
            {
                return trimmed.Substring(1);
            }

            if (trimmed.StartsWith("\r\n"))
            {
                return trimmed.Substring(2);
            }
            return trimmed;
        }

        private static Expression Wrap(Expression ex)
        {
            return RuntimeHelper.EnsureObjectResult(RValueList.EmitNarrow(ex));
        }

        private static ExpressionType TokenToType(int token)
        {
            switch (token)
            {
                case ChunkParser.EQ:
                    return ExpressionType.Equal;
                case ChunkParser.NEQ:
                    return ExpressionType.NotEqual;
                case ChunkParser.AddOp:
                    return ExpressionType.Add;
                case ChunkParser.MinusOp:
                    return ExpressionType.Subtract;
                case ChunkParser.MultiplyOp:
                    return ExpressionType.Multiply;
                case ChunkParser.DivOp:
                    return ExpressionType.Divide;
                case ChunkParser.ModOp:
                    return ExpressionType.Modulo;
                case ChunkParser.LT:
                    return ExpressionType.LessThan;
                case ChunkParser.GT:
                    return ExpressionType.GreaterThan;
                case ChunkParser.LE:
                    return ExpressionType.LessThanOrEqual;
                case ChunkParser.GE:
                    return ExpressionType.GreaterThanOrEqual;
                case ChunkParser.UnaryMinusOp:
                    return ExpressionType.Negate;
                case ChunkParser.NotOp:
                    return ExpressionType.Not;
                default:
                    throw new InvalidOperationException("Unknown operation type");
            }
        }
    }
}
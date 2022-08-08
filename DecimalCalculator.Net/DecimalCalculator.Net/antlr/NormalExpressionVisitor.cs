using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DecimalCalculator.NET.antlr;

namespace DecimalCalculator.Net.antlr
{
    public class NormalExpressionVisitor : AbstractParseTreeVisitor<Expression>, IcalculatorVisitor<Expression>
    {
        private ParameterExpression _arg;
        public NormalExpressionVisitor()
        {
            _arg = Expression.Parameter(typeof(IDictionary<string, decimal>), "bindings");
        }

        public ParameterExpression Arg => _arg;

        public Expression VisitAtom([NotNull] calculatorParser.AtomContext ctx)
        {
            var len_children = ctx.ChildCount;
            if (len_children == 1)
            {
                var constant = ctx.constant();
                if (constant != null)
                {
                    return constant.Accept(this);
                }
                var scientific = ctx.scientific();
                if (scientific != null)
                {
                    return scientific.Accept(this);
                }
                var variable = ctx.variable();
                if (variable != null)
                {
                    return variable.Accept(this);
                }
                return Expression.Empty();
            }
            var exp = ctx.expression();
            return exp.Accept(this);
        }

        public Expression VisitConstant([NotNull] calculatorParser.ConstantContext ctx)
        {
            var txt = ctx.GetText();
            if (txt == "pi")
            {
                return Expression.Constant((decimal)Math.PI);
            }
            else if (txt == "e")
            {
                return Expression.Constant((decimal)Math.E);
            }
            return Expression.Empty();
        }

        public Expression VisitExpression([NotNull] calculatorParser.ExpressionContext ctx)
        {
            var len_children = ctx.ChildCount;
            var dec0 = ctx.GetChild(0).Accept(this);
            if (len_children == 1)
            {
                return dec0;
            }
            var count = (len_children - 1) / 2;
            for (var i = 0; i < count; ++i)
            {
                var op_idx = 2 * i + 1;
                var c_idx = 2 * i + 2;
                var dec = ctx.GetChild(c_idx).Accept(this);
                var op = ctx.GetChild(op_idx).GetText();
                if (op == "+")
                {
                    dec0 = Expression.Add(dec0, dec);
                }
                else if (op == "-")
                {
                    dec0 = Expression.Subtract(dec0, dec);
                }
            }
            return dec0;
        }

        public Expression VisitFunc_([NotNull] calculatorParser.Func_Context ctx)
        {
            var funcname = ctx.GetChild(0).GetText();
            var arg1 = ctx.GetChild(2).Accept(this);
            MethodInfo method = null;
            switch (funcname)
            {
                case "cos":
                    method = typeof(MathDecimal).GetMethod("Cos", new[] { typeof(decimal) });
                    break;
                case "tan":
                    method = typeof(MathDecimal).GetMethod("Tan", new[] { typeof(decimal) });
                    break;
                case "sin":
                    method = typeof(MathDecimal).GetMethod("Sin", new[] { typeof(decimal) });
                    break;
                case "acos":
                    method = typeof(MathDecimal).GetMethod("Acos", new[] { typeof(decimal) });
                    break;
                case "atan":
                    method = typeof(MathDecimal).GetMethod("Atan", new[] { typeof(decimal) });
                    break;
                case "asin":
                    method = typeof(MathDecimal).GetMethod("Asin", new[] { typeof(decimal) });
                    break;
                case "log":
                    method = typeof(MathDecimal).GetMethod("Log", new[] { typeof(decimal) });
                    break;
                case "ln":
                    method = typeof(MathDecimal).GetMethod("Ln", new[] { typeof(decimal) });
                    break;
                case "sqrt":
                    method = typeof(MathDecimal).GetMethod("Sqrt", new[] { typeof(decimal) });
                    break;
                case "floor":
                    method = typeof(decimal).GetMethod("Floor", new[] { typeof(decimal) });
                    break;
                case "ceil":
                    method = typeof(decimal).GetMethod("Ceiling", new[] { typeof(decimal) });
                    break;
                case "round":
                    method = typeof(decimal).GetMethod("Round", new[] { typeof(decimal) });
                    break;
                case "round2":
                    method = typeof(MathDecimal).GetMethod("Round2", new[] { typeof(decimal) });
                    break;

                default:
                    return Expression.Empty();
            }
            return Expression.Call(method, arg1);
        }

        public Expression VisitMultiplyingExpression([NotNull] calculatorParser.MultiplyingExpressionContext ctx)
        {
            var len_children = ctx.ChildCount;
            var dec0 = ctx.GetChild(0).Accept(this);
            if (len_children == 1)
            {
                return dec0;
            }
            var count = (len_children - 1) / 2;
            for (var i = 0; i < count; ++i)
            {
                var op_idx = 2 * i + 1;
                var c_idx = 2 * i + 2;
                var dec = ctx.GetChild(c_idx).Accept(this);
                var op = ctx.GetChild(op_idx).GetText();
                if (op == "*")
                {
                    dec0 = Expression.Multiply(dec0, dec);
                }
                else if (op == "/")
                {
                    dec0 = Expression.Divide(dec0, dec);
                }
            }
            return dec0;
        }

        public Expression VisitPowExpression([NotNull] calculatorParser.PowExpressionContext ctx)
        {
            var len_children = ctx.ChildCount;
            var dec0 = ctx.GetChild(0).Accept(this);
            if (len_children == 1)
            {
                return dec0;
            }
            var count = (len_children - 1) / 2;
            for (var i = 0; i < count; ++i)
            {
                var c_idx = 2 * i + 2;
                var dec = ctx.GetChild(c_idx).Accept(this);
                var method = typeof(MathDecimal).GetMethod("Pow", new[] { typeof(decimal), typeof(decimal) });
                dec0 = Expression.Call(method, dec0, dec);
            }
            return dec0;
        }

        public Expression VisitScientific([NotNull] calculatorParser.ScientificContext ctx)
        {
            var txt = ctx.GetText();
            var val = decimal.Parse(txt, System.Globalization.NumberStyles.Float);
            return Expression.Constant(val);
        }

        public Expression VisitSignedAtom([NotNull] calculatorParser.SignedAtomContext ctx)
        {
            var len_children = ctx.ChildCount;
            if (len_children == 1)
            {
                var func_ = ctx.func_();
                if (func_ != null)
                {
                    return func_.Accept(this);
                }
                var atom = ctx.atom();
                if (atom != null)
                {
                    return atom.Accept(this);
                }
                return Expression.Empty();
            }
            var sign = ctx.GetChild(0).GetText();
            var signedAtom = ctx.signedAtom().Accept(this);
            if (sign == "-")
            {
                return Expression.Negate(signedAtom);
            }
            return signedAtom;
        }

        public Expression VisitVariable([NotNull] calculatorParser.VariableContext ctx)
        {
            var txt = ctx.GetText();
            return Expression.Property(_arg, "Item", Expression.Constant(txt));
        }
    }

    public static class MathDecimal
    {
        public static decimal Cos(decimal arg)
        {
            return (decimal)Math.Cos((double)arg);
        }

        public static decimal Tan(decimal arg)
        {
            return (decimal)Math.Tan((double)arg);
        }

        public static decimal Sin(decimal arg)
        {
            return (decimal)Math.Sin((double)arg);
        }

        public static decimal Acos(decimal arg)
        {
            return (decimal)Math.Acos((double)arg);
        }

        public static decimal Atan(decimal arg)
        {
            return (decimal)Math.Atan((double)arg);
        }

        public static decimal Asin(decimal arg)
        {
            return (decimal)Math.Asin((double)arg);
        }

        public static decimal Log(decimal arg)
        {
            return (decimal)Math.Log10((double)arg);
        }

        public static decimal Ln(decimal arg)
        {
            return (decimal)Math.Log((double)arg);
        }

        public static decimal Sqrt(decimal arg)
        {
            return (decimal)Math.Sqrt((double)arg);
        }

        public static decimal Round2(decimal arg)
        {
            return decimal.Ceiling(arg - 0.5M);
        }

        public static decimal Pow(decimal arg1, decimal arg2)
        {
            return (decimal)Math.Pow((double)arg1, (double)arg2);
        }
    }
}


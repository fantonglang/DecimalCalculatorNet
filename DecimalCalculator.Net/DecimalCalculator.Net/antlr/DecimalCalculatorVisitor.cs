using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.Linq;

namespace DecimalCalculator.NET.antlr
{
    public class DecimalCalculatorVisitor : AbstractParseTreeVisitor<decimal>, IcalculatorVisitor<decimal>
    {
        private Dictionary<string, decimal> _bindings = new Dictionary<string, decimal>();

        public DecimalCalculatorVisitor(object bindings)
        {
            if (bindings == null)
            {
                return;
            }
            System.Type type = bindings.GetType();
            foreach (var prop in type.GetProperties())
            {
                var propType = prop.PropertyType;
                var propName = prop.Name;
                if (propType == typeof(String))
                {
                    var val = (string)type.GetProperty(propName).GetValue(bindings);
                    _bindings.Add(propName, decimal.Parse(val));
                }
                else
                {
                    var val = Convert.ToDecimal(type.GetProperty(propName).GetValue(bindings));
                    _bindings.Add(propName, val);
                }
            }
        }

        public decimal VisitAtom([NotNull] calculatorParser.AtomContext ctx)
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
                return 0;
            }
            var exp = ctx.expression();
            return exp.Accept(this);
        }

        public decimal VisitConstant([NotNull] calculatorParser.ConstantContext ctx)
        {
            var txt = ctx.GetText();
            if (txt == "pi")
            {
                return (decimal)Math.PI;
            }
            else if (txt == "e")
            {
                return (decimal)Math.E;
            }
            return 0;
        }

        public decimal VisitExpression([NotNull] calculatorParser.ExpressionContext ctx)
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
                    dec0 = dec0 + dec;
                }
                else if (op == "-")
                {
                    dec0 = dec0 - dec;
                }
            }
            return dec0;
        }

        public decimal VisitFunc_([NotNull] calculatorParser.Func_Context ctx)
        {
            var funcname = ctx.GetChild(0).GetText();
            var arg1 = ctx.GetChild(2).Accept(this);
            switch (funcname)
            {
                case "cos":
                    return (decimal)Math.Cos((double)arg1);
                case "tan":
                    return (decimal)Math.Tan((double)arg1);
                case "sin":
                    return (decimal)Math.Sin((double)arg1);
                case "acos":
                    return (decimal)Math.Acos((double)arg1);
                case "atan":
                    return (decimal)Math.Atan((double)arg1);
                case "asin":
                    return (decimal)Math.Asin((double)arg1);
                case "log":
                    return (decimal)Math.Log10((double)arg1);
                case "ln":
                    return (decimal)Math.Log((double)arg1);
                case "sqrt":
                    return (decimal)Math.Sqrt((double)arg1);
                case "floor":
                    return decimal.Floor(arg1);
                case "ceil":
                    return decimal.Ceiling(arg1);
                case "round":
                    return decimal.Round(arg1);
                case "round2":
                    return decimal.Ceiling(arg1 - 0.5M);

                default:
                    return arg1;
            }
        }

        public decimal VisitMultiplyingExpression([NotNull] calculatorParser.MultiplyingExpressionContext ctx)
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
                    dec0 = dec0 * dec;
                }
                else if (op == "/")
                {
                    dec0 = dec0 / dec;
                }
            }
            return dec0;
        }

        public decimal VisitPowExpression([NotNull] calculatorParser.PowExpressionContext ctx)
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
                dec0 = (decimal)Math.Pow((double)dec0, (double)dec);
            }
            return dec0;
        }

        public decimal VisitScientific([NotNull] calculatorParser.ScientificContext ctx)
        {
            var txt = ctx.GetText();
            return decimal.Parse(txt, System.Globalization.NumberStyles.Float);
        }

        public decimal VisitSignedAtom([NotNull] calculatorParser.SignedAtomContext ctx)
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
                return 0;
            }
            var sign = ctx.GetChild(0).GetText();
            var signedAtom = ctx.signedAtom().Accept(this);
            if (sign == "-")
            {
                return -signedAtom;
            }
            return signedAtom;
        }

        public decimal VisitVariable([NotNull] calculatorParser.VariableContext ctx)
        {
            var txt = ctx.GetText();
            var bindings = _bindings;
            if (bindings == null || !bindings.ContainsKey(txt))
            {
                throw new Exception($"the variable {txt} has no value");
            }
            var value = bindings[txt];
            return value;
        }
    }
}


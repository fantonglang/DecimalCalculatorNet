using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr4.Runtime;
using DecimalCalculator.Net.antlr;
using DecimalCalculator.NET.antlr;

namespace DecimalCalculator.NET
{
    public static class DecimalCalculator
    {
        public static decimal Calc(string formula, object bindings = null)
        {
            var inputStream = CharStreams.fromString(formula);
            var lexer = new calculatorLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new calculatorParser(tokenStream);
            parser.BuildParseTree = true;
            var tree = parser.expression();

            var result = tree.Accept(new DecimalCalculatorVisitor(bindings));
            return result;
        }

        public static calculatorParser.ExpressionContext GetAntlrExpression(string formula)
        {
            var inputStream = CharStreams.fromString(formula);
            var lexer = new calculatorLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new calculatorParser(tokenStream);
            parser.BuildParseTree = true;
            var tree = parser.expression();

            return tree;
        }

        public static decimal ExecAntlrExp(calculatorParser.ExpressionContext exp, object bindings = null)
        {
            var result = exp.Accept(new DecimalCalculatorVisitor(bindings));
            return result;
        }

        public static Func<IDictionary<string, decimal>, decimal> GetExpression(string formula)
        {
            var inputStream = CharStreams.fromString(formula);
            var lexer = new calculatorLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new calculatorParser(tokenStream);
            parser.BuildParseTree = true;
            var tree = parser.expression();

            var visitor = new NormalExpressionVisitor();
            var exp = tree.Accept(visitor);
            var lambda = Expression.Lambda(exp, visitor.Arg);
            var func = (Func<IDictionary<string, decimal>, decimal>)lambda.Compile();
            return func;
        }

        public static decimal ExecExp(Func<IDictionary<string, decimal>, decimal> exp, IDictionary<string, decimal> bindings = null)
        {
            var _bindings = bindings ?? new Dictionary<string, decimal>();
            return exp(_bindings);
        }
    }
}


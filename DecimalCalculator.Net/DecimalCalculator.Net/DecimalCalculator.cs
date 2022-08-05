using System;
using Antlr4.Runtime;
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
    }
}


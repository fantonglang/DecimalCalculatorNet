using System.Collections;
using System.Diagnostics;
using Xunit.Abstractions;

namespace DecimalCalculator.Test;

public class DecimalCalculatorTest
{
    private readonly ITestOutputHelper Output;
    public DecimalCalculatorTest(ITestOutputHelper output)
    {
        Output = output;
    }

    [Theory]
    [ClassData(typeof(TestCalcData))]
    public void TestCalc(string formula, object bindings, decimal result)
    {
        var _result = DecimalCalculator.NET.DecimalCalculator.Calc(formula, bindings);
        Assert.Equal(result, decimal.Round(_result));
    }

    [Fact]
    public void TestCalc2()
    {
        var exp = DecimalCalculator.NET.DecimalCalculator.GetAntlrExpression("a - 0.1");
        var result1 = DecimalCalculator.NET.DecimalCalculator.ExecAntlrExp(exp, new { a = 0.3M });
        Assert.Equal(0.2M, result1);
        var result2 = DecimalCalculator.NET.DecimalCalculator.ExecAntlrExp(exp, new { a = 0.4M });
        Assert.Equal(0.3M, result2);
        var result3 = DecimalCalculator.NET.DecimalCalculator.ExecAntlrExp(exp, new { a = 0.5M });
        Assert.Equal(0.4M, result3);
        var result4 = DecimalCalculator.NET.DecimalCalculator.ExecAntlrExp(exp, new { a = 0.6M });
        Assert.Equal(0.5M, result4);
    }

    [Fact]
    public void TestPerformance()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        string formula = "(a+b+c+d+ee+f+g)*h*i*floor(j*m)/k/l";
        var exp = DecimalCalculator.NET.DecimalCalculator.GetAntlrExpression(formula);
        for (int i = 0; i < 10000; ++i)
        {
            object bindings = new { a = 12.5, b = 348.1, c = 33, d = 12, ee = 1, f = 47, g = 123.12, h = -25, i = -1, j = 23, m = 16, k = 2, l = 3 };
            var result = DecimalCalculator.NET.DecimalCalculator.ExecAntlrExp(exp, bindings);
        }
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        Output.WriteLine($"antlr costs {ts.TotalSeconds}s");

        stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < 10000; ++i)
        {
            decimal a = 12.5M, b = 348.1M, c = 33M, d = 12M, e = 1M, f = 47M, g = 123.12M, h = -25M, _i = -1M, j = 23M, m = 16M, k = 2M, l = 3M;
            decimal result = (a + b + c + d + e + f + g) * h * _i * decimal.Floor(j * m) / k / l;
        }
        stopwatch.Stop();
        ts = stopwatch.Elapsed;
        Output.WriteLine($"normal way costs {ts.TotalSeconds}s");
    }

    [Fact]
    public void TestExpPerformance()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        string formula = "(a+b+c+d+e+f+g)*h*i*floor(j*m)/k/l";
        var exp = DecimalCalculator.NET.DecimalCalculator.GetExpression(formula);
        for (int i = 0; i < 10000; ++i)
        {
            var bindings = new Dictionary<string, decimal>
            {
                { "a", 12.5M },
                { "b", 348.1M },
                { "c", 33M },
                { "d", 12M },
                { "e", 1M },
                { "f", 47M },
                { "g", 123.12M },
                { "h", -25M },
                { "i", -1M },
                { "j", 23M },
                { "m", 16M },
                { "k", 2M },
                { "l", 3M }
            };
            var result = DecimalCalculator.NET.DecimalCalculator.ExecExp(exp, bindings);
        }
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        Output.WriteLine($"antlr costs {ts.TotalSeconds}s");
    }
}

public class TestCalcData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { "cos(pi/2) * 1000", new { }, 0M };
        yield return new object[] { "(0.3 - a) * 10", new { a = 0.1M }, 2M };
        yield return new object[] { "ceil(a - 0.5)", new { a = 1M }, 1M };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
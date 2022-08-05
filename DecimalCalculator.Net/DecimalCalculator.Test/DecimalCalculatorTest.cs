using System.Collections;
using System.Diagnostics;

namespace DecimalCalculator.Test;

public class DecimalCalculatorTest
{
    [Theory]
    [ClassData(typeof(TestCalcData))]
    public void TestCalc(string formula, object bindings, decimal result)
    {
        var _result = DecimalCalculator.NET.DecimalCalculator.Calc(formula, bindings);
        Assert.Equal(result, decimal.Round(_result));
    }

    [Fact]
    public void TestPerformance()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        for(int i=0; i< 10000; ++i)
        {
            string formula = "(a+b+c+d+e+f+g)*h*i*j*m/k/l";
            object bindings = new { a = 12.5, b = 348.1, c = 33, d = 12, e = 1, f = 47, g = 123.12, h = -25, i = -1, j = 23, m = 16, k = 2, l = 3 };
            var result = DecimalCalculator.NET.DecimalCalculator.Calc(formula, bindings);
        }
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        Console.WriteLine(ts.TotalSeconds);
    }
}

public class TestCalcData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { "cos(pi/2) * 1000", new { }, 0M };
        yield return new object[] { "(0.3 - a) * 10", new { a = 0.1M }, 2M };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

public class TestyAI
{
    private readonly ITestOutputHelper _output;
    private readonly IRandom random = new RandomAdapter(12345);

    public TestyAI(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void Test_InicjalizacjaGry()
    {
        var runner1 = new GameRunner(
            12345,
            r => new RandomAgent(r),
            r => new RandomAgent(r)
        );

        var result1 = runner1.PlayGame(SimulationMode.Debug);

        _output.WriteLine($"Agent 1 Score: {result1.Agent1Score}");
        _output.WriteLine($"Agent 2 Score: {result1.Agent2Score}");

        var runner2 = new GameRunner(
            12345,
            r => new RandomAgent(r),
            r => new RandomAgent(r)
        );

        var result2 = runner2.PlayGame(SimulationMode.Debug);

        Console.WriteLine(result1.Agent1Score == result2.Agent1Score);
        Console.WriteLine(result1.Agent2Score == result2.Agent2Score);
        Console.WriteLine(result1.Winner == result2.Winner);
    }
    [Fact]
    public void Test_CloneGracza()
    {
        var gracz = new Gracz("Test");
        gracz.Surowce[Surowiec.Monety] = 10;

        var clone = gracz.Clone();

        clone.Surowce[Surowiec.Monety] -= 5;

        Assert.NotEqual(
            gracz.Surowce[Surowiec.Monety],
            clone.Surowce[Surowiec.Monety]);

        Assert.NotSame(gracz.Surowce, clone.Surowce);
    }
}

using BgPoelab;
using Xunit;

namespace BgPoelab.Tests;

public sealed class PoelabCalculatorTests
{
    private readonly PoelabCalculator _calculator = new();

    [Fact]
    public void AllOff_HasZeroProbability() =>
        Assert.Equal(0.0, _calculator.Calculate([0, 0, 0, 0, 0, 0]));

    [Fact]
    public void TwoCheckersOnOnePoint_HasKnownReferenceValues()
    {
        Assert.Equal(0.0, _calculator.Calculate([2, 0, 0, 0, 0, 0]), 14);
        Assert.Equal(10.0 / 36.0, _calculator.Calculate([0, 2, 0, 0, 0, 0]), 14);
        Assert.Equal(0.5, _calculator.Calculate([0, 0, 2, 0, 0, 0]), 14);
    }

    [Fact]
    public void StartingBlot_IsAlreadyFailure() =>
        Assert.Equal(1.0, _calculator.Calculate([1, 2, 0, 0, 0, 0]));

    [Fact]
    public void Position222333_MatchesIndependentReferenceCalculation() =>
        Assert.Equal(0.6627202618771073, _calculator.Calculate([2, 2, 2, 3, 3, 3]), 13);

    [Fact]
    public void CatalogContains12201BlotFreePositions() =>
        Assert.Equal(12_201, new BearoffPositionCatalog().EnumerateBlotFree().Count());

    [Fact]
    public void AscendingInvariantIdAnalysis_MatchesCatalogAnalysisForEveryBlotFreePosition()
    {
        Dictionary<long, double> expected = CalculateCatalogBaseline();
        var catalog = new BearoffPositionCatalog();
        var calculator = new PoelabCalculator();
        var actual = new Dictionary<long, double>();

        for (long id = 0; id <= catalog.MaxInvariantId; id++)
        {
            IndexedBearoffPosition indexed = catalog.GetByInvariantId(id);
            if (!indexed.Position.IsBlotFree)
                continue;

            actual.Add(id, calculator.Calculate(indexed.Position));
        }

        AssertEquivalent(expected, actual);
    }

    [Fact]
    public void ReverseInvariantIdAnalysis_MatchesCatalogAnalysisForEveryBlotFreePosition()
    {
        Dictionary<long, double> expected = CalculateCatalogBaseline();
        var catalog = new BearoffPositionCatalog();
        var calculator = new PoelabCalculator();
        var actual = new Dictionary<long, double>();

        for (long id = catalog.MaxInvariantId; id >= 0; id--)
        {
            IndexedBearoffPosition indexed = catalog.GetByInvariantId(id);
            if (!indexed.Position.IsBlotFree)
                continue;

            actual.Add(id, calculator.Calculate(indexed.Position));
        }

        AssertEquivalent(expected, actual);
    }

    [Fact]
    public void BearoffPosition_EnforcesDomainAndHexEncoding()
    {
        var position = new BearoffPosition([10, 5, 0, 0, 0, 0]);

        Assert.Equal("A50000", position.ToHexString());
        Assert.Equal(15, position.CheckersOnBoard);
        Assert.True(position.IsBlotFree);

        Assert.Throws<ArgumentException>(
            () => new BearoffPosition([10, 6, 0, 0, 0, 0]));
    }

    [Fact]
    public void OptimalPlayReporter_ReturnsCanonicalMoveNotation()
    {
        var calculator = new PoelabCalculator();
        var reporter = new OptimalPlayReporter(calculator);
        var position = new BearoffPosition([2, 0, 0, 0, 0, 0]);

        string play = reporter.GetOptimalPlay(position, 1, 1);

        Assert.Equal("1/off(2)", play);
    }

    [Fact]
    public void OptimalPlayReporter_ReportsBlotWhenEveryPlayLeavesBlot()
    {
        var calculator = new PoelabCalculator();
        var reporter = new OptimalPlayReporter(calculator);
        var position = new BearoffPosition([3, 0, 0, 0, 0, 0]);

        Assert.Equal("BLOT", reporter.GetOptimalPlay(position, 1, 2));
    }

    private static Dictionary<long, double> CalculateCatalogBaseline()
    {
        var calculator = new PoelabCalculator();

        return new BearoffPositionCatalog()
            .EnumerateBlotFree()
            .ToDictionary(
                indexed => indexed.InvariantId,
                indexed => calculator.Calculate(indexed.Position));
    }

    private static void AssertEquivalent(
        Dictionary<long, double> expected,
        Dictionary<long, double> actual)
    {
        Assert.Equal(12_201, expected.Count);
        Assert.Equal(expected.Count, actual.Count);

        foreach ((long id, double expectedPoelab) in expected)
        {
            Assert.True(
                actual.TryGetValue(id, out double actualPoelab),
                $"Invariant ID {id} was missing from traversal.");

            Assert.Equal(expectedPoelab, actualPoelab);
        }
    }
}

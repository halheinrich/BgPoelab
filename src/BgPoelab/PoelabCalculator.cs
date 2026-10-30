using BgDataTypes_Lib;
using BgMoveGen;

namespace BgPoelab;

/// <summary>Computes probability of ever ending a complete legal play with a blot.</summary>
public sealed class PoelabCalculator
{
    private const int DieSides = 6;
    private const double OrderedRollCount = DieSides * DieSides;

    private readonly Dictionary<BearoffPosition, double> _memo = new();

    public double Calculate(ReadOnlySpan<int> points) =>
        Calculate(new BearoffPosition(points));

    public double Calculate(BearoffPosition position)
    {
        if (!position.IsBlotFree)
            return 1.0;

        return CalculateBlotFree(position);
    }

    private double CalculateBlotFree(BearoffPosition position)
    {
        if (position.CheckersOnBoard == 0)
            return 0.0;

        if (_memo.TryGetValue(position, out double cached))
            return cached;

        BoardState board = position.ToBoardState();
        double sum = 0.0;

        for (int die1 = 1; die1 <= DieSides; die1++)
        {
            for (int die2 = 1; die2 <= DieSides; die2++)
            {
                IReadOnlyList<BoardPosition> successors =
                    MoveGenerator.GenerateResultingPositions(board, die1, die2);

                double best = 1.0;

                foreach (BoardPosition successor in successors)
                {
                    BearoffPosition successorPosition =
                        BearoffPosition.FromBoardPosition(successor);

                    double value = successorPosition.IsBlotFree
                        ? CalculateBlotFree(successorPosition)
                        : 1.0;

                    if (value < best)
                        best = value;

                    if (best == 0.0)
                        break;
                }

                sum += best;
            }
        }

        double result = sum / OrderedRollCount;
        _memo.Add(position, result);
        return result;
    }
}

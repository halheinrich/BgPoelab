using BgMoveGen;

namespace BgPoelab;

/// <summary>
/// Selects the legal play that minimizes downstream POELAB for a dice roll.
/// </summary>
public sealed class OptimalPlayReporter
{
    private readonly PoelabCalculator _calculator;

    /// <summary>Creates a reporter that values each resulting position with <paramref name="calculator"/>.</summary>
    /// <param name="calculator">The calculator to use; sharing one shares its memo.</param>
    /// <exception cref="ArgumentNullException"><paramref name="calculator"/> is null.</exception>
    public OptimalPlayReporter(PoelabCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    /// <summary>
    /// The notation of the legal play that leaves a blot-free position with the
    /// lowest POELAB. Equal POELABs are broken by the ordinally least notation,
    /// and <c>"BLOT"</c> is returned when every legal play leaves a blot.
    /// </summary>
    /// <remarks>The notation is <see cref="BgDataTypes_Lib.Play.ToNotation"/>'s; the empty play is the empty string.</remarks>
    /// <param name="position">The position the roll is played from.</param>
    /// <param name="die1">One die, 1 through 6.</param>
    /// <param name="die2">The other die, 1 through 6.</param>
    /// <exception cref="ArgumentOutOfRangeException">A die is outside 1 to 6.</exception>
    public string GetOptimalPlay(
        BearoffPosition position,
        int die1,
        int die2)
    {
        IReadOnlyList<CandidatePlay> candidates =
            MoveGenerator.GenerateCandidatePlays(
                position.ToBoardState(),
                die1,
                die2);

        double bestPoelab = double.PositiveInfinity;
        string? bestNotation = null;
        bool foundBlotFreeCandidate = false;

        foreach (CandidatePlay candidate in candidates)
        {
            BearoffPosition resultingPosition =
                BearoffPosition.FromBoardPosition(candidate.ResultingPosition);

            if (!resultingPosition.IsBlotFree)
                continue;

            foundBlotFreeCandidate = true;

            double poelab = _calculator.Calculate(resultingPosition);
            string notation = candidate.Play.ToNotation();

            if (poelab < bestPoelab ||
                (poelab == bestPoelab &&
                 (bestNotation is null ||
                  StringComparer.Ordinal.Compare(notation, bestNotation) < 0)))
            {
                bestPoelab = poelab;
                bestNotation = notation;
            }
        }

        return foundBlotFreeCandidate
            ? bestNotation!
            : "BLOT";
    }
}
using BgMoveGen;

namespace BgPoelab;

/// <summary>
/// Selects the legal play that minimizes downstream POELAB for a dice roll.
/// </summary>
public sealed class OptimalPlayReporter
{
    private readonly PoelabCalculator _calculator;

    public OptimalPlayReporter(PoelabCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

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
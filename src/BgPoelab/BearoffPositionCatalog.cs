using BgBoNamespace;

namespace BgPoelab;

/// <summary>
/// Owns the mapping between BgPoelab bearoff positions and bgBoEnumeration invariant IDs.
/// </summary>
public sealed class BearoffPositionCatalog
{
    private const int OffPoint = 0;

    private readonly BgBoEnumeration _enumeration =
        new(BearoffPosition.TotalCheckers, OffPoint, BearoffPosition.HomePointCount);

    public long MaxInvariantId => checked((long)_enumeration.MaxId);

    public IEnumerable<IndexedBearoffPosition> EnumerateBlotFree()
    {
        foreach (nint[] menOnPoint in _enumeration.GetMenOnPointArrayYielder())
        {
            BearoffPosition position = FromEnumerationPosition(menOnPoint);
            if (!position.IsBlotFree)
                continue;

            yield return new IndexedBearoffPosition(
                checked((long)_enumeration.GetInvariantIdFromPosition(menOnPoint)),
                position);
        }
    }

    public IndexedBearoffPosition GetByInvariantId(long invariantId)
    {
        if (invariantId < 0 || invariantId > MaxInvariantId)
            throw new ArgumentOutOfRangeException(nameof(invariantId));

        nint nativeId = checked((nint)invariantId);
        nint[] menOnPoint = _enumeration.GetMenOnPointFromInvariantId(nativeId);

        return new IndexedBearoffPosition(
            invariantId,
            FromEnumerationPosition(menOnPoint));
    }

    private static BearoffPosition FromEnumerationPosition(nint[] menOnPoint)
    {
        ArgumentNullException.ThrowIfNull(menOnPoint);

        if (menOnPoint.Length <= BearoffPosition.HomePointCount)
            throw new ArgumentException(
                "Enumeration position does not contain off plus home-board points 1 through 6.",
                nameof(menOnPoint));

        Span<int> points = stackalloc int[BearoffPosition.HomePointCount];

        for (int point = 1; point <= BearoffPosition.HomePointCount; point++)
            points[point - 1] = checked((int)menOnPoint[point]);

        return new BearoffPosition(points);
    }
}

public readonly record struct IndexedBearoffPosition(
    long InvariantId,
    BearoffPosition Position);

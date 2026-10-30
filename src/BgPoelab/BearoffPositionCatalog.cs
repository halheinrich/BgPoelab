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

    /// <summary>
    /// The highest invariant ID bgBoEnumeration assigns to a bearoff position;
    /// IDs run from 0 to this value inclusive.
    /// </summary>
    public long MaxInvariantId => checked((long)_enumeration.MaxId);

    /// <summary>
    /// Every blot-free bearoff position with its invariant ID, in
    /// bgBoEnumeration's enumeration order. Positions are produced lazily.
    /// </summary>
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

    /// <summary>The bearoff position with <paramref name="invariantId"/>, which may hold a blot.</summary>
    /// <param name="invariantId">An ID from 0 to <see cref="MaxInvariantId"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="invariantId"/> is outside 0 to <see cref="MaxInvariantId"/>.
    /// </exception>
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

/// <summary>A bearoff position paired with its bgBoEnumeration invariant ID.</summary>
/// <param name="InvariantId">The ID bgBoEnumeration assigns to <paramref name="Position"/>.</param>
/// <param name="Position">The bearoff position.</param>
public readonly record struct IndexedBearoffPosition(
    long InvariantId,
    BearoffPosition Position);

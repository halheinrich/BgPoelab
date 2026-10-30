using BgDataTypes_Lib;

namespace BgPoelab;

/// <summary>
/// Immutable one-sided bearoff position. Points are numbered 1 through 6;
/// any of the 15 checkers not on those points are borne off.
/// </summary>
public readonly record struct BearoffPosition
{
    /// <summary>The number of home-board points a position covers: points 1 through 6.</summary>
    public const int HomePointCount = 6;

    /// <summary>The checkers a side has; those not on points 1 through 6 are borne off.</summary>
    public const int TotalCheckers = 15;

    // BoardPosition's layout: index 0 and 25 are the bars, 1-24 the points.
    private const int BoardSlotCount = 26;

    private readonly ulong _packed;

    /// <summary>Creates a position from the checker count on each of points 1 through 6.</summary>
    /// <param name="points">Six counts, point 1 first.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="points"/> does not hold six counts, or they total more than
    /// <see cref="TotalCheckers"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">A count is outside 0 to <see cref="TotalCheckers"/>.</exception>
    public BearoffPosition(ReadOnlySpan<int> points)
    {
        if (points.Length != HomePointCount)
            throw new ArgumentException(
                $"Exactly {HomePointCount} home-board point counts are required.",
                nameof(points));

        ulong packed = 0;
        int checkersOnBoard = 0;

        for (int i = 0; i < HomePointCount; i++)
        {
            int count = points[i];
            if ((uint)count > TotalCheckers)
                throw new ArgumentOutOfRangeException(
                    nameof(points),
                    $"Point counts must be between 0 and {TotalCheckers}.");

            checkersOnBoard += count;
            packed |= (ulong)count << (i * 4);
        }

        if (checkersOnBoard > TotalCheckers)
            throw new ArgumentException(
                $"A bearoff position cannot contain more than {TotalCheckers} checkers on the board.",
                nameof(points));

        _packed = packed;
    }

    /// <summary>The number of checkers on <paramref name="point"/>.</summary>
    /// <param name="point">A home-board point, 1 through 6.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="point"/> is outside 1 to 6.</exception>
    public int this[int point]
    {
        get
        {
            if ((uint)(point - 1) >= HomePointCount)
                throw new ArgumentOutOfRangeException(nameof(point));

            return (int)((_packed >> ((point - 1) * 4)) & 0xFUL);
        }
    }

    /// <summary>The checkers on points 1 through 6; zero once every checker is borne off.</summary>
    public int CheckersOnBoard
    {
        get
        {
            int total = 0;
            for (int point = 1; point <= HomePointCount; point++)
                total += this[point];

            return total;
        }
    }

    /// <summary>Whether no point holds exactly one checker.</summary>
    public bool IsBlotFree
    {
        get
        {
            for (int point = 1; point <= HomePointCount; point++)
                if (this[point] == 1)
                    return false;

            return true;
        }
    }

    /// <summary>
    /// A board holding these checkers as the on-roll side's, with no opponent
    /// checkers: the form BgMoveGen generates plays from.
    /// </summary>
    public BoardState ToBoardState()
    {
        Span<int> counts = stackalloc int[BoardSlotCount];

        for (int point = 1; point <= HomePointCount; point++)
            counts[point] = this[point];

        return new BoardState(new BoardPosition(counts));
    }

    /// <summary>
    /// The position the on-roll side holds on points 1 through 6 of
    /// <paramref name="board"/>, such as a position BgMoveGen returns.
    /// The bars and points 7 through 24 are not read.
    /// </summary>
    /// <param name="board">A board in the on-roll side's frame.</param>
    /// <exception cref="ArgumentOutOfRangeException">One of points 1 through 6 holds an opponent checker.</exception>
    public static BearoffPosition FromBoardPosition(BoardPosition board)
    {
        Span<int> points = stackalloc int[HomePointCount];
        for (int point = 1; point <= HomePointCount; point++)
            points[point - 1] = board[point];

        return new BearoffPosition(points);
    }

    /// <summary>
    /// Six uppercase hexadecimal digits, point 1 first, each the count on its
    /// point (<c>A</c> to <c>F</c> for 10 to 15): the CSV's Position column.
    /// </summary>
    public string ToHexString()
    {
        return string.Create(
            HomePointCount,
            this,
            static (chars, position) =>
            {
                const string HexDigits = "0123456789ABCDEF";

                for (int point = 1; point <= HomePointCount; point++)
                    chars[point - 1] = HexDigits[position[point]];
            });
    }
}

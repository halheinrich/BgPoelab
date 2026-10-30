using BgDataTypes_Lib;

namespace BgPoelab;

/// <summary>
/// Immutable one-sided bearoff position. Points are numbered 1 through 6;
/// any of the 15 checkers not on those points are borne off.
/// </summary>
public readonly record struct BearoffPosition
{
    public const int HomePointCount = 6;
    public const int TotalCheckers = 15;

    // BoardPosition's layout: index 0 and 25 are the bars, 1-24 the points.
    private const int BoardSlotCount = 26;

    private readonly ulong _packed;

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

    public int this[int point]
    {
        get
        {
            if ((uint)(point - 1) >= HomePointCount)
                throw new ArgumentOutOfRangeException(nameof(point));

            return (int)((_packed >> ((point - 1) * 4)) & 0xFUL);
        }
    }

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

    public BoardState ToBoardState()
    {
        Span<int> counts = stackalloc int[BoardSlotCount];

        for (int point = 1; point <= HomePointCount; point++)
            counts[point] = this[point];

        return new BoardState(new BoardPosition(counts));
    }

    public static BearoffPosition FromBoardPosition(BoardPosition board)
    {
        Span<int> points = stackalloc int[HomePointCount];
        for (int point = 1; point <= HomePointCount; point++)
            points[point - 1] = board[point];

        return new BearoffPosition(points);
    }

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

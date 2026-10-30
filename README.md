BgPoelab

Calculates POELAB: the probability that a one-sided bearoff position will ever
end a complete legal play with a blot, assuming checker play is chosen optimally
to minimize that probability.

Only the board after the complete legal play for a roll is tested. Intermediate
moves within the play do not count.

Architecture

BgMoveGen is the source of truth for legal resulting board states.

bgBoEnumeration is the source of truth for invariant IDs and enumeration.

BearoffPosition is BgPoelab's immutable six-point domain representation.

BearoffPositionCatalog owns the translation between bgBoEnumeration and
BearoffPosition.

PoelabCalculator owns only the recursive memoized POELAB calculation.

A bearoff position contains at most 15 checkers on points 1 through 6; the
remainder are borne off.

Dependencies

The project references these existing read-only projects:

BgMoveGen

BgDataTypes_Lib

bgBoEnumeration

The first two are expected as siblings under the Backgammon umbrella.
bgBoEnumeration is currently referenced at:

D:\Users\Hal\Documents\Visual Studio 2022\Projects\bgBoEnumeration\bgBoEnumeration\bgBoEnumeration.csproj

CSV

The output has one row per blot-free position and 30 columns:

InvariantID,Position,POELAB,1 pt,2 pt,3 pt,4 pt,5 pt,6 pt,11,12,13,14,15,16,22,23,24,25,26,33,34,35,36,44,45,46,55,56,66

InvariantID is supplied by bgBoEnumeration.

1 pt through 6 pt are the checker counts on each point, in decimal.

The 21 roll columns, 11 through 66, give the optimal play for that roll: the
legal play leaving a blot-free position with the lowest POELAB, ties broken by
the ordinally least notation. A play is written in standard notation, such as
1/off(2); BLOT means every legal play leaves a blot. The cells are empty when
no checkers remain.

Position is exactly six uppercase hexadecimal digits for points 1 through 6.
Digits 0-9 represent 0-9 checkers and A-F represent 10-15 checkers.
For example, 222333 remains 222333, while 10 checkers on point 1 and 5 on
point 2 is A50000.

POELAB is written with invariant culture and G17 double precision.

Verification

The test suite includes independent reference values and exhaustive comparisons
of all 12,201 blot-free positions under catalog order, ascending invariant-ID
order, and reverse invariant-ID order.
Running

From the repository root:

    dotnet run --project src/BgPoelab/BgPoelab.csproj -c Release

The generated CSV is written under output/. The output directory is intentionally
not tracked by Git because the results can be regenerated from the source.

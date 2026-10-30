# BgPoelab

> Collaboration contract: [`../AGENTS.md`](../AGENTS.md)
> Umbrella status & dependency graph: [`../INSTRUCTIONS.md`](../INSTRUCTIONS.md)
> Mission & principles: [`../VISION.md`](../VISION.md)

## What it is for

BgPoelab is a console program that computes POELAB for every blot-free
one-sided bearoff position: the probability of ever ending a complete legal
play with a blot, under the play that minimizes it. For each position it
writes one CSV row with the POELAB and the optimal play for each of the 21
rolls. [`README.md`](README.md) defines POELAB, the architecture and the CSV
columns; this file does not repeat them.

## Stack, solution, repo

C# / .NET 10 console app / xUnit.

- Solution: `BgPoelab.slnx`, with `src/BgPoelab` (the program) and
  `tests/BgPoelab.Tests`.
- Repo: https://github.com/halheinrich/BgPoelab, branch `main`.
- Build policy: the repo-root `Directory.Build.props` (TFM,
  `TreatWarningsAsErrors`, XML docs on the program's public surface; the
  test project opts out of doc generation) and `Directory.Packages.props`
  (Central Package Management, no inline `Version=`).

## Depends on

- **BgMoveGen** generates the legal plays and the positions they reach.
- **BgDataTypes_Lib** supplies `BoardState`, `BoardPosition` and `Play`.
  The play notation in the CSV is `Play.ToNotation()`, so a change to that
  notation changes the output, and the tie-break between equal POELABs as
  well.
- **bgBoEnumeration** assigns the invariant IDs and drives the enumeration.
  It is not an umbrella member; see the pitfalls below.

BgMoveGen and BgDataTypes_Lib are referenced as umbrella siblings
(`..\BgMoveGen`, `..\BgDataTypes_Lib`).

## Pitfalls

**bgBoEnumeration lives outside the umbrella, at an absolute path.**
`src/BgPoelab/BgPoelab.csproj` defaults `BgBoEnumerationProject` to
`D:\Users\Hal\Documents\Visual Studio 2022\Projects\bgBoEnumeration\bgBoEnumeration\bgBoEnumeration.csproj`.
On 2026-10-29 that folder was not under git, and the project targets
`net9.0`. A checkout on any other machine, or with that folder moved, does
not build until bgBoEnumeration is supplied.

**The override works for a project build, not for the solution.** Supply
another copy with `-p:BgBoEnumerationProject=<path to bgBoEnumeration.csproj>`
when building, testing or running a project directly, for example
`dotnet test tests/BgPoelab.Tests/BgPoelab.Tests.csproj -c Release -p:BgBoEnumerationProject=...`.
`BgPoelab.slnx` lists bgBoEnumeration by its own relative path, so when the
default location is missing, `dotnet build BgPoelab.slnx` fails at restore
with MSB3202 ("project file was not found"), with or without the override.
Probed 2026-10-29: a copy of the solution whose bgBoEnumeration entry
pointed at a missing path failed this way, the unchanged copy built clean,
and a direct build of the test project with the override pointing at a copy
of bgBoEnumeration elsewhere succeeded.

**Running the program overwrites `output/poelab.csv`.** With no argument it
writes there, finding the repo root by walking up to `BgPoelab.slnx`. The
`output/` folder is gitignored, so that file is the only local copy of the
previous results. Pass a path to write anywhere else.

## Running and checking

From the repo root:

```bash
dotnet build BgPoelab.slnx -c Release
```

```bash
dotnet test BgPoelab.slnx -c Release
```

```bash
dotnet run --project src/BgPoelab/BgPoelab.csproj -c Release -- <output.csv>
```

The test suite includes exhaustive comparisons over all 12,201 blot-free
positions in three traversal orders; it took 8 to 15 seconds in Release
(measured 2026-10-29). The program prints the row count and the 222333 POELAB, a quick
check against the test's reference value.

For a change that must not alter results, write the CSV to another path and
compare it byte for byte with the existing `output/poelab.csv`. That is how
the port to the current BgMoveGen and BgDataTypes_Lib APIs was verified
(`halheinrich/backgammon#453`).

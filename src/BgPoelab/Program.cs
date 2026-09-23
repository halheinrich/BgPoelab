using System.Globalization;
using System.Text;
using BgPoelab;

string outputPath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.Combine(FindRepositoryRoot(), "output", "poelab.csv");

var catalog = new BearoffPositionCatalog();
var calculator = new PoelabCalculator();
var playReporter = new OptimalPlayReporter(calculator);

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

using var writer = new StreamWriter(outputPath, false, new UTF8Encoding(false));
writer.WriteLine(
    "InvariantID,Position,POELAB,1 pt,2 pt,3 pt,4 pt,5 pt,6 pt," +
    "11,12,13,14,15,16,22,23,24,25,26,33,34,35,36,44,45,46,55,56,66");
int rows = 0;
double target = double.NaN;
long targetInvariantId = -1;

foreach (IndexedBearoffPosition indexed in catalog.EnumerateBlotFree())
{
    double poelab = calculator.Calculate(indexed.Position);
    string encodedPosition = indexed.Position.ToHexString();

    writer.Write(indexed.InvariantId.ToString(CultureInfo.InvariantCulture));
    writer.Write(',');
    writer.Write(encodedPosition);
    writer.Write(',');
    writer.Write(poelab.ToString("G17", CultureInfo.InvariantCulture));

    for (int point = 1; point <= BearoffPosition.HomePointCount; point++)
    {
        writer.Write(',');
        writer.Write(indexed.Position[point].ToString(CultureInfo.InvariantCulture));
    }

    for (int die1 = 1; die1 <= 6; die1++)
    {
        for (int die2 = die1; die2 <= 6; die2++)
        {
            writer.Write(',');
            writer.Write(
                playReporter.GetOptimalPlay(
                    indexed.Position,
                    die1,
                    die2));
        }
    }

    writer.WriteLine();
    if (encodedPosition == "222333")
    {
        target = poelab;
        targetInvariantId = indexed.InvariantId;
    }

    rows++;
}

Console.WriteLine($"Wrote {rows:N0} blot-free positions to {outputPath}");
Console.WriteLine($"222333 invariant ID = {targetInvariantId}");
Console.WriteLine($"222333 POELAB = {target:G17}");

static string FindRepositoryRoot()
{
    DirectoryInfo? directory = new(AppContext.BaseDirectory);

    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "BgPoelab.slnx")))
            return directory.FullName;

        directory = directory.Parent;
    }

    throw new InvalidOperationException(
        $"Could not locate the BgPoelab repository root from '{AppContext.BaseDirectory}'.");
}
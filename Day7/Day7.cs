var lines = File.ReadAllLines("Day7.txt");

int lineCount = lines.Length;
var tachyonBeam = 'S';
var splitter = '^';

SortedSet<int> beamLocations = new() { lines[0].IndexOf(tachyonBeam) };
var caretPositions = new Dictionary<int, List<int>>();

for (int i = 2; i < lineCount; i += 2)
{
    var carets = lines[i]
        .Select((c, index) => (c, index))
        .Where(x => x.c == splitter)
        .Select(x => x.index)
        .ToList();

    if (carets.Count > 0)
    {
        caretPositions[i] = carets;
    }
}

/*
foreach (var kvp in caretPositions)
{
    Console.WriteLine($"Row {kvp.Key}: [{string.Join(", ", kvp.Value)}]");
}
*/

// Part 1
/*
    We have SortedSet for beam locations and List of caret positions per row
    If any caret position in a row matches a beam location, that splitter activates
    When a splitter activates, it removes the beam at its position and adds beams to the left and right positions
    SortedSet covers unique beam locations automatically
*/
var countOfBeamSplites = caretPositions
    .SelectMany(row => row.Value
        .Where(pos => beamLocations.Contains(pos))
        .Select(pos => (row: row.Key, pos)))
    .Count(x =>
    {
        beamLocations.Remove(x.pos);
        beamLocations.Add(x.pos - 1);
        beamLocations.Add(x.pos + 1);
        return true;
    });

Console.WriteLine($"Total beam splitters activated: {countOfBeamSplites}");

// Part 2
/*
    Count how many combination of beam paths can be formed, for each left/right choice at each splitter
    We will use recursive approach
    Also we need to use memoization for avoid recomputing same states
*/
var memo = new Dictionary<(int row, int pos), long>();

long CountBeamPaths(int row, int pos)
{
    if (row >= lineCount)
        return 1;

    if (memo.TryGetValue((row, pos), out var cached))
        return cached;

    long totalPaths = 0;

    if (caretPositions.TryGetValue(row, out var carets))
    {
        if (carets.Contains(pos))
        {
            totalPaths += CountBeamPaths(row + 2, pos - 1);
            totalPaths += CountBeamPaths(row + 2, pos + 1);
        }
        else
        {
            totalPaths += CountBeamPaths(row + 2, pos);
        }
    }
    else
    {
        totalPaths += CountBeamPaths(row + 2, pos);
    }

    memo[(row, pos)] = totalPaths;
    return totalPaths;
}

var totalBeamPaths = CountBeamPaths(2, lines[0].IndexOf(tachyonBeam));
Console.WriteLine($"Total distinct beam paths: {totalBeamPaths}");

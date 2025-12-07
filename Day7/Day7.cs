var lines = File.ReadAllLines("Day7.txt");

int lineCount = lines.Length;
var tachyonBeam = 'S';
var splitter = '^';
int startPos = lines[0].IndexOf(tachyonBeam);

SortedSet<int> beamLocations = new() { startPos };
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
var sw = System.Diagnostics.Stopwatch.StartNew();

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

sw.Stop();
Console.WriteLine($"Total distinct beam paths: {totalBeamPaths}");
Console.WriteLine($"Part 2 Time: {sw.ElapsedMilliseconds}ms");

// Part 2 optimization
/*
    Easy algorithm that i foun on reddit:
    take beam as number and on split sum all overlap beams
    On last line sum all beams to get total paths
*/

var beamPaths = Enumerable.Range(0, lineCount)
    .ToDictionary(i => i, i => 0L);

beamPaths[startPos] = 1;

var beamLocations2 = new SortedSet<int> { startPos };

sw.Restart();
foreach (var (row, pos) in caretPositions
    .SelectMany(row => row.Value
        .Where(pos => beamLocations2.Contains(pos))
        .Select(pos => (row: row.Key, pos))))
{
    beamLocations2.Remove(pos);
    beamLocations2.Add(pos - 1);
    beamLocations2.Add(pos + 1);

    beamPaths[pos - 1] += beamPaths[pos];
    beamPaths[pos + 1] += beamPaths[pos];
    beamPaths[pos] = 0;
}

sw.Stop();
Console.WriteLine($"Total distinct beam paths (optimized): {beamPaths.Values.Sum()}");
Console.WriteLine($"Part 2 Optimized Time: {sw.ElapsedMilliseconds}ms");

// Part 2 2ms vs Part 2 Optimized 2ms on my machine 
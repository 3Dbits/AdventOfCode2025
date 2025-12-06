using System.Diagnostics;
var lines = File.ReadAllLines("Day5.txt");
var rangeLines = new List<Range>();
var idLines = new List<long>();
bool parsingRanges = true;

foreach (var line in lines)
{
    if (string.IsNullOrWhiteSpace(line))
    {
        parsingRanges = false;
        continue;
    }

    if (parsingRanges && line.Contains("-"))
    {
        var parts = line.Split('-');
        rangeLines.Add(new Range(long.Parse(parts[0]), long.Parse(parts[1])));
    }
    else if (!parsingRanges && long.TryParse(line, out var id))
    {
        idLines.Add(id);
    }
}

Console.WriteLine($"Ranges: {rangeLines.Count}");
Console.WriteLine($"IDs: {idLines.Count}");

///////////////////////////////////////////////////////////////////////////////////////////////

// Part 1 - Easy way
var sw1 = Stopwatch.StartNew();
var result = idLines.Count(id => rangeLines.Any(range => range.Contains(id)));
sw1.Stop();
Console.WriteLine($"IDs found in ranges (Any): {result}");
Console.WriteLine($"Time: {sw1.ElapsedMilliseconds}ms");

// Maybe optimized way?? -- nope :D
var sw2 = Stopwatch.StartNew();
var result2 = idLines.Count(id => 
    rangeLines
        .Where(range => id >= range.Start)
        .Any(range => id <= range.End)
);
sw2.Stop();
Console.WriteLine($"IDs found in ranges (Where+Any): {result2}");
Console.WriteLine($"Time: {sw2.ElapsedMilliseconds}ms");

Console.WriteLine($"\nDifference: {Math.Abs(sw1.ElapsedMilliseconds - sw2.ElapsedMilliseconds)}ms");

// Part 2
var listOfValidIds = new RangeList(rangeLines);
listOfValidIds.MergeAll();

var countOfIds = listOfValidIds.TotalCount();
Console.WriteLine($"\nTotal valid IDs after merging ranges2: {countOfIds}");

///////////////////////////////////////////////////////////////////////////////////////////////

class RangeList(IEnumerable<Range> ranges) : List<Range>(ranges)
{
    public void MergeAll()
    {
        if (Count == 0) return;

        this.Sort((a, b) => a.Start.CompareTo(b.Start));

        bool anyMerged;
        do
        {
            anyMerged = false;

            for (int i = 0; i < Count; i++)
            {
                for (int j = i + 1; j < Count; j++)
                {
                    if (this[i].Overlaps(this[j]))
                    {
                        var newStart = Math.Min(this[i].Start, this[j].Start);
                        var newEnd = Math.Max(this[i].End, this[j].End);
                        this[i] = new Range(newStart, newEnd);
                        RemoveAt(j);
                        anyMerged = true;
                        break;
                    }
                }

                if (anyMerged)
                    break;
            }
        } while (anyMerged);
    }

    public long TotalCount() => this.Sum(r => r.End - r.Start + 1);

    public bool ContainsId(long id) => this.Any(range => range.Contains(id));
}

struct Range(long start, long end)
{
    public long Start { get; set; } = start;
    public long End { get; set; } = end;

    public readonly bool Contains(long id) => id >= Start && id <= End;

    public readonly bool Overlaps(Range other) => Start <= other.End && End >= other.Start;
}
// Read and parse the file
var ranges = File.ReadAllText("Day2.txt")
    .Split(',')
    .Select(range =>
    {
        var parts = range.Split('-');
        var first = long.Parse(parts[0]);
        var last = long.Parse(parts[1]);
        return (first, last);
    })
    .ToList();

Console.WriteLine($"Range counts: {ranges.Count}");
Console.WriteLine($"First range: {ranges[0].first}-{ranges[0].last}");

// Part 1: Count how many numbers have repeating parts
var valueSum = ranges
    .SelectMany(range => LongRange(range.first, range.last))
    .Where(IsPartsRepeating)
    .Sum();

Console.WriteLine($"Sum of repeating parts: {valueSum}");

// Part 2: Count how many numbers have repeating sequences
var sequenceSum = ranges
    .SelectMany(range => LongRange(range.first, range.last))
    .Where(IsSequenceRepeating)
    .Sum();

Console.WriteLine($"Sum of repeating sequences: {sequenceSum}");

static IEnumerable<long> LongRange(long start, long end)
{
    for (long i = start; i <= end; i++)
    {
        yield return i;
    }
}

static bool IsPartsRepeating(long number)
{
    int digitCount = (int)Math.Floor(Math.Log10(number)) + 1;
    long divisor = (long)Math.Pow(10, digitCount / 2);

    long firstHalf = number / divisor;
    long secondHalf = number % divisor;

    return firstHalf == secondHalf;
}

static bool IsSequenceRepeating(long number)
{
    string numStr = number.ToString();
    int length = numStr.Length;
    
    for (int patternLength = 1; patternLength <= length / 2; patternLength++)
    {
        if (length % patternLength == 0)
        {
            string pattern = numStr.Substring(0, patternLength);
            int repeatCount = length / patternLength;
            
            string repeated = string.Concat(Enumerable.Repeat(pattern, repeatCount));
            
            if (repeated == numStr && repeatCount >= 2)
            {
                return true;
            }
        }
    }
    
    return false;
}
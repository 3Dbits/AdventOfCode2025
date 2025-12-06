var lines = File.ReadAllLines("Day6.txt");

// Part 1
/*
    Read line by line
    Last line contains operators for each column
    For each column, parse numbers and apply operation
*/
var columns = ParseColumns(lines);
var productOfSums = CalculateProductOfSums(columns);
Console.WriteLine($"Product of sums: {productOfSums}");

// Part 2
/*
    Similar to Part 1 but we need to look alignment in each column
    We apply operations on numbers formed by index in each column
    123 12  1  144
    12  123 12 1
    1   123 5  155
    +   *   +  *
    in pattern we see that operator starts at column first index, so we can subtract with next operator position to get digit count 
*/
var columnsWithDigits = ParseDigitColumns(lines);
var productOfDigitSums = CalculateProductOfDigitSums(columnsWithDigits);
Console.WriteLine($"Product of digit sums: {productOfDigitSums}");

// Part 2 optimization
/*
    As per assessment, we need to iteratate form right to left
    Read all rows per index, and on last column (where last line contains operators) we apply operation on numbers formed in that column
*/
// TODO ..

static long CalculateProductOfSums(Dictionary<int, (List<int> numbers, Operation op)> columns) =>
    columns.Values
        .Select(col => col.op == Operation.Added
            ? col.numbers.Sum(n => (long)n)
            : col.numbers.Aggregate(1L, (acc, n) => acc * n))
        .Sum();

static long CalculateProductOfDigitSums(Dictionary<int, (List<int[]> numbers, Operation op)> columnsWithDigits) =>
    columnsWithDigits.Values
        .Select(col =>
        {
            int digitCount = col.numbers[0].Length;

            var numbers = Enumerable.Range(0, digitCount)
                .Select(digitPos =>
                {
                    var digitString = string.Join("", col.numbers.Select(arr => arr[digitPos]));
                    var trimmed = digitString.Trim('0');
                    if (string.IsNullOrEmpty(trimmed))
                        throw new InvalidOperationException($"Invalid case: all zeros at digit position {digitPos}");
                    return long.Parse(trimmed);
                })
                .ToList();

            return col.op == Operation.Added
                ? numbers.Sum()
                : numbers.Aggregate(1L, (acc, n) => acc * n);
        })
        .Sum();

static Dictionary<int, (List<int[]> numbers, Operation op)> ParseDigitColumns(string[] lines)
{
    var operatorLine = lines[^1];
    var numberRows = lines[..^1];
    var rowLength = numberRows[0].Length;

    var operatorPositions = operatorLine
        .Select((col, index) => (col, index))
        .Where(x => !char.IsWhiteSpace(x.col))
        .Select(x => x.index)
        .ToList();

    return operatorPositions.Select((startPos, col) =>
    {
        var digitCount = col < operatorPositions.Count - 1
            ? operatorPositions[col + 1] - startPos - 1
            : rowLength - startPos;

        var digitArrays = numberRows
            .Select(row => Enumerable.Range(0, digitCount)
                .Select(i =>
                {
                    var pos = startPos + i;
                    return pos < row.Length && !char.IsWhiteSpace(row[pos]) ? row[pos] - '0' : 0;
                })
                .ToArray())
            .ToList();

        var op = operatorLine[startPos] == '+' ? Operation.Added : Operation.Multiplied;

        return (col, value: (numbers: digitArrays, op));
    })
    .ToDictionary(x => x.col, x => x.value);
}

static Dictionary<int, (List<int> numbers, Operation op)> ParseColumns(string[] lines)
{
    var numberRows = lines[..^1]
        .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        .ToList();

    var operators = lines[^1]
        .Split(' ', StringSplitOptions.RemoveEmptyEntries);

    return Enumerable.Range(0, numberRows[0].Length)
        .ToDictionary(
            col => col,
            col => (
                numbers: numberRows.Select(row => int.Parse(row[col])).ToList(),
                op: operators[col][0] == '+' ? Operation.Added : Operation.Multiplied
            )
        );
}

enum Operation
{
    Added,      // +
    Multiplied  // *
}
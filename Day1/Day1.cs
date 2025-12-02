// Read and parse the file
var orders = File.ReadAllLines("Day1.txt")
    .Select(line => 
    {
        var direction = line[0] == 'L' ? Direction.Left : Direction.Right;
        var value = int.Parse(line.Substring(1));
        return (direction, value);
    })
    .ToList();

Console.WriteLine($"List counts: {orders.Count}");

// Part 1: Count how many times the safe hits zero
var zeroCounts = orders
    .Aggregate((safe: new Safe(50), count: 0), (acc, order) =>
    {
        var newSafe = order.direction == Direction.Left
            ? acc.safe - order.value
            : acc.safe + order.value;
        var newCount = newSafe.IsAtZero() ? acc.count + 1 : acc.count;
        return (newSafe, newCount);
    })
    .count;

Console.WriteLine($"Zero counts: {zeroCounts}");

// Part 2: Count how many times the safe passes through zero
var finalSafe = orders
    .Aggregate(new Safe(50), (safe, order) =>
    {
        return order.direction == Direction.Left
            ? safe - order.value
            : safe + order.value;
    });

Console.WriteLine($"Zero counts passes: {finalSafe.GetZeroCounts()}");

enum Direction
{
    Left,
    Right
}

struct Safe(int startingPosition)
{
    private int position = startingPosition;
    private int zeroCounts = 0;
    private const int MaxPosition = 99;
    private const int MinPosition = 0;

    public readonly int GetPosition() => position;

    public readonly bool IsAtZero() => position == 0;

    public readonly int GetZeroCounts() => zeroCounts;

    public static Safe operator +(Safe safe, int value)
    {
        var currentPosition = safe.position;
        var timesCrossed = 0;

        if (value >= (100 - currentPosition))
        {
            var firstHit = 100 - currentPosition;
            timesCrossed = 1 + (value - firstHit) / 100;
        }
        
        var newPosition = (currentPosition + value) % 100;
        
        safe.zeroCounts += timesCrossed;
        safe.position = newPosition;
        return safe;
    }

    public static Safe operator -(Safe safe, int value)
    {
        var currentPosition = safe.position;
        var timesCrossed = 0;
        
        if (value >= currentPosition && currentPosition > 0)
        {
            var firstHit = currentPosition;
            timesCrossed = 1 + (value - firstHit) / 100;
        }
        else if (value >= currentPosition + 100 && currentPosition == 0)
        {
            timesCrossed = value / 100;
        }
        
        var newPosition = ((currentPosition - value) % 100 + 100) % 100;
        
        safe.zeroCounts += timesCrossed;
        safe.position = newPosition;
        return safe;
    }
}


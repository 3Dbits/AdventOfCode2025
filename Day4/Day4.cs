var lines = File.ReadAllLines("Day4.txt");
var matrix = lines
    .SelectMany((line, y) =>
        line.Select((ch, x) =>
            new { Pos = new Position(x, y), Value = ch }))
    .ToDictionary(item => item.Pos, item => item.Value);

var path = new[]
{
    Direction.Left,
    Direction.Up,
    Direction.Right,
    Direction.Right,
    Direction.Down,
    Direction.Down,
    Direction.Left,
    Direction.Left
};

var rollOfPaper = '@';
var emptySpace = '.';

// Part 1
var startPositions = matrix
    .Where(kvp => kvp.Value == rollOfPaper)
    .Select(kvp => kvp.Key);
var result = startPositions
    .Count(pos => CountRollsInPath(pos, path, 0, matrix) < 4);

Console.WriteLine($"Number of rolls of paper with less than 4 rolls sorrounding: {result}");

// Part 2
var mutableMatrix = new Dictionary<Position, char>(matrix);
int totalRemoved = 0;
int removedInRound;

do
{
    var startPositions2 = mutableMatrix
        .Where(kvp => kvp.Value == rollOfPaper)
        .Select(kvp => kvp.Key)
        .ToList();
    
    var positionsToRemove = startPositions2
        .Where(pos => CountRollsInPath(pos, path, 0, mutableMatrix) < 4)
        .ToList();
    
    removedInRound = positionsToRemove.Count;
    totalRemoved += removedInRound;
    
    foreach (var pos in positionsToRemove)
    {
        mutableMatrix[pos] = emptySpace;
    }
    
    //Console.WriteLine($"Removed {removedInRound} rolls in this round");
    
} while (removedInRound > 0);

Console.WriteLine($"Total rolls removed: {totalRemoved}");

///////////////////////////////////////////////////////////////////////////////////////////////

// Recursive function :)
int CountRollsInPath(Position currentPos, Direction[] directions, int index, Dictionary<Position, char> matrix)
{
    if (index >= directions.Length)
        return 0;

    var nextPos = currentPos.Move(directions[index]);

    if (!matrix.ContainsKey(nextPos))
        return CountRollsInPath(nextPos, directions, index + 1, matrix);

    int currentRoll = matrix[nextPos] == rollOfPaper ? 1 : 0;
    return currentRoll + CountRollsInPath(nextPos, directions, index + 1, matrix);
}

//////////////////////////////////////////////////////////////////////////////////////////////
readonly struct Position(int x, int y)
{
    public int X { get; } = x;
    public int Y { get; } = y;
}

enum Direction
{
    Right,
    Left,
    Up,
    Down
}

static class DirectionHelpers
{
    public static (int dx, int dy) GetOffset(this Direction direction) => direction switch
    {
        Direction.Right => (1, 0),
        Direction.Left => (-1, 0),
        Direction.Up => (0, -1),
        Direction.Down => (0, 1),
        _ => throw new ArgumentException("Invalid direction")
    };

    public static Position Move(this Position pos, Direction direction)
    {
        var (dx, dy) = direction.GetOffset();
        return new Position(pos.X + dx, pos.Y + dy);
    }
}
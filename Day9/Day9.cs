var lines = File.ReadAllLines("test.txt");

var points = lines
    .Select(line =>
    {
        var parts = line.Split(',');
        return new Point(
            int.Parse(parts[0]),
            int.Parse(parts[1])
        );
    })
    .ToList();

// Part 1
/*
    First we create rectangles for all point pairs, sorted by area
    .Skip(i + 1) covers each combination only once (A->B is the same as B->A)
*/
var sortedRectangleCombinations = points
    .SelectMany((point, i) => points
        .Skip(i + 1)
        .Select(other => new Rectangle(point, other)))
    .OrderByDescending(v => v.AreaL)
    .ToList();

Console.WriteLine($"Rectangle: {sortedRectangleCombinations[0]}");

// Part 2
var vectorCombination = points
    .SelectMany((point, i) => points
        .Skip(i + 1)
        .Select(other => new Vector(point, other)))
    .Where(v => !v.IsDiagonal)
    .ToList();

foreach (var vec in vectorCombination)
{
    Console.WriteLine($"  {vec.PointStart} -> {vec.PointEnd}");
}

// Find leftmost point (minimum X coordinate)
var startPoint = points.OrderBy(p => p.X).ThenBy(p => p.Y).First();
var gridVectors = new List<Vector>();
var currentPoint = startPoint;
var visitedPoints = new HashSet<Point>();

// Directions: up-right, right-down, down-left, left-up
var directions = new[] { (1, 1), (1, -1), (-1, -1), (-1, 1) };
int directionIndex = 0;

while (true)
{
    visitedPoints.Add(currentPoint);
    var (dx, dy) = directions[directionIndex];

    // Find closest point in current direction (horizontal or vertical)
    var nextPoint = points
        .Where(p => !visitedPoints.Contains(p))
        .Where(p =>
            (dx > 0 && p.X > currentPoint.X && p.Y == currentPoint.Y) ||  // right
            (dx < 0 && p.X < currentPoint.X && p.Y == currentPoint.Y) ||  // left
            (dy > 0 && p.Y > currentPoint.Y && p.X == currentPoint.X) ||  // up
            (dy < 0 && p.Y < currentPoint.Y && p.X == currentPoint.X))    // down
        .OrderBy(p => Math.Abs(p.X - currentPoint.X) + Math.Abs(p.Y - currentPoint.Y))
        .FirstOrDefault();

    if (nextPoint.Equals(default(Point)))
    {
        // No point in current direction, try next direction
        directionIndex = (directionIndex + 1) % 4;

        // If we've tried all directions and found nothing, we're done
        if (directionIndex == 0)
            break;
        continue;
    }

    gridVectors.Add(new Vector(currentPoint, nextPoint));
    currentPoint = nextPoint;

    // Check if we've circled back to start
    if (currentPoint.Equals(startPoint) && gridVectors.Count > 1)
        break;
}

// Add closing vector back to start if we didn't already
if (!currentPoint.Equals(startPoint) && gridVectors.Count > 0)
{
    gridVectors.Add(new Vector(currentPoint, startPoint));
}

// Remove vectors that are crossed by grid or are the same as grid vectors
var filteredVectors = VectorFilter.RemoveCrossedVectors(vectorCombination, gridVectors);

// Also remove vectors that are the same as grid vectors
/*
filteredVectors = filteredVectors
    .Where(v => !gridVectors.Any(g =>
        (g.PointStart.Equals(v.PointStart) && g.PointEnd.Equals(v.PointEnd)) ||
        (g.PointStart.Equals(v.PointEnd) && g.PointEnd.Equals(v.PointStart))))
    .ToList();
*/

Console.WriteLine($"Grid vectors: {gridVectors.Count}");
Console.WriteLine($"Filtered vectors: {filteredVectors.Count}");

// Find pairs of vectors that share a common point and create rectangles
var rectanglesFromVectors = filteredVectors
    .SelectMany((v1, i) => filteredVectors
        .Skip(i + 1)
        .Where(v2 =>
            v1.PointStart.Equals(v2.PointStart) ||
            v1.PointStart.Equals(v2.PointEnd) ||
            v1.PointEnd.Equals(v2.PointStart) ||
            v1.PointEnd.Equals(v2.PointEnd))
        .Select(v2 => new
        {
            Vector1 = v1,
            Vector2 = v2,
            // Find the 4 points: common point and 3 corners
            Points = new[] { v1.PointStart, v1.PointEnd, v2.PointStart, v2.PointEnd }.Distinct().ToList()
        })
        .Where(x => x.Points.Count == 3) // Should have exactly 3 unique points (1 shared, 2 corners)
        .Select(x =>
        {
            // Find the rectangle corners from the 3 points
            var minX = x.Points.Min(p => p.X);
            var maxX = x.Points.Max(p => p.X);
            var minY = x.Points.Min(p => p.Y);
            var maxY = x.Points.Max(p => p.Y);
            return new Rectangle(new Point(minX, minY), new Point(maxX, maxY));
        }))
    .OrderByDescending(r => r.AreaL)
    .ToList();

if (rectanglesFromVectors.Any())
{
    var biggestRectangle = rectanglesFromVectors.First();
    Console.WriteLine($"\nBiggest rectangle from filtered vectors:");
    Console.WriteLine($"{biggestRectangle}");
}



public class Vector(Point pointStart, Point pointEnd)
{
    public Point PointStart { get; set; } = pointStart;
    public Point PointEnd { get; set; } = pointEnd;

    public bool IsHorizontal => PointStart.Y == PointEnd.Y;
    public bool IsVertical => PointStart.X == PointEnd.X;
    public bool IsDiagonal => !IsHorizontal && !IsVertical;
}

public class VectorFilter
{
    public static List<Vector> RemoveCrossedVectors(List<Vector> vectors, List<Vector> badVectors)
    {
        return vectors.Where(v => !IsCrossedByAny(v, badVectors)).ToList();
    }

    private static bool IsCrossedByAny(Vector vector, List<Vector> badVectors)
    {
        return badVectors.Any(bad => DoCross(vector, bad));
    }

    private static bool DoCross(Vector v1, Vector v2)
    {
        if ((v1.IsHorizontal && v2.IsHorizontal) || (v1.IsVertical && v2.IsVertical))
            return false;

        if (v1.IsHorizontal && v2.IsVertical)
        {
            return CheckCrossing(
                v2.PointStart.X, Math.Min(v1.PointStart.X, v1.PointEnd.X), Math.Max(v1.PointStart.X, v1.PointEnd.X),
                v1.PointStart.Y, Math.Min(v2.PointStart.Y, v2.PointEnd.Y), Math.Max(v2.PointStart.Y, v2.PointEnd.Y)
            );
        }
        else
        {
            return CheckCrossing(
                v1.PointStart.X, Math.Min(v2.PointStart.X, v2.PointEnd.X), Math.Max(v2.PointStart.X, v2.PointEnd.X),
                v2.PointStart.Y, Math.Min(v1.PointStart.Y, v1.PointEnd.Y), Math.Max(v1.PointStart.Y, v1.PointEnd.Y)
            );
        }
    }

    private static bool CheckCrossing(int verticalX, int minHorizontalX, int maxHorizontalX,
                                      int horizontalY, int minVerticalY, int maxVerticalY)
    {
        return verticalX >= minHorizontalX && verticalX <= maxHorizontalX &&
               horizontalY >= minVerticalY && horizontalY <= maxVerticalY;
    }
}

public struct Point(int x, int y)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public override readonly string ToString() => $"({X}, {Y})";
}

class Rectangle(Point topLeft, Point bottomRight)
{
    public Point TopLeft { get; set; } = topLeft;
    public Point BottomRight { get; set; } = bottomRight;
    public long AreaL => (long)(Math.Abs(BottomRight.X - TopLeft.X) + 1) * (long)(Math.Abs(BottomRight.Y - TopLeft.Y) + 1);

    public override string ToString() => $"TopLeft: {TopLeft}, BottomRight: {BottomRight}, Area: {AreaL}";
}
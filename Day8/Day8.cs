var lines = File.ReadAllLines("Day8.txt");
var vectorsToLookAt = 1000;

var coordinates = lines
    .Select(line =>
    {
        var parts = line.Split(',');
        return new Coordinate(
            int.Parse(parts[0]),
            int.Parse(parts[1]),
            int.Parse(parts[2])
        );
    })
    .ToList();


var sortedVectorsCombinations = coordinates
    .SelectMany((coord, i) => coordinates
        .Skip(i + 1)
        .Select(other => new Vector(coord, other)))
    .OrderBy(v => v.Distance)
    .ToList();

var circuit = new Circuit(coordinates
    .Select(coord => new List<Coordinate> { coord })
    .ToList());

Console.WriteLine($"Initial junction boxes: {circuit.GetJunctionBoxCount()}");

// Part 1
/*
    First we create vectors for all coordinate pairs, sorted by distance
    .Skip(i + 1) covers each combination only once (A->B is the same as B->A)
    then we iterate through the shortest vectors and combine the junction boxes they connect
*/
for (int i = 0; i < Math.Min(vectorsToLookAt, sortedVectorsCombinations.Count); i++)
{
    circuit.Combine(sortedVectorsCombinations[i]);
    //Console.WriteLine($"Combined vector {i + 1}: boxes remaining = {circuit.GetJunctionBoxCount()}");
}

Console.WriteLine($"\nTotal junction boxes: {circuit.GetJunctionBoxCount()}");
//Console.WriteLine(circuit.ToString());
Console.WriteLine($"Multiply top three junction boxes: {circuit.GetThreeLargestJunctionBoxProduct()}");

//Part 2
/*
    Same as part 1, but we continue combining vectors until only one junction box remains
*/
var circuit2 = new Circuit(coordinates
    .Select(coord => new List<Coordinate> { coord })
    .ToList());

int vectorIndex = 0;
while (vectorIndex < sortedVectorsCombinations.Count && !circuit2.Combine(sortedVectorsCombinations[vectorIndex]))
{
    vectorIndex++;
}

Console.WriteLine($"\nTotal junction boxes part 2: {circuit2.GetJunctionBoxCount()}");
Console.WriteLine($"X Distance of last combined vector: {circuit2.GetXDistanceOfLastVector()}");

struct Coordinate(int x, int y, int z)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public int Z { get; set; } = z;
    public override readonly string ToString() => $"({X}, {Y}, {Z})";
}

struct Vector
{
    public Coordinate From { get; set; }
    public Coordinate To { get; set; }
    public double Distance { get; set; }

    public Vector(Coordinate from, Coordinate to)
    {
        From = from;
        To = to;

        // Calculate Euclidean/straight-line distance
        // https://en.wikipedia.org/wiki/Euclidean_distance
        double dx = to.X - from.X;
        double dy = to.Y - from.Y;
        double dz = to.Z - from.Z;
        Distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public readonly long XDistance() => (long)To.X * From.X;
}

class Circuit(List<List<Coordinate>> junctionBoxes)
{
    public List<List<Coordinate>> JunctionBoxes { get; private set; } = junctionBoxes;
    public Vector LastVector { get; private set; }

    // Combines two junction boxes connected by the given vector
    // Returns true if only one junction box remains after combining
    // Not my best work, but it flags loop to stop when only one box remains
    public bool Combine(Vector vector)
    {
        var boxFrom = JunctionBoxes.FirstOrDefault(box => box.Contains(vector.From));
        var boxTo = JunctionBoxes.FirstOrDefault(box => box.Contains(vector.To));

        if (boxFrom == boxTo)
            return false;

        JunctionBoxes.Remove(boxFrom);
        JunctionBoxes.Remove(boxTo);

        var combinedBox = new List<Coordinate>();
        if (boxFrom != null) combinedBox.AddRange(boxFrom);
        if (boxTo != null) combinedBox.AddRange(boxTo);

        JunctionBoxes.Add(combinedBox);

        if (GetJunctionBoxCount() == 1)
        {
            LastVector = vector;
            return true;
        }

        return false;
    }

    public int GetJunctionBoxCount() => JunctionBoxes.Count;

    public long GetXDistanceOfLastVector() => LastVector.XDistance();

    public long GetThreeLargestJunctionBoxProduct()
    {
        return JunctionBoxes
            .OrderByDescending(box => box.Count)
            .Take(3)
            .Aggregate(1L, (acc, box) => acc * box.Count);
    }

    public override string ToString()
    {
        return string.Join("\n", JunctionBoxes
        .OrderByDescending(box => box.Count)
        .Take(10)
        .Select((box, index) =>
            $"Box {index + 1}: [{string.Join(", ", box)}]"));
    }
}

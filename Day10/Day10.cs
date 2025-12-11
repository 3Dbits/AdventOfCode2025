var lines = File.ReadAllLines("Day10.txt");

var machines = lines
    .Select(line => Machine.Parse(line))
    .ToList();

Console.WriteLine($"Total machines: {machines.Count}");

// Part 1
var count = 0;
foreach (var (machine, index) in machines.Select((m, i) => (m, i)))
{
    Console.WriteLine($"\nMachine {index + 1}:");
    Console.WriteLine($"  Buttons: {machine.Buttons}");

    var fewestPresses = machine.FindFewestPresses();
    count += fewestPresses >= 0 ? fewestPresses : 0;

    if (fewestPresses >= 0)
    {
        Console.WriteLine($"  Fewest presses to turn all lights on: {fewestPresses}");
    }
    else
    {
        Console.WriteLine($"  Cannot turn all lights on within 10 presses");
    }
}
//var fewestPresses = machines[2].FindFewestPresses();
Console.WriteLine($"\nSum of fewest presses for all machines: {count}");

// Part 2 - Joltage version
var count2 = 0;
machines = machines
    .OrderBy(m => m.Joltages.Values.Sum() + m.Joltages.Values.Count)
    //.Take(35)
    .Skip(44)
    .Take(10)
    .ToList();
foreach (var (machine, index) in machines.Select((m, i) => (m, i)))
{
    Console.WriteLine($"\nMachine {index + 1}:");
    Console.WriteLine($"  Joltages: {machine.Joltages}");

    var fewestPresses2 = machine.FindFewestPressesForJoltage();
    count2 += fewestPresses2 >= 0 ? fewestPresses2 : 0;

    if (fewestPresses2 >= 0)
    {
        Console.WriteLine($" Fewest presses to have all joltages on: {fewestPresses2}");
    }
    else
    {
        Console.WriteLine($" Cannot have all joltages on within 100 presses");
    }
}
//var fewestPresses = machines[2].FindFewestPresses();
Console.WriteLine($"\nSum of fewest presses for all machines: {count2}");

class Machine(Buttons buttons, List<List<int>> buttonCombinations, Joltage joltages)
{
    public Buttons Buttons { get; set; } = buttons;
    public List<List<int>> ButtonCombinations { get; set; } = buttonCombinations;
    public Joltage Joltages { get; set; } = joltages;

    public int FindFewestPresses()
    {
        var targetState = Buttons.Lights.ToList();
        var initialState = Enumerable.Repeat(false, targetState.Count).ToList();
        var queue = new Queue<(List<bool> state, int presses, List<int> path)>();
        var visited = new Dictionary<string, int>();

        var initialKey = StateToKey(initialState);
        queue.Enqueue((initialState, 0, new List<int>()));
        visited[initialKey] = 0;

        //Console.WriteLine($"Initial state: [{StateToString(initialState)}]");
        //Console.WriteLine($"Target state:  [{StateToString(targetState)}]");

        while (queue.Count > 0)
        {
            var (currentState, pressCount, path) = queue.Dequeue();

            // Check if we reached the target state
            if (currentState.SequenceEqual(targetState))
            {
                /*Console.WriteLine($"\nSolution found with {pressCount} presses!");
                Console.WriteLine($"Path: {string.Join(" → ", path.Select(i => $"Combo{i}({string.Join(",", ButtonCombinations[i])})"))}");
                Console.WriteLine($"\nButton sequence:");
                foreach (var (comboIndex, step) in path.Select((idx, step) => (idx, step + 1)))
                {
                    Console.WriteLine($"  Step {step}: Press Combo{comboIndex} - buttons ({string.Join(",", ButtonCombinations[comboIndex])})");
                }
                Console.WriteLine($"Final state: [{StateToString(currentState)}]");*/
                return pressCount;
            }

            if (pressCount >= 10)
                continue;

            for (int i = 0; i < ButtonCombinations.Count; i++)
            {
                var newState = currentState.ToList();
                var combination = ButtonCombinations[i];

                foreach (var buttonIndex in combination)
                {
                    if (buttonIndex >= 0 && buttonIndex < newState.Count)
                    {
                        newState[buttonIndex] = !newState[buttonIndex];
                    }
                }

                var stateKey = StateToKey(newState);
                var newPressCount = pressCount + 1;

                if (!visited.ContainsKey(stateKey) || visited[stateKey] > newPressCount)
                {
                    visited[stateKey] = newPressCount;
                    var newPath = new List<int>(path) { i };
                    queue.Enqueue((newState, newPressCount, newPath));

                    var indent = new string(' ', pressCount * 2);
                    //Console.WriteLine($"{indent}Press {newPressCount}: Combo {i} ({string.Join(",", combination)}) → [{StateToString(newState)}]");
                }
            }
        }

        Console.WriteLine("\nNo solution found within 10 presses");
        return -1;
    }

    private string StateToKey(List<bool> state) => string.Join("", state.Select(b => b ? '1' : '0'));
    private string StateToString(List<bool> state) => string.Join("", state.Select(b => b ? '#' : '.'));

    public int FindFewestPressesForJoltage()
    {
        var targetState = Joltages.Values.ToList();
        var initialState = Enumerable.Repeat(0, targetState.Count).ToList();
        var queue = new Queue<(List<int> state, int presses, List<int> path)>();
        var visited = new Dictionary<string, int>();

        var initialKey = JoltageStateToKey(initialState);
        queue.Enqueue((initialState, 0, new List<int>()));
        visited[initialKey] = 0;

        while (queue.Count > 0)
        {
            var (currentState, pressCount, path) = queue.Dequeue();

            if (currentState.SequenceEqual(targetState))
            {
                return pressCount;
            }

            if (pressCount >= 100)
                continue;

            for (int i = 0; i < ButtonCombinations.Count; i++)
            {
                var newState = currentState.ToList();
                var combination = ButtonCombinations[i];

                foreach (var buttonIndex in combination)
                {
                    if (buttonIndex >= 0 && buttonIndex < newState.Count)
                    {
                        newState[buttonIndex]++;
                    }
                }

                bool exceededTarget = false;
                for (int j = 0; j < newState.Count; j++)
                {
                    if (newState[j] > targetState[j])
                    {
                        exceededTarget = true;
                        break;
                    }
                }

                if (exceededTarget)
                    continue;

                var stateKey = JoltageStateToKey(newState);
                var newPressCount = pressCount + 1;

                if (!visited.ContainsKey(stateKey) || visited[stateKey] > newPressCount)
                {
                    visited[stateKey] = newPressCount;
                    var newPath = new List<int>(path) { i };
                    queue.Enqueue((newState, newPressCount, newPath));
                }
            }
        }

        return -1;
    }

    private string JoltageStateToKey(List<int> state) => string.Join(",", state);

    public static Machine Parse(string line)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var buttonsStr = parts[0];
        var buttons = Buttons.Parse(buttonsStr);

        var buttonCombinations = new List<List<int>>();
        var joltageIndex = -1;

        for (int i = 1; i < parts.Length; i++)
        {
            if (parts[i].StartsWith('{'))
            {
                joltageIndex = i;
                break;
            }

            if (parts[i].StartsWith('('))
            {
                var combo = parts[i].Trim('(', ')')
                    .Split(',')
                    .Select(int.Parse)
                    .ToList();
                buttonCombinations.Add(combo);
            }
        }

        var joltagesStr = parts[joltageIndex];
        var joltages = Joltage.Parse(joltagesStr);

        return new Machine(buttons, buttonCombinations, joltages);
    }
}

class Buttons(List<bool> lights)
{
    public List<bool> Lights { get; set; } = lights;

    public static Buttons Parse(string input)
    {
        var cleaned = input.Trim('[', ']');
        var lights = cleaned.Select(c => c == '#').ToList();
        return new Buttons(lights);
    }

    public int LightCount => Lights.Count(l => l);

    public override string ToString()
    {
        var lightStr = string.Join("", Lights.Select(l => l ? '#' : '.'));
        return $"[{lightStr}] (On: {LightCount}/{Lights.Count})";
    }
}

class Joltage(List<int> values)
{
    public List<int> Values { get; set; } = values;

    public static Joltage Parse(string input)
    {
        var cleaned = input.Trim('{', '}');
        var values = cleaned.Split(',').Select(int.Parse).ToList();
        return new Joltage(values);
    }

    public override string ToString()
    {
        return $"{{{string.Join(",", Values)}}}";
    }
}

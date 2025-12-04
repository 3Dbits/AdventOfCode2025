// Read and parse the file
var numberRows = File.ReadAllLines("Day3.txt")
    .Select(line => line
        .Where(char.IsDigit)
        .Select(c => c - '0')
        .ToList())
    .ToList();

Console.WriteLine($"Total rows: {numberRows.Count}");
Console.WriteLine($"First row has {numberRows[0].Count} digits");
Console.WriteLine($"First few digits of first row: {string.Join(", ", numberRows[0].Take(10))}");

// Part 1:
// Find the biggest number in list
// If there is two biggest numbers, use both for final number
// Else look at the right side of list after first biggest number for the second biggest in the rest of the list
// If there is none, look at the left side of list before the first biggest number for the second biggest in the rest of the list
var totalSum = numberRows
    .Select(row =>
    {
        int biggestDigit = row.Max();
        
        var biggestIndices = row
            .Select((digit, index) => new { digit, index })
            .Where(x => x.digit == biggestDigit)
            .Select(x => x.index)
            .ToList();

        int firstDigit, secondDigit;
        int firstIndex, secondIndex;

        if (biggestIndices.Count >= 2)
        {
            firstDigit = biggestDigit;
            secondDigit = biggestDigit;
            firstIndex = biggestIndices[0];
            secondIndex = biggestIndices[1];
        }
        else
        {
            firstDigit = biggestDigit;
            firstIndex = biggestIndices[0];

            var digitsAfter = row
                .Skip(firstIndex + 1)
                .ToList();

            if (digitsAfter.Any())
            {
                secondDigit = digitsAfter.Max();
                secondIndex = row.IndexOf(secondDigit, firstIndex + 1);
            }
            else
            {
                var digitsBefore = row
                    .Take(firstIndex)
                    .ToList();
                secondDigit = digitsBefore.Max();
                secondIndex = row.LastIndexOf(secondDigit, firstIndex - 1);
            }
        }

        int combinedNumber = firstIndex < secondIndex
            ? firstDigit * 10 + secondDigit
            : secondDigit * 10 + firstDigit;

        return combinedNumber;
    })
    .ToList();

//foreach (var num in totalSum)
//   Console.WriteLine($"Combined number: {num}");
Console.WriteLine($"Total sum of combined numbers: {totalSum.Sum()}");

// Part 2:
// Prat 1 is not valid anymore :)
// For each position n (0 to 11), find the leftmost occurrence of the highest digit
// in the valid search window: from (position after previous digit) to (12-n positions from end)
var totalSumPart2 = numberRows
    .Select(row =>
    {
        if (row.Count < 12)
            return 0L;
        
        var selectedDigits = new List<int>();
        int searchStart = 0;
        

        for (int digitPosition = 0; digitPosition < 12; digitPosition++)
        {
            int digitsRemaining = 12 - digitPosition - 1; // How many more digits we need after this one
            int searchEnd = row.Count - digitsRemaining; // We must leave enough digits for the rest
            
            int maxDigit = -1;
            int maxDigitIndex = -1;
            
            for (int i = searchStart; i < searchEnd; i++)
            {
                if (row[i] > maxDigit)
                {
                    maxDigit = row[i];
                    maxDigitIndex = i;
                }
            }
            
            selectedDigits.Add(maxDigit);
            searchStart = maxDigitIndex + 1;
        }
        
        long number = 0;
        foreach (var digit in selectedDigits)
        {
            number = number * 10 + digit;
        }
        
        return number;
    })
    .ToList();

//foreach (var num in totalSumPart2)
//    Console.WriteLine($"Combined number (Part 2): {num}");
Console.WriteLine($"Total sum of combined numbers (Part 2): {totalSumPart2.Sum(x => (long)x)}");
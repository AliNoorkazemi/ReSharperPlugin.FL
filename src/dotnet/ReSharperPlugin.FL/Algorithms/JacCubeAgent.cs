using System;
using System.Collections.Generic;
using System.Linq;

namespace ReSharperPlugin.FL.Algorithms;

public class JacCubeAgent
{
    private readonly Dictionary<string, double> _linesRanks = new();

    public void ExecuteLineRanks(LineExecutionDataSet dataSet, int totalFailing)
    {
        foreach (var lineExecutionData in dataSet.LineExecutions)
        {
            var lineSuspiciousness = GetLineSuspiciousness(totalFailing,
                lineExecutionData.Value.Failing,
                lineExecutionData.Value.Succeeding);
            
            _linesRanks.Add(lineExecutionData.Key, lineSuspiciousness);
        }
    }

    private static double GetLineSuspiciousness(double totalFailing, 
        double lineFailing,
        double lineSucceeding)
    {
        return lineFailing / Cbrt(totalFailing + lineSucceeding);
    }

    private static double Cbrt(double num)
    {
        return Math.Pow(num, (double)1 / 3);
    }
    
    public IReadOnlyCollection<string> GetSuspiciousLines()
    {
        var sortedDictionary = _linesRanks.OrderByDescending(pair => pair.Value).ToList();

        if (!sortedDictionary.Any())
        {
            return Array.Empty<string>();
        }

        var highestValues = sortedDictionary.Select(v => v.Value).Distinct().Take(3).ToArray();

        return sortedDictionary
            .Where(pair => highestValues.Any(highestValue => Math.Abs(pair.Value - highestValue) < 0.000001))
            .Select(pair => pair.Key)
            .ToList();
    }
}
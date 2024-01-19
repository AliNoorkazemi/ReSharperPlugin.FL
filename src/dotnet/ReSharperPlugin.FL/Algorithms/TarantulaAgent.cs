using System;
using System.Collections.Generic;
using System.Linq;
using ReSharperPlugin.FL.Models;

namespace ReSharperPlugin.FL.Algorithms;

public class TarantulaAgent
{
    private readonly Dictionary<string, double> _linesRanks = new();

    public void ExecuteLineRanks(LineExecutionDataSet dataSet, int totalFailing, int totalSucceeding)
    {
        foreach (var lineExecutionData in dataSet.LineExecutions)
        {
            var lineSuspiciousness = GetLineSuspiciousness(totalFailing,
                totalSucceeding,
                lineExecutionData.Value.Failing,
                lineExecutionData.Value.Succeeding);
            
            _linesRanks.Add(lineExecutionData.Key, lineSuspiciousness);
        }
    }

    private static double GetLineSuspiciousness(double totalFailing, 
        double totalSucceeding, 
        double lineFailing,
        double lineSucceeding)
    {
        if (totalSucceeding == 0)
        {
            return lineFailing / totalFailing /
                (1 + lineFailing / totalFailing);
        }
        
        return lineFailing / totalFailing /
               (lineSucceeding / totalSucceeding + lineFailing / totalFailing);
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
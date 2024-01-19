using System;
using System.Collections.Generic;
using System.Linq;
using ReSharperPlugin.FL.Models;

namespace ReSharperPlugin.FL.Algorithms;

public class SBIAgent
{
    private readonly Dictionary<string, double> _linesRanks = new();

    public void ExecuteLineRanks(LineExecutionDataSet dataSet)
    {
        foreach (var lineExecutionData in dataSet.LineExecutions)
        {
            var lineSuspiciousness = GetLineSuspiciousness(lineExecutionData.Value.Failing,
                lineExecutionData.Value.Succeeding);
            
            _linesRanks.Add(lineExecutionData.Key, lineSuspiciousness);
        }
    }

    private static double GetLineSuspiciousness(double lineFailing, double lineSucceeding)
    {
        return lineFailing / (lineFailing + lineSucceeding);
    }
    
    public IReadOnlyCollection<string> GetSuspiciousLines()
    {
        var sortedDictionary = _linesRanks.OrderByDescending(pair => pair.Value).ToList();

        if (!sortedDictionary.Any())
        {
            return Array.Empty<string>();
        }

        var highestValue = sortedDictionary.First().Value;

        return sortedDictionary
            .Where(pair => Math.Abs(pair.Value - highestValue) < 0.000001)
            .Select(pair => pair.Key)
            .ToList();
    }
}
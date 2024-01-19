using System;
using System.Collections.Generic;
using System.Linq;

namespace ReSharperPlugin.FL.Algorithms;

public class BaseAgent
{
    protected readonly Dictionary<string, double> _linesRanks = new();
    
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
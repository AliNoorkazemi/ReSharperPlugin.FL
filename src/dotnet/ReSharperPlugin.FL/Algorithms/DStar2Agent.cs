using System;
using System.Collections.Generic;
using System.Linq;
using ReSharperPlugin.FL.Models;

namespace ReSharperPlugin.FL.Algorithms;

public class DStar2Agent : BaseAgent
{
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
        if (lineSucceeding + totalFailing - lineFailing == 0)
        {
            return Math.Pow(totalFailing, 2) + 1;
        }
        
        return Math.Pow(lineFailing, 2) / (lineSucceeding + totalFailing - lineFailing);
    }
}
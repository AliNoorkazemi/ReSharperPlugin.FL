﻿using ReSharperPlugin.FL.Models;

namespace ReSharperPlugin.FL.Algorithms;

public class JaccardAgent : BaseAgent
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
        return lineFailing / (totalFailing + lineSucceeding);
    }
}
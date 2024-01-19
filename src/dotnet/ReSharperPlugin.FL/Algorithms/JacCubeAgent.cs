using System;
using ReSharperPlugin.FL.Models;

namespace ReSharperPlugin.FL.Algorithms;

public class JacCubeAgent : BaseAgent
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
        return lineFailing / Cbrt(totalFailing + lineSucceeding);
    }

    private static double Cbrt(double num)
    {
        return Math.Pow(num, (double)1 / 3);
    }
}
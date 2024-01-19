using ReSharperPlugin.FL.Models;

namespace ReSharperPlugin.FL.Algorithms;

public class Opt2Agent : BaseAgent
{
    public void ExecuteLineRanks(LineExecutionDataSet dataSet, int totalSucceeding)
    {
        foreach (var lineExecutionData in dataSet.LineExecutions)
        {
            var lineSuspiciousness = GetLineSuspiciousness(totalSucceeding,
                lineExecutionData.Value.Failing,
                lineExecutionData.Value.Succeeding);
            
            _linesRanks.Add(lineExecutionData.Key, lineSuspiciousness);
        }
    }

    private static double GetLineSuspiciousness(double totalSucceeding, 
        double lineFailing,
        double lineSucceeding)
    {
        return lineFailing - lineSucceeding / (totalSucceeding + 1);
    }
}
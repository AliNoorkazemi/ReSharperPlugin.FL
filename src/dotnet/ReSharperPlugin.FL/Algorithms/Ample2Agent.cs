using ReSharperPlugin.FL.Models;

namespace ReSharperPlugin.FL.Algorithms;

public class Ample2Agent : BaseAgent
{
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
            return lineFailing / totalFailing;
        }

        if (totalFailing == 0)
        {
            return 1 - lineSucceeding / totalSucceeding;
        }
        
        return lineFailing / totalFailing - lineSucceeding / totalSucceeding;
    }
}
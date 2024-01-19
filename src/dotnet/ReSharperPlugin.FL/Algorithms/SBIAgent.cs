using ReSharperPlugin.FL.Models;

namespace ReSharperPlugin.FL.Algorithms;

public class SBIAgent : BaseAgent
{
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
}
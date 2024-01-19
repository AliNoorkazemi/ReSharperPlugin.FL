using System.Collections.Generic;

namespace ReSharperPlugin.FL.Models;

public class LineExecutionDataSet
{
    public readonly Dictionary<string, LineRecord> LineExecutions;

    public LineExecutionDataSet()
    {
        LineExecutions = new Dictionary<string, LineRecord>();
    }

    public void AddOrUpdateLineExecutionsFailing(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (LineExecutions.TryGetValue(line, out var lineExecutionRecord))
            {
                lineExecutionRecord.Failing++;    
            }
            else
            {
                LineExecutions[line] = new LineRecord { Failing = 1 };
            }
        }
    }

    public void AddOrUpdateLineExecutionsSucceeding(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (LineExecutions.TryGetValue(line, out var lineExecutionRecord))
            {
                lineExecutionRecord.Succeeding++;    
            }
            else
            {
                LineExecutions[line] = new LineRecord { Succeeding = 1 };
            }
        }
    }
}
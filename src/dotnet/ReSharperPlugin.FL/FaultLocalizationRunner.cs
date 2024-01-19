using System;
using System.Collections.Generic;
using System.IO;
using ReSharperPlugin.FL.Algorithms;
using ReSharperPlugin.FL.Compilation;
using ReSharperPlugin.FL.Instrumentation;
using ReSharperPlugin.FL.Models;

namespace ReSharperPlugin.FL;

public class FaultLocalizationRunner
{
    private readonly IEnumerable<TestCase> _testCases;
    private readonly string _codeContent;

    public FaultLocalizationRunner(IEnumerable<TestCase> testCases, string codeContent)
    {
        _testCases = testCases;
        _codeContent = codeContent;
    }

    public string GetFaultLocalizationReport()
    {
        var compiler = new Compiler();
        var emitResult = compiler.Compile(_codeContent);

        var lineExecutionDataSet = new LineExecutionDataSet();

        if (emitResult.Success)
        {
            var outputWriter = new StringWriter();
            Console.SetOut(outputWriter);

            var totalFailing = 0;
            var totalSucceeding = 0;

            foreach (var testCase in _testCases)
            {
                var runSuccessfully = compiler.Run(testCase.Input);

                var outputConsole = outputWriter.ToString();
                var executedLines = InstrumentFactory.GetExecutedLines(outputConsole);

                var actualOutput = InstrumentFactory.GetActualOutput(outputConsole).TrimEnd('\r');

                if (runSuccessfully)
                {
                    if (actualOutput == testCase.Output)
                    {
                        lineExecutionDataSet.AddOrUpdateLineExecutionsSucceeding(executedLines);
                        totalSucceeding++;
                    }
                    else
                    {
                        lineExecutionDataSet.AddOrUpdateLineExecutionsFailing(executedLines);
                        totalFailing++;
                    }

                    var sb = outputWriter.GetStringBuilder();
                    sb.Remove(0, sb.Length);
                }
                else
                {
                    return "RunTime error";
                }
            }

            outputWriter.Dispose();
            compiler.Dispose();

            var tarantulaAgent = new TarantulaAgent();
            tarantulaAgent.ExecuteLineRanks(lineExecutionDataSet, totalFailing, totalSucceeding);

            var tarantulaFaultLocations = tarantulaAgent.GetSuspiciousLines();

            var ochiaiAgent = new OchiaiAgent();
            ochiaiAgent.ExecuteLineRanks(lineExecutionDataSet, totalFailing);

            var ochiaiFaultLocations = ochiaiAgent.GetSuspiciousLines();

            var jaccardAgent = new JaccardAgent();
            jaccardAgent.ExecuteLineRanks(lineExecutionDataSet, totalFailing);

            var jaccardFaultLocations = jaccardAgent.GetSuspiciousLines();

            var museAgent = new MuseAgent();
            museAgent.ExecuteLineRanks(lineExecutionDataSet, totalFailing, totalSucceeding);

            var museFaultLocations = museAgent.GetSuspiciousLines();

            var opt2Agent = new Opt2Agent();
            opt2Agent.ExecuteLineRanks(lineExecutionDataSet, totalSucceeding);

            var opt2FaultLocations = opt2Agent.GetSuspiciousLines();

            var barinelAgent = new BarinelAgent();
            barinelAgent.ExecuteLineRanks(lineExecutionDataSet);

            var barinelFaultLocations = barinelAgent.GetSuspiciousLines();

            var dStar2Agent = new DStar2Agent();
            dStar2Agent.ExecuteLineRanks(lineExecutionDataSet, totalFailing);

            var dStar2FaultLocations = dStar2Agent.GetSuspiciousLines();

            var sbiAgent = new SBIAgent();
            sbiAgent.ExecuteLineRanks(lineExecutionDataSet);

            var sbiFaultLocations = sbiAgent.GetSuspiciousLines();

            var jacCubeAgent = new JacCubeAgent();
            jacCubeAgent.ExecuteLineRanks(lineExecutionDataSet, totalFailing);

            var jacCubeFaultLocations = jacCubeAgent.GetSuspiciousLines();

            var ample2Agent = new Ample2Agent();
            ample2Agent.ExecuteLineRanks(lineExecutionDataSet, totalFailing, totalSucceeding);

            var ample2FaultLocations = ample2Agent.GetSuspiciousLines();

            return GetReportText(tarantulaFaultLocations,
                ochiaiFaultLocations,
                jaccardFaultLocations,
                museFaultLocations,
                opt2FaultLocations,
                barinelFaultLocations,
                dStar2FaultLocations,
                sbiFaultLocations,
                jacCubeFaultLocations,
                ample2FaultLocations);
        }

        return "Compile error";
    }

    private static string GetReportText(IEnumerable<string> tarantulaFaultLocations,
        IEnumerable<string> ochiaiFaultLocations, 
        IEnumerable<string> jaccardFaultLocations, 
        IEnumerable<string> museFaultLocations, 
        IEnumerable<string> opt2FaultLocations, 
        IEnumerable<string> barinelFaultLocations, 
        IEnumerable<string> dStar2FaultLocations, 
        IEnumerable<string> sbiFaultLocations, 
        IEnumerable<string> jacCubeFaultLocations, 
        IEnumerable<string> ample2FaultLocations)
    {
        var report = new string[]
        {
            $"Tarantula suspected faulty lines => {string.Join(",", tarantulaFaultLocations)}",
            $"Ochiai suspected faulty lines => {string.Join(",", ochiaiFaultLocations)}",
            $"Jaccard suspected faulty lines => {string.Join(",", jaccardFaultLocations)}",
            $"Muse suspected faulty lines => {string.Join(",", museFaultLocations)}",
            $"Op2 suspected faulty lines => {string.Join(",", opt2FaultLocations)}",
            $"Barinel suspected faulty lines => {string.Join(",", barinelFaultLocations)}",
            $"DStar suspected faulty lines => {string.Join(",", dStar2FaultLocations)}",
            $"SBI suspected faulty lines => {string.Join(",", sbiFaultLocations)}",
            $"JacCube suspected faulty lines => {string.Join(",", jacCubeFaultLocations)}",
            $"Ample suspected faulty lines => {string.Join(",", ample2FaultLocations)}"
        };

        return string.Join("\n", report);
    }
}
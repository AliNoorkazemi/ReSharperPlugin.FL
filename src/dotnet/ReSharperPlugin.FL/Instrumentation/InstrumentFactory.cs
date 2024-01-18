using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReSharperPlugin.FL.Instrumentation;

public static class InstrumentFactory
{
    private const string InstrumentCode = "db24733c-5d52-4aad-bd7e-92d4cde5ff5b";

    public static string ReturnInstrumentedCode(string originalCode)
    {
        var originalLines = originalCode.Split('\r').ToList();
        var newLines = new List<string>(originalLines.Count);

        for (var i = 0; i < originalLines.Count ; i++)
        {
            var originalLine = originalLines.ElementAt(i);

            if (ContainsNoneInstrumentedWords(originalLine) ||
                InvalidLines().Match(originalLine).Success ||
                IsComment(originalLine) ||
                (
                    !IsControlFlowAction(originalLine) &&
                    !IsEndWithSemicolon(originalLine)
                )
               )
            {
                newLines.Add(originalLine);
                continue;
            }

            var newLine = AppendInstrumentationAtStart(originalLine, i + 1);
            
            newLines.Add(newLine);
        }

        return AppendAllLinesTogether(newLines);
    }

    private static bool ContainsNoneInstrumentedWords(string line)
    {
        return line.TrimStart('\n').TrimStart('\r').Split(' ').Any(word => NoneInstrumentedWords.Contains(word));
    }

    private static IEnumerable<string> NoneInstrumentedWords => new[]
    {
        "using", "public", "static", "private", "protected", "internal", "class", "interface", "try", "catch", "const",
        "finally", "do", "struct", "#region", "#endregion", "enum", "else"
    };

    private static bool IsEndWithSemicolon(string originalLine)
    {
        return originalLine.TrimEnd(' ').TrimEnd('\r').EndsWith(";");
    }

    private static bool IsControlFlowAction(string originalLine)
    {
        return ControlFlowActions.Any(c => originalLine.TrimStart(' ').StartsWith(c));
    }

    private static IEnumerable<string> ControlFlowActions => new[]
    {
        "for", "if", "foreach", "while"
    };

    private static bool IsComment(string originalLine)
    {
        return originalLine.TrimStart(' ').StartsWith("//");
    }

    private static string AppendInstrumentationAtStart(string originalLine, int lineNumber)
    {
        return $"Console.WriteLine($\"\\n{lineNumber}#{InstrumentCode}\"); {originalLine}";
    }

    private static string AppendAllLinesTogether(IEnumerable<string> lines)
    {
        return string.Join("\n", lines);
    }

    private static Regex InvalidLines()
    {
        return new Regex("^[\\s\\{\\}\\(\\)\\;\\,]+$");
    }

    public static IReadOnlyCollection<string> GetExecutedLines(string outputConsole)
    {
        return outputConsole.Split('\n')
            .Where(line => line.Contains(InstrumentCode))
            .Select(line => line.Split('#')[0])
            .Distinct()
            .ToList();
    }

    public static string GetActualOutput(string outputConsole)
    {
        var filteredStrings = outputConsole.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !line.Contains(InstrumentCode))
            .ToArray();

        return string.Join("", filteredStrings);
    }
}
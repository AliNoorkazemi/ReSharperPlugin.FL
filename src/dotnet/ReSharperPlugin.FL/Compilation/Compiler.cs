using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using ReSharperPlugin.FL.Instrumentation;

namespace ReSharperPlugin.FL.Compilation;

public class Compiler : IDisposable
{
    private const string MainMethodName = "Main";
    
    private string MainClassName { get; set; }
    
    private object?[] MainMethodArgs { get; set; }
    
    private const string AssemblyName = "FakeAssembly";

    private readonly MemoryStream _memoryStream;

    public Compiler()
    {
        _memoryStream = new MemoryStream();
    }

    public EmitResult Compile(string codeContent)
    {
        codeContent = SetMainMethodAsPublic(codeContent);
        
        var instrumentedCode = InstrumentFactory.ReturnInstrumentedCode(codeContent);

        var realNamespace = GetNameSpace(codeContent);

        MainClassName = realNamespace is not null ? $"{realNamespace}.Program" : "Program";

        MainMethodArgs = GetMainMethodArgs(codeContent);

        var syntaxTree = SyntaxFactory.ParseSyntaxTree(instrumentedCode);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        var compilation = CSharpCompilation.Create(AssemblyName)
            .WithOptions(compilationOptions)
            .AddReferences(GetMetadataReferences())
            .AddSyntaxTrees(syntaxTree);
        
        return compilation.Emit(_memoryStream);
    }

    private static string SetMainMethodAsPublic(string codeContent)
    {
        return codeContent.Contains("public static void Main")
            ? codeContent
            : codeContent.Replace("static void Main", "public static void Main");
    }

    private static object?[] GetMainMethodArgs(string codeContent)
    {
        return codeContent.Contains("static void Main()")
            ? Array.Empty<object?>()
            : new object?[] { Array.Empty<string>() };
    }

    private static string? GetNameSpace(string originalCode)
    {
        var lines = originalCode.Split('\n').ToList();

        var namespaceLine = lines.FirstOrDefault(l => l.Contains("namespace"));

        return namespaceLine?
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .ElementAt(1)
            .Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries)
            .ElementAt(0);
    }

    private static MetadataReference[] GetMetadataReferences()
    {
        return new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Queue<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Convert).Assembly.Location),
            MetadataReference.CreateFromFile(
                typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Text.RegularExpressions").Location),
        };
    }

    public bool Run(string input)
    {
        try
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);

            var assembly = Assembly.Load(_memoryStream.ToArray());

            var libraryClassType = assembly.GetType(MainClassName);
            var libraryInstance = Activator.CreateInstance(libraryClassType!);
            var libraryMethod = libraryClassType!.GetMethod(MainMethodName);

            using var stringReader = new StringReader(input);
        
            Console.SetIn(stringReader);

            try
            {
                libraryMethod!.Invoke(libraryInstance, MainMethodArgs);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void Dispose()
    {
        _memoryStream.Dispose();
    }
}
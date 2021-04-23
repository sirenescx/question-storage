using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace QuestionStorage.Helpers.Generation
{
    public class AssemblyBuilder
    {
        private static readonly QRandom Random = new QRandom();
        
        private static readonly IEnumerable<string> DefaultNamespaces = new[]
        {
            "System",
            "System.IO",
            "System.Net",
            "System.Linq",
            "System.Text.RegularExpressions",
            "System.Collections.Generic"
        };
        
        private static int GenerateAssemblyNumber() =>
            int.Parse(Random.StringFromRegex(
                "([1-8][0-9]{6}|9[0-8][0-9]{5}|99[0-8][0-9]{4}|999[0-8][0-9]{3}|9999[0-8][0-9]{2}|99999[0-8][0-9]|999999[0-9]|10000000)"));
        
        internal static Assembly CompileSourceRoslyn(string sourceCode)
        {
            using (var ms = new MemoryStream())
            {
                var assemblyFileName = $"QStorage{GenerateAssemblyNumber()}.dll";
                var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

                var compilation = CSharpCompilation.Create(assemblyFileName,
                    new[] {CSharpSyntaxTree.ParseText(sourceCode)},
                    new[]
                    {
                        MetadataReference.CreateFromFile(typeof(Common).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.Extensions.dll"))
                    },
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithUsings(DefaultNamespaces)
                );

                var compilationResult = compilation.Emit(ms);
                if (!compilationResult.Success)
                {
                    foreach (var issue in compilationResult.Diagnostics
                        .Select(codeIssue => $"ID: {codeIssue.Id}, " +
                                             $"Message: {codeIssue.GetMessage()}, " +
                                             $"Location: {codeIssue.Location.GetLineSpan()}, " +
                                             $"Severity: {codeIssue.Severity} "))
                    {
                        Console.WriteLine(issue);
                    }
                }

                var assembly = Assembly.Load(ms.ToArray());

                return assembly;
            }
        }
    }
}
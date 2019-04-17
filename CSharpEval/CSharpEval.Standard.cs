#if STANDARD
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.Loader;
using System.IO;
using System.Reflection;
using System.Linq;

namespace RandomSolutions
{
    public partial class CSharpEval
    {
        static MethodInfo _compile(string code, IEnumerable<Assembly> refs)
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            var references = _references.Concat((refs ?? new[] { Assembly.GetEntryAssembly(), Assembly.GetCallingAssembly() }).Select(x=>x.Location)).Distinct();

            var compilation = CSharpCompilation.Create(
                assemblyName: Path.GetRandomFileName(),
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(code) },
                references: references.Select(x => MetadataReference.CreateFromFile(x)),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    throw new Exception(string.Join(" \n", result.Diagnostics
                        .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                        .Select(x => x.GetMessage())));
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                    return assembly.GetType("Eval.Code").GetMember("Run").First() as MethodInfo;
                }
            }
        }
        
        static string _assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

        static IEnumerable<string> _references = _commonRefs.Select(x => x.Location)
            .Concat(new[] {
                Path.Combine(_assemblyPath, "System.Runtime.dll"),
            }).Distinct();
    }
}
#endif

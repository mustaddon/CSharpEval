#if NET45
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Linq;

namespace RandomSolutions
{
    public partial class CSharpEval
    {
        static IEnumerable<string> _assemblyNames = _commonAssemblies.Select(x => x.ManifestModule.Name);

        static MethodInfo _compile(string code, IEnumerable<Assembly> assemblies)
        {
            var assemblyNames = _assemblyNames
                .Concat(assemblies.Select(x => x.ManifestModule.Name))
                .Distinct()
                .ToArray();

            var codeProvider = new CSharpCodeProvider();
            var compilerParameters = new CompilerParameters(assemblyNames)
            {
                CompilerOptions = "/t:library",
                GenerateInMemory = true,
            };

            var compilerResults = codeProvider.CompileAssemblyFromSource(compilerParameters, code.ToString());

            if (compilerResults.Errors.Count > 0)
                throw new Exception(string.Join(" \n", compilerResults.Errors.Cast<CompilerError>().Select(x => x.ErrorText)));

            var type = compilerResults.CompiledAssembly.GetType("Eval.Code");
            return type.GetMethod("Run");
        }
    }
}
#endif

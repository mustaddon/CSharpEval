using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace RandomSolutions
{
    public partial class CSharpEval
    {
        public static uint CacheLimit = 10 << 10;

        public static void ClearCache()
        {
            lock (_compiled) _compiled.Clear();
        }

        public static T Execute<T>(string code, Dictionary<string, object> vals = null, IEnumerable<Assembly> assemblies = null)
        {
            if (code.IndexOf(_return) < 0)
                code = string.Concat(_return, code);

            return (T)_execute(code, vals, assemblies);
        }

        public static void Execute(string code, Dictionary<string, object> vals = null, IEnumerable<Assembly> assemblies = null)
        {
            _execute(code + "; return null", vals, assemblies);
        }
        
        static object _execute(string code, Dictionary<string, object> vals, IEnumerable<Assembly> assemblies)
        {
            var codeToCompile = _getCompileCode(code, vals);
            var hash = _getHash(codeToCompile);
            
            MethodInfo compiled = null;

            if (_compiled.ContainsKey(hash))
                compiled = _compiled[hash];
            else
                lock (_compiled)
                    if (!_compiled.ContainsKey(hash))
                    {
                        compiled = _compile(codeToCompile, assemblies ?? new[] { Assembly.GetEntryAssembly(), Assembly.GetCallingAssembly() });

                        if (_compiled.Count > CacheLimit)
                            _compiled.Clear();

                        _compiled.Add(hash, compiled);
                    }

            return compiled.Invoke(null, vals?.Values.ToArray());
        }

        static string _getCompileCode(string code, Dictionary<string, object> vals)
        {
            var codeBuilder = new StringBuilder(@"
            using System;
            using System.Text;
            using System.Linq;
            using System.Collections.Generic;

            namespace Eval
            {
                public class Code
                {
                    public static object Run(");
            codeBuilder.Append(string.Join(", ", vals?.Select(x => $"{_getType(x.Value?.GetType())} {x.Key}") ?? new string[0]));
            codeBuilder.Append(@") {
                        ");
            codeBuilder.Append(code);
            codeBuilder.AppendLine(@";
                    }
                }
            }");

            return codeBuilder.ToString();
        }

        static readonly IEnumerable<Assembly> _commonAssemblies = new[] {
            typeof(Object).Assembly,
            typeof(Console).Assembly,
            typeof(Enumerable).Assembly,
            typeof(IEnumerable).Assembly,
            typeof(IEnumerable<>).Assembly,
            Assembly.GetExecutingAssembly(),
        };

        static string _getType(Type type)
        {
            if (type == null)
                type = typeof(object);

            if (type.IsGenericType == true)
            {
                var gTypes = type.GenericTypeArguments.Select(x => _getType(x));
                return $"{type.FullName.Substring(0, type.FullName.IndexOf('`'))}<{string.Join(",", gTypes)}>";
            }

            return type.FullName;
        }

        static string _getHash(string data)
        {
            var sha = new SHA256Managed();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
            var base64 = Convert.ToBase64String(hash);
            return base64;
        }

        static readonly Dictionary<string, MethodInfo> _compiled = new Dictionary<string, MethodInfo>();
        
        const string _return = "return ";
    }
}

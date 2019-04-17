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

        public static T Execute<T>(string code, object args, IEnumerable<Assembly> refs = null)
            => Execute<T>(code, _getArgs(args), refs);

        public static void Execute(string code, object args, IEnumerable<Assembly> refs = null)
            => Execute(code, _getArgs(args), refs);

        public static T Execute<T>(string code, Dictionary<string, object> args, IEnumerable<Assembly> refs = null)
            => Execute<T>(code, _getArgs(args), refs);

        public static void Execute(string code, Dictionary<string, object> args, IEnumerable<Assembly> refs = null)
            => Execute(code, _getArgs(args), refs);

        public static T Execute<T>(string code, Dictionary<string, Tuple<Type, object>> args = null, IEnumerable<Assembly> refs = null)
        {
            if (code.IndexOf(_return) < 0)
                code = string.Concat(_return, code);

            return (T)_execute(code, args, refs);
        }

        public static void Execute(string code, Dictionary<string, Tuple<Type, object>> args = null, IEnumerable<Assembly> refs = null)
        {
            _execute(string.Concat(code, "; return null"), args, refs);
        }

        public static uint CacheLimit = 1 << 12;

        public static void ClearCache()
        {
            lock (_compiled) _compiled.Clear();
        }



        static object _execute(string code, Dictionary<string, Tuple<Type, object>> args, IEnumerable<Assembly> refs)
        {
            var codeToCompile = _getCompileCode(code, args);

            MethodInfo compiled = null;

            if (CacheLimit == 0)
            {
                compiled = _compile(codeToCompile, refs);
            }
            else
            {
                var hash = _getHash(codeToCompile);

                if (_compiled.ContainsKey(hash))
                    compiled = _compiled[hash];
                else
                    lock (_compiled)
                        if (!_compiled.ContainsKey(hash))
                        {
                            compiled = _compile(codeToCompile, refs);

                            if (_compiled.Count > CacheLimit)
                                _compiled.Clear();

                            _compiled.Add(hash, compiled);
                        }
            }

            return compiled.Invoke(null, args?.Values.Select(x => x.Item2).ToArray());
        }

        static string _getCompileCode(string code, Dictionary<string, Tuple<Type, object>> args)
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
            codeBuilder.Append(string.Join(", ", args?.Select(x => $"{_getType(x.Value.Item1)} {x.Key}") ?? new string[0]));
            codeBuilder.Append(@") {
                        ");
            codeBuilder.Append(code);
            codeBuilder.AppendLine(@";
                    }
                }
            }");

            return codeBuilder.ToString();
        }

        static readonly IEnumerable<Assembly> _commonRefs = new[] {
            typeof(Object).Assembly,
            typeof(Console).Assembly,
            typeof(Enumerable).Assembly,
            typeof(IEnumerable).Assembly,
            typeof(IEnumerable<>).Assembly,
            Assembly.GetExecutingAssembly(),
        };

        static string _getType(Type type)
        {
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

        static Dictionary<string, Tuple<Type, object>> _getArgs(object args)
        {
            return args.GetType().GetProperties().Where(x => x.CanRead)
                .ToDictionary(x => x.Name, x => new Tuple<Type, object>(x.PropertyType, x.GetValue(args)));
        }

        static Dictionary<string, Tuple<Type, object>> _getArgs(Dictionary<string, object> args)
        {
            return args?.ToDictionary(x => x.Key, x => new Tuple<Type, object>(x.Value?.GetType() ?? typeof(object), x.Value));
        }

        static readonly Dictionary<string, MethodInfo> _compiled = new Dictionary<string, MethodInfo>();

        const string _return = "return ";
    }
}

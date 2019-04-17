using RandomSolutions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = CSharpEval.Execute<int>(@"
	            var list = new List<int>() { 1, 2, 3, 4, 5 };
	            var filter = list.Where(x => x < k);
	            return filter.Sum(x => x);", new { k = 4 });
        }
    }
}

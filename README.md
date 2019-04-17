# CSharpEval [![NuGet version](https://badge.fury.io/nu/RandomSolutions.CSharpEval.svg)](http://badge.fury.io/nu/RandomSolutions.CSharpEval)
Compile and execute C# code at program runtime

## Examples

*Simple math expression*
```csharp
var result = CSharpEval.Execute<int>("A + B", new { A = 2, B = 3});
```

*Linq sample*
```csharp
var result = CSharpEval.Execute<int>(@"
    var list = new List<int>() { 1, 2, 3, 4, 5 };
    return list.Where(x => x < K).Sum(x => x);
", new { K = 5 });
```

*Adding references*
```csharp
CSharpEval.Execute("Console.WriteLine(A)", 
    new { A = ExternalLib.SomeEnum.Sample }, 
    new[] { Assembly.GetAssembly(typeof(ExternalLib.SomeEnum)) });
```

using Dant.AspNetDependencyValidator.Analyzer;

namespace Dant.AspNetDependencyValidator.AnalyzerTests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var t = new ValidationTask();
            t.AssemblyPath = @"...";
            var result = t.Execute();
        }
    }
}
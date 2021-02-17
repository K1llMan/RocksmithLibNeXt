using System;

using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace RocksmithLibNeXt.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            Summary summary = BenchmarkRunner.Run<Benchmark>();
            Console.ReadLine();
        }
    }
}

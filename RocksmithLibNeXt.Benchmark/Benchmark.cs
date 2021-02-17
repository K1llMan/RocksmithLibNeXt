using System.IO;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

using RocksmithLibNeXt.GenericUseCases;

namespace RocksmithLibNeXt.Benchmark
{
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, invocationCount: 1)]
    public class Benchmark
    {
        private UseCasesConfig config;

        public Benchmark()
        {
            config = new();
        }

        [Benchmark]
        public void PsarcOpen()
        {
            UseCases.PsarcOpen(config.InputFileName);
        }

        [Benchmark]
        public void PsarcExtract()
        {
            UseCases.PsarcExtract(config.InputFileName, config.TempDir);
        }

        [Benchmark]
        public void PsarcSave()
        {
            UseCases.PsarcSave(config.TempDir, Path.GetRandomFileName());
        }
    }
}

using RocksmithLibNeXt.GenericUseCases;

using Xunit;
using Xunit.Abstractions;

// Optional
[assembly: CollectionBehavior(DisableTestParallelization = true)]
// Optional
[assembly: TestCaseOrderer("Xunit.Extensions.Ordering.TestCaseOrderer", "Xunit.Extensions.Ordering")]
// Optional
[assembly: TestCollectionOrderer("Xunit.Extensions.Ordering.CollectionOrderer", "Xunit.Extensions.Ordering")]

[assembly: TestFramework("Xunit.Extensions.Ordering.TestFramework", "Xunit.Extensions.Ordering")]

namespace RocksmithLibNeXt.Tests
{
    [CollectionDefinition("RocksmithLibNeXt Test Harness")]
    public class RocksmithLibNeXtTestCollection : ICollectionFixture<UseCasesConfig>
    {
    }

    public class RocksmithLibNeXtTest
    {
        public RocksmithLibNeXtTest(UseCasesConfig fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            Output = output;
        }

        public UseCasesConfig Fixture { get; set; }

        public ITestOutputHelper Output { get; set; }
    }
}
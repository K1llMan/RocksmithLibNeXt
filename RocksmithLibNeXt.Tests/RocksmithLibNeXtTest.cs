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
    public class YandexTestCollection : ICollectionFixture<RocksmithLibNeXtTestHarness>
    {
    }

    public class RocksmithLibNeXtTest
    {
        public RocksmithLibNeXtTest(RocksmithLibNeXtTestHarness fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            Output = output;
        }

        public RocksmithLibNeXtTestHarness Fixture { get; set; }

        public ITestOutputHelper Output { get; set; }
    }
}
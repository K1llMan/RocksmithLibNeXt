using System.IO;
using System.Linq;

using FluentAssertions;

using RocksmithLibNeXt.Formats.Psarc;

using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace RocksmithLibNeXt.Tests.Formats
{
    [Collection("RocksmithLibNeXt Test Harness")]
    public class PsarcTest : RocksmithLibNeXtTest
    {
        [Fact]
        [Order(0)]
        public void Open_ValidData_True()
        {
            Psarc psarc = new();

            psarc.Open(Fixture.InputFileName);

            psarc.TableOfContent.Count.Should().Be(23);
        }

        [Fact]
        [Order(1)]
        public void Extract_ValidData_True()
        {
            Psarc psarc = new();
            psarc.Open(Fixture.InputFileName);

            psarc.Extract(Fixture.TempDir);

            Directory.Exists(Fixture.TempDir).Should().BeTrue();
            Directory.GetFiles(Fixture.TempDir, "*").Length.Should().Equals(23);
        }

        [Fact]
        [Order(2)]
        public void Save_ValidData_True()
        {
            Psarc psarc = new();

            Directory.Exists(Fixture.TempDir).Should().BeTrue();
            Directory.GetFiles(Fixture.TempDir, "*", SearchOption.AllDirectories).ToList().ForEach(f => {
                string relPath = Path.GetRelativePath(Fixture.TempDir, f);
                psarc.AddEntry(relPath, f);
            });

            psarc.Save(Fixture.OutputFileName);

            FileInfo file = new(Fixture.OutputFileName);

            file.Exists.Should().BeTrue();
            file.Length.Should().BeGreaterOrEqualTo(new FileInfo(Fixture.InputFileName).Length);
        }

        public PsarcTest(RocksmithLibNeXtTestHarness fixture, ITestOutputHelper output) : base(fixture, output)
        {
            if (File.Exists(Fixture.OutputFileName))
                File.Delete(Fixture.OutputFileName);
        }
    }
}
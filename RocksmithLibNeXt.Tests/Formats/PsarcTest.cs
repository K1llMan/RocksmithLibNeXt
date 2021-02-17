using System.IO;

using FluentAssertions;

using RocksmithLibNeXt.Formats.Psarc;
using RocksmithLibNeXt.GenericUseCases;

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
            Psarc psarc = UseCases.PsarcOpen(Fixture.InputFileName);

            psarc.TableOfContent.Count.Should().Be(23);
        }

        [Fact]
        [Order(1)]
        public void Extract_ValidData_True()
        {
            UseCases.PsarcExtract(Fixture.InputFileName, Fixture.TempDir);

            Directory.Exists(Fixture.TempDir).Should().BeTrue();
            Directory.GetFiles(Fixture.TempDir, "*").Length.Should().Equals(23);
        }

        [Fact]
        [Order(2)]
        public void Save_ValidData_True()
        {
            UseCases.PsarcSave(Fixture.TempDir, Fixture.OutputFileName);

            FileInfo file = new(Fixture.OutputFileName);

            file.Exists.Should().BeTrue();
            file.Length.Should().BeGreaterOrEqualTo(new FileInfo(Fixture.InputFileName).Length);
        }

        public PsarcTest(UseCasesConfig fixture, ITestOutputHelper output) : base(fixture, output)
        {
            if (File.Exists(Fixture.OutputFileName))
                File.Delete(Fixture.OutputFileName);
        }
    }
}
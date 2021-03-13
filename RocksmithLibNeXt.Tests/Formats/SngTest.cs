using System.IO;

using FluentAssertions;

using RocksmithLibNeXt.Formats.Sng.Models;
using RocksmithLibNeXt.GenericUseCases;

using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace RocksmithLibNeXt.Tests.Formats
{
    [Collection("RocksmithLibNeXt Test Harness")]
    public class SngTest : RocksmithLibNeXtTest
    {
        [Fact]
        [Order(0)]
        public void SngStreamRead_ValidData_True()
        {
            FileStream fs = new(Fixture.InputSngStream, FileMode.Open);
            BinaryReader reader = new(fs);
            SngData sng = SngData.Read(reader);
            fs.Close();

            sng.Should().NotBeNull();
        }

        [Fact]
        [Order(1)]
        public void SngStreamWrite_ValidData_True()
        {
            FileStream fs = new(Fixture.InputSngStream, FileMode.Open);
            BinaryReader reader = new(fs);
            SngData sng = SngData.Read(reader);
            

            FileStream ofs = new(Fixture.OutputSngStream, FileMode.Create);
            BinaryWriter writer = new(ofs);
            sng.Write(writer);

            Fixture.CompareStreams(fs, ofs).Should().BeTrue();
        }

        /*
        [Fact]
        [Order(2)]
        public void OpenValidData_True()
        {
            Sng sng = new();
            sng.Open(Fixture.InputSng);

            sng.Should().NotBeNull();
        }
        */

        public SngTest(UseCasesConfig fixture, ITestOutputHelper output) : base(fixture, output)
        {
            if (File.Exists(Fixture.OutputPsarc))
                File.Delete(Fixture.OutputPsarc);
        }
    }
}
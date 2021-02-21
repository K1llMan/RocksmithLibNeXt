using System.IO;

using FluentAssertions;

using RocksmithLibNeXt.Formats.Sng;
using RocksmithLibNeXt.Formats.Sng.Common;
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
        #region Test class

        private class TestCollection : SngCollection<TestCollection, Bpm>
        {

        }

        #endregion Test class

        [Fact]
        [Order(0)]
        public void CollectionInterface_ValidData_True()
        {
            using FileStream fs = new(Fixture.InputSng, FileMode.Open);
            BinaryReader reader = new(fs);

            TestCollection collection = TestCollection.Read(reader);

            collection.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Order(1)]
        public void OpenStream_ValidData_True()
        {
            FileStream fs = new(Fixture.InputSngStream, FileMode.Open);
            BinaryReader reader = new(fs);

            SngData sng = SngData.Read(reader);
            sng.Should().NotBeNull();
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
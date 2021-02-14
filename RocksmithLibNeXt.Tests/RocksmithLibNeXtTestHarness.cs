﻿using System;
using System.IO;
using System.Reflection;

using RocksmithLibNeXt.Formats.Psarc;

namespace RocksmithLibNeXt.Tests
{
    public class RocksmithLibNeXtTestHarness : IDisposable
    {
        #region Properties

        public string LocalDir { get; set; }

        public string InputFileName { get; set; }

        public string OutputFileName{ get; set; }

        public string TempDir { get; set; }

        public Psarc Psarc { get; set; }

        #endregion Properties

        #region Auxiliary functions

        private void InitProperties()
        {
            LocalDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestData");

            InputFileName = Path.Combine(LocalDir, "test.psarc");
            OutputFileName = Path.Combine(LocalDir, "testOut.psarc");
            TempDir = Path.Combine(LocalDir, "temp");
        }

        #endregion Auxiliary functions

        #region Main functions

        public RocksmithLibNeXtTestHarness()
        {
            InitProperties();
        }

        public void Dispose()
        {
        }

        #endregion Main functions
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using RocksmithLibNeXt.Formats.Psarc.Models;


// future use code incubator 

namespace RocksmithLibNeXt.Formats.Psarc
{
    public static class PsarcExtensions
    {
        public static string TuningToName(string tolkenTuning, List<TuningDefinition> tuningXml = null)
        {
            var jObj = JObject.Parse(tolkenTuning);
            TuningStrings songTuning = jObj.ToObject<TuningStrings>();

            return TuningToName(songTuning, tuningXml);
        }

        public static string TuningToName(TuningStrings songTuning, List<TuningDefinition> tuningXml = null)
        {
            // 2X speed hack ... use preloaded TuningDefinitionRepository
            if (tuningXml == null || tuningXml.Count == 0)
                tuningXml = TuningDefinitionRepository.Instance.LoadTuningDefinitions(GameVersion.RS2014);

            foreach (var tuning in tuningXml)
                if (tuning.Tuning.String0 == songTuning.String0 &&
                    tuning.Tuning.String1 == songTuning.String1 &&
                    tuning.Tuning.String2 == songTuning.String2 &&
                    tuning.Tuning.String3 == songTuning.String3 &&
                    tuning.Tuning.String4 == songTuning.String4 &&
                    tuning.Tuning.String5 == songTuning.String5)
                    return tuning.UIName;

            return "Other";
        }

        public static string TuningStringToName(string strings, List<TuningDefinition> tuningXml = null)
        {
            // 2X speed hack ... use preloaded TuningDefinitionRepository
            if (tuningXml == null)
                tuningXml = TuningDefinitionRepository.Instance.LoadTuningDefinitions(GameVersion.RS2014);

            foreach (var tuning in tuningXml)
                if ((string) ("" + tuning.Tuning.String0 + tuning.Tuning.String1 + tuning.Tuning.String2 + tuning.Tuning.String3 + tuning.Tuning.String4 + tuning.Tuning.String5) == strings)
                    return tuning.UIName;

            return "Other";
        }


        public static bool RemoveArchiveEntry(string psarcPath, string entryName)
        {
            if (!File.Exists(psarcPath))
                return false;

            using Psarc archive = new(true);
            using FileStream psarcStream = File.OpenRead(psarcPath);
            archive.Read(psarcStream);
            psarcStream.Dispose(); // CRITICAL

            PsarcEntry? tocEntry = archive.TableOfContent.FirstOrDefault(entry => entry.Name == entryName);

            if (tocEntry == null) {
                archive.Dispose(); // CRITICAL
                return true;
            }

            archive.TableOfContent.Remove(tocEntry);
            archive.TableOfContent.Insert(0, new PsarcEntry() { Name = "NamesBlock.bin" });

            using (FileStream fs = File.Create(psarcPath)) {
                archive.Write(fs, true);
            }

            archive.Dispose(); // CRITICAL
            return true;
        }

        public static bool InjectArchiveEntry(string psarcPath, string entryName, string sourcePath, bool useMemory = true)
        {
            if (!File.Exists(psarcPath))
                return false;

            using Psarc archive = new(useMemory);
            using FileStream psarcStream = File.OpenRead(psarcPath);
            try {
                archive.Read(psarcStream);
                psarcStream.Dispose(); // CRITICAL

                MemoryStream entryStream = new();

                using (FileStream sourceStream = File.OpenRead(sourcePath)) {
                    sourceStream.CopyTo(entryStream);
                }

                entryStream.Position = 0;
                PsarcEntry tocPsarcEntry = archive.TableOfContent.FirstOrDefault(x => x.Name == entryName);

                if (tocPsarcEntry != null) {
                    tocPsarcEntry.Data.Dispose(); // CRITICAL
                    tocPsarcEntry.Data = entryStream;
                }
                else {
                    archive.AddEntry(entryName, entryStream);

                    // evil genius ... ;) => forces archive update
                    archive.TableOfContent.Insert(0, new PsarcEntry() { Name = "NamesBlock.bin" });
                }
            }
            catch {
                archive.Dispose(); // CRITICAL
                return false;
            }

            using (FileStream fs = File.Create(psarcPath)) {
                archive.Write(fs, true);
            }

            archive.Dispose(); // CRITICAL
            return true;
        }

        public static bool InjectArchiveEntry(string psarcPath, string entryName, Stream sourceStream, bool useMemory = true)
        {
            if (!File.Exists(psarcPath))
                return false;

            using Psarc archive = new(useMemory);
            using FileStream psarcStream = File.OpenRead(psarcPath);
            try {
                archive.Read(psarcStream);
                psarcStream.Dispose(); // CRITICAL

                MemoryStream entryStream = new();
                sourceStream.Position = 0;
                sourceStream.CopyTo(entryStream);
                entryStream.Position = 0;
                PsarcEntry tocPsarcEntry = archive.TableOfContent.FirstOrDefault(x => x.Name == entryName);

                if (tocPsarcEntry != null) {
                    tocPsarcEntry.Data.Dispose(); // CRITICAL
                    tocPsarcEntry.Data = entryStream;
                }
                else {
                    archive.AddEntry(entryName, entryStream);

                    // evil genius ... ;) => forces archive update
                    archive.TableOfContent.Insert(0, new PsarcEntry() { Name = "NamesBlock.bin" });
                }
            }
            catch {
                archive.Dispose(); // CRITICAL
                return false;
            }

            using (FileStream fs = File.Create(psarcPath)) {
                archive.Write(fs, true);
            }

            archive.Dispose(); // CRITICAL
            return true;
        }

        public static string ExtractArchiveFile(string psarcPath, string entryNamePath, string outputDir)
        {
            if (!File.Exists(psarcPath))
                return "";

            using Psarc archive = new(true);
            using FileStream psarcStream = File.OpenRead(psarcPath);
            archive.Read(psarcStream, true);
            PsarcEntry? tocEntry = archive.TableOfContent.Where(entry => entry.Name.Contains(entryNamePath)).FirstOrDefault();

            if (tocEntry != null) {
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                archive.InflateEntry(tocEntry, Path.Combine(outputDir, Path.GetFileName(tocEntry.ToString())));

                return Path.Combine(outputDir, tocEntry.ToString());
            }

            return "";
        }

        public static Stream ExtractArchiveFile(string psarcPath, string entryNamePath)
        {
            if (!File.Exists(psarcPath))
                return null;

            using Psarc archive = new(true);
            using FileStream psarcStream = File.OpenRead(psarcPath);
            archive.Read(psarcStream, true);
            PsarcEntry? tocEntry = archive.TableOfContent.FirstOrDefault(x => x.Name.Equals(entryNamePath));

            if (tocEntry != null) {
                archive.InflateEntry(tocEntry);
                return tocEntry.Data;
            }

            return null;
        }

        public static bool ReplaceData(this Psarc p, Func<PsarcEntry, bool> dataEntry, Stream newData)
        {
            PsarcEntry? de = p.TableOfContent.Where(dataEntry).FirstOrDefault();
            if (de != null) {
                if (de.Data != null) {
                    de.Data.Dispose();
                    de.Data = null;
                }
                else {
                    p.InflateEntry(de);
                }

                de.Data = newData;
                return true;
            }

            return false;
        }

        public static Stream ExtractPSARCData(this Stream stream, Func<PsarcEntry, bool> dataEntry)
        {
            using Psarc p = new(true);
            p.Read(stream, true);

            PsarcEntry? de = p.TableOfContent.Where(dataEntry).FirstOrDefault();
            if (de != null) {
                MemoryStream ms = new();
                p.InflateEntry(de);
                if (de.Data == null)
                    return null;
                de.Data.Position = 0;
                de.Data.CopyTo(ms);
                ms.Position = 0;
                return ms;
            }

            return null;
        }

        public static Stream GetData(this Psarc p, Func<PsarcEntry, bool> dataEntry)
        {
            PsarcEntry? de = p.TableOfContent.Where(dataEntry).FirstOrDefault();
            if (de != null) {
                if (de.Data == null)
                    p.InflateEntry(de);

                return de.Data;
            }

            return null;
        }

        public static bool ReplaceData(this Psarc p, Dictionary<Func<PsarcEntry, bool>, Stream> newData)
        {
            bool result = true;
            foreach (KeyValuePair<Func<PsarcEntry, bool>, Stream> d in newData)
                if (!p.ReplaceData(d.Key, d.Value))
                    result = false;
            return result;
        }

        public static NoCloseStream ReplaceData(this Psarc p, Func<PsarcEntry, bool> dataEntry, string newData)
        {
            NoCloseStream s = new();
            using (StreamWriter sr = new(s)) {
                sr.Write(newData);
            }

            s.Position = 0;

            if (!ReplaceData(p, dataEntry, s)) {
                s.canClose = true;
                s.Dispose();
                return null;
            }

            return s;
        }
    }


    /// <summary>
    /// Useful when some other class(StreamReader/StreamWriter...) tries to close the stream before it's supposed to be closed.
    /// </summary>
    public class NoCloseStream : MemoryStream
    {
        public bool canClose;

        public void CloseEx()
        {
            canClose = true;
            Close();
        }

        public override void Close()
        {
            if (canClose)
                base.Close();
        }
    }

    public class NoCloseStreamList : List<NoCloseStream>, IDisposable
    {
        public NoCloseStream NewStream()
        {
            NoCloseStream l = new();
            Add(l);
            return l;
        }

        public void Dispose()
        {
            foreach (NoCloseStream l in this)
                l.CloseEx();
            Clear();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using RocksmithLibNeXt.Formats.Psarc.Models;

// PsarcLoader methods are used to load archives into memory
// More efficient and faster than unpacking to physical files
// RS2014 ONLY

namespace RocksmithLibNeXt.Formats.Psarc
{
    public sealed class PsarcLoader : IDisposable
    {
        private Psarc archive;
        private Stream fileStream;
        public string ErrMsg { get; set; }

        // Loads song archive file to memory.
        public PsarcLoader(string fileName, bool useMemory = true)
        {
            archive = new Psarc(useMemory);
            fileStream = File.OpenRead(fileName);
            archive.Read(fileStream);
        }

        public Stream ExtractEntryData(Func<PsarcEntry, bool> entryLINQ)
        {
            PsarcEntry? entry = archive.TableOfContent.Where(entryLINQ).FirstOrDefault();
            if (entry != null) {
                MemoryStream ms = new();
                archive.InflateEntry(entry);
                if (entry.Data == null)
                    return null;

                entry.Data.Position = 0;
                entry.Data.CopyTo(ms);
                entry.Dispose();
                ms.Position = 0;
                return ms;
            }

            return null;
        }

        public List<string> ExtractEntryNames()
        {
            Stopwatch sw = new();
            sw.Restart();
            List<string> entryNames = new();

            // this iterates through all entries in archive
            foreach (PsarcEntry entry in archive.TableOfContent) {
                // do something interesting with the entry
                entryNames.Add(entry.Name);
                // needed to free memory (prevent crashing)
                if (entry.Data != null)
                    entry.Data.Dispose();
            }

            return entryNames;
        }

        // produces a comprehensive list of archive entry errors
        public List<string> FullErrorCheck()
        {
            List<string> errorLog = new();

            // this iterates through all entries in archive looking for errors
            foreach (PsarcEntry entry in archive.TableOfContent) {
                archive.InflateEntry(entry);
                if (!string.IsNullOrEmpty(archive.ErrMsg.ToString()))
                    errorLog.Add(archive.ErrMsg.ToString());

                if (entry.Data == null) {
                    errorLog.Add("Null Entry Error: " + entry.Name);
                }
                else {
                    entry.Data.Position = 0;
                    MemoryStream ms = new();
                    using StreamReader reader = new(ms, new UTF8Encoding(), false, 65536);
                    try {
                        string canRead = reader.ReadToEnd();
                    }
                    catch (Exception ex) {
                        errorLog.Add("Error Reading Entry: " + entry.Name + Environment.NewLine + ex.Message);
                    }
                }
            }

            return errorLog;
        }


        // this method will work for Song Packs too!
        public IEnumerable<Manifest2014<Attributes2014>> ExtractJsonManifests()
        {
            Stopwatch sw = new();
            sw.Restart();

            // every song contains gamesxblock but may not contain showlights.xml
            List<PsarcEntry> xblockEntries = archive.TableOfContent.Where(x => x.Name.StartsWith("gamexblocks/nsongs") && x.Name.EndsWith(".xblock")).ToList();
            if (!xblockEntries.Any())
                throw new Exception("Could not find valid xblock file in archive.");

            var jsonData = new List<Manifest2014<Attributes2014>>();
            // this foreach loop addresses song packs otherwise it is only done one time
            foreach (PsarcEntry xblockEntry in xblockEntries) {
                // CAREFUL with use of Contains and Replace to avoid creating duplicates
                string strippedName = xblockEntry.Name.Replace(".xblock", "").Replace("gamexblocks/nsongs", "");
                if (strippedName.Contains("_fcp_dlc"))
                    strippedName = strippedName.Replace("fcp_dlc", "");

                List<PsarcEntry> jsonEntries = archive.TableOfContent.Where(x => x.Name.StartsWith("manifests/songs") &&
                                                                                  x.Name.EndsWith(".json") && x.Name.Contains(strippedName)).OrderBy(x => x.Name).ToList();

                // looping through song multiple times gathering each arrangement
                foreach (PsarcEntry jsonEntry in jsonEntries) {
                    var dataObj = new Manifest2014<Attributes2014>();

                    archive.InflateEntry(jsonEntry);
                    jsonEntry.Data.Position = 0;
                    MemoryStream ms = new();
                    using (StreamReader reader = new(ms, new UTF8Encoding(), false, 65536)) //4Kb is default alloc size for windows .. 64Kb is default Psarc alloc
                    {
                        jsonEntry.Data.Position = 0;
                        jsonEntry.Data.CopyTo(ms);
                        ms.Position = 0;
                        var jsonObj = JObject.Parse(reader.ReadToEnd());
                        dataObj = JsonConvert.DeserializeObject<Manifest2014<Attributes2014>>(jsonObj.ToString());
                    }

                    jsonData.Add(dataObj);
                }
            }

            sw.Stop();
            return jsonData;
        }

        public ManifestHeader2014<AttributesHeader2014> ExtractHsanManifest()
        {
            Stopwatch sw = new();
            sw.Restart();
            // every song and song pack contain only one hsan file
            PsarcEntry? hsanEntry = archive.TableOfContent.FirstOrDefault(x => x.Name.StartsWith("manifests/songs") && x.Name.EndsWith(".hsan"));

            if (hsanEntry == null)
                throw new Exception("Could not find valid hsan manifest in archive.");

            var hsanData = new ManifestHeader2014<AttributesHeader2014>(new Platform(GamePlatform.Pc, GameVersion.RS2014));
            archive.InflateEntry(hsanEntry);
            MemoryStream ms = new();
            using (StreamReader reader = new(ms, new UTF8Encoding(), false, 65536)) //4Kb is default alloc size for windows .. 64Kb is default Psarc alloc
            {
                hsanEntry.Data.Position = 0;
                hsanEntry.Data.CopyTo(ms);
                ms.Position = 0;
                var jsonObj = JObject.Parse(reader.ReadToEnd());
                hsanData = JsonConvert.DeserializeObject<ManifestHeader2014<AttributesHeader2014>>(jsonObj.ToString());
            }

            sw.Stop();
            return hsanData;
        }

        public ToolkitInfo ExtractToolkitInfo()
        {
            var tkInfo = new ToolkitInfo();
            PsarcEntry? toolkitVersionEntry = archive.TableOfContent.FirstOrDefault(x => x.Name.Equals("toolkit.version"));

            if (toolkitVersionEntry != null) {
                archive.InflateEntry(toolkitVersionEntry);
                tkInfo = GeneralExtension.GetToolkitInfo(new StreamReader(toolkitVersionEntry.Data));
            }
            else {
                // this helps prevent null exceptions
                tkInfo.ToolkitVersion = "Null";
                tkInfo.PackageAuthor = "Ubisoft";
                tkInfo.PackageVersion = "0";
                tkInfo.PackageComment = "Null";
                tkInfo.PackageRating = "5";
            }

            return tkInfo;
        }

        public string ExtractAppId()
        {
            string appId = string.Empty;

            PsarcEntry? appIdEntry = archive.TableOfContent.FirstOrDefault(x => x.Name.Equals("appid.appid"));
            if (appIdEntry != null) {
                archive.InflateEntry(appIdEntry);

                using StreamReader reader = new(appIdEntry.Data);
                appId = reader.ReadLine();
            }

            return appId;
        }

        // ===================== FOR FUTURE ====================

        public Bitmap ExtractAlbumArt(bool extractTaggerOrg)
        {
            Bitmap imageData = null;
            Stopwatch sw = new();
            sw.Restart();

            //Func<PsarcEntry, bool> entryLINQ;
            //if (extractTaggerOrg)
            //    entryLINQ = entry => entry.Name == "tagger.org";
            //else
            //    entryLINQ = x => x.Name.Contains("256.dds");

            //var albumArtEntry = archive.TableOfContent.FirstOrDefault(entryLINQ);
            //if (albumArtEntry == null && extractTaggerOrg)
            //    RSTKTools.GlobalExtension.ShowProgress("Could not find tagger.org entry in archive.");

            //if (albumArtEntry != null)
            //{
            //    archive.InflateEntry(albumArtEntry);
            //    var ms = new MemoryStream();
            //    using (var reader = new StreamReader(ms, new UTF8Encoding(), false, 65536)) //4Kb is default alloc size for windows .. 64Kb is default Psarc alloc
            //    {
            //        albumArtEntry.Data.Position = 0;
            //        albumArtEntry.Data.CopyTo(ms);
            //        ms.Position = 0;

            //        var b = ImageExtensions.DDStoBitmap(ms);
            //        if (b != null)
            //            imageData = b;
            //    }
            //}

            sw.Stop();
            return imageData;
        }


        /// <summary>
        /// Convert wem archive entries to ogg files
        /// </summary>
        /// <param name="wems"></param>
        /// <param name="audioOggPath"></param>
        /// <param name="previewOggPath"></param>
        /// <returns></returns>
        public bool ConvertWemEntries(List<PsarcEntry> wems, string audioOggPath, string previewOggPath = "")
        {
            // TODO: Debug this untested revised code before first use

            bool result = false;

            if (wems.Count > 1)
                wems.Sort((e1, e2) =>
                {
                    if (e1.Length < e2.Length)
                        return 1;
                    if (e1.Length > e2.Length)
                        return -1;
                    return 0;
                });

            if (wems.Count > 0) {
                PsarcEntry top = wems[0]; // wem audio with internal TableOfContent path
                string tempAudioPath = Path.Combine(Path.GetTempPath(), top.Name);
                top.Data.Position = 0;

                using FileStream fs = File.Create(tempAudioPath);
                top.Data.CopyTo(fs);
                try {
                    OggFile.Revorb(tempAudioPath, audioOggPath, Path.GetExtension(tempAudioPath).GetWwiseVersion());
                    result = true;
                }
                catch {
                    result = false;
                }
            }

            if (!string.IsNullOrEmpty(previewOggPath) && result && wems.Count > 0) {
                PsarcEntry bottom = wems.Last();
                string tempAudioPath = Path.Combine(Path.GetTempPath(), bottom.Name);
                bottom.Data.Position = 0;
                using FileStream fs = File.Create(tempAudioPath);
                bottom.Data.CopyTo(fs);
                try {
                    OggFile.Revorb(tempAudioPath, previewOggPath, Path.GetExtension(tempAudioPath).GetWwiseVersion());
                    result = true;
                }
                catch {
                    result = false;
                }
            }

            return result;
        }

        public void Dispose()
        {
            if (fileStream != null) {
                fileStream.Close();
                fileStream.Dispose();
                fileStream = null;
            }

            if (archive != null) {
                archive.Dispose();
                archive = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
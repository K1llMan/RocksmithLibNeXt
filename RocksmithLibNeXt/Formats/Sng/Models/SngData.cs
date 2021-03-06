using System.IO;

using RocksmithLibNeXt.Formats.Sng.Common;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class SngData
    {
        #region Properties

        public SngCollection<Action> Actions { get; set; }
        public SngCollection<Arrangement> Arrangements { get; set; }
        public SngCollection<Bpm> BPMs { get; set; }
        public SngCollection<ChordNotes> ChordNotes { get; set; }
        public SngCollection<Chord> Chords { get; set; }
        public SngCollection<Dna> DNAs { get; set; }
        public SngCollection<Event> Events { get; set; }
        public Metadata Meta { get; set; }
        public SngCollection<NLinkedDifficulty> NLD { get; set; }
        public SngCollection<PhraseExtraInfoByLevel> PhraseExtraInfo { get; set; }
        public SngCollection<PhraseIteration> PhraseIterations { get; set; }
        public SngCollection<Phrase> Phrases { get; set; }
        public SngCollection<Section> Sections { get; set; }
        public SngCollection<SymbolDefinition> SymbolsDefinition { get; set; }
        public SngCollection<SymbolsHeader> SymbolsHeader { get; set; }
        public SngCollection<SymbolsTexture> SymbolsTexture { get; set; }
        public SngCollection<Tone> Tones { get; set; }
        public SngCollection<Vocal> Vocals { get; set; }

        #endregion Properties

        #region Main functions

        public static SngData Read(BinaryReader r)
        {
            SngData sd = new() {
                BPMs = SngCollection<Bpm>.Read(r),
                Phrases = SngCollection<Phrase>.Read(r),
                Chords = SngCollection<Chord>.Read(r),
                ChordNotes = SngCollection<ChordNotes>.Read(r),
                Vocals = SngCollection<Vocal>.Read(r)
            };

            if (sd.Vocals.Count > 0)
            {
                sd.SymbolsHeader = SngCollection<SymbolsHeader>.Read(r);
                sd.SymbolsTexture = SngCollection<SymbolsTexture>.Read(r);
                sd.SymbolsDefinition = SngCollection<SymbolDefinition>.Read(r);
            }

            sd.PhraseIterations = SngCollection<PhraseIteration>.Read(r);
            sd.PhraseExtraInfo = SngCollection<PhraseExtraInfoByLevel>.Read(r);
            sd.NLD = SngCollection<NLinkedDifficulty>.Read(r);
            sd.Actions = SngCollection<Action>.Read(r);
            sd.Events = SngCollection<Event>.Read(r);
            sd.Tones = SngCollection<Tone>.Read(r);
            sd.DNAs = SngCollection<Dna>.Read(r);
            sd.Sections = SngCollection<Section>.Read(r);
            sd.Arrangements = SngCollection<Arrangement>.Read(r);
            sd.Meta = Metadata.Read(r);

            return sd;
        }

        public void Write(BinaryWriter w)
        {

        }

        #endregion Main functions
    }
}
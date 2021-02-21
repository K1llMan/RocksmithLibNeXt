using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class SngData
    {
        #region Properties

        public ActionCollection Actions { get; set; }
        public ArrangementCollection Arrangements { get; set; }
        public BpmCollection BPMs { get; set; }
        public ChordNotesCollection ChordNotes { get; set; }
        public ChordCollection Chords { get; set; }
        public DnaCollection DNAs { get; set; }
        public EventCollection Events { get; set; }
        public Metadata Meta { get; set; }
        public NLinkedDifficultyCollection NLD { get; set; }
        public PhraseExtraInfoByLevelCollection PhraseExtraInfo { get; set; }
        public PhraseIterationCollection PhraseIterations { get; set; }
        public PhraseCollection Phrases { get; set; }
        public SectionCollection Sections { get; set; }
        public SymbolDefinitionCollection SymbolsDefinition { get; set; }
        public SymbolsHeaderCollection SymbolsHeader { get; set; }
        public SymbolsTextureCollection SymbolsTexture { get; set; }
        public ToneCollection Tones { get; set; }
        public VocalCollection Vocals { get; set; }

        #endregion Properties

        #region Main functions

        public static SngData Read(BinaryReader r)
        {
            SngData sd = new() {
                BPMs = BpmCollection.Read(r),
                Phrases = PhraseCollection.Read(r),
                Chords = ChordCollection.Read(r),
                ChordNotes = ChordNotesCollection.Read(r),
                Vocals = VocalCollection.Read(r)
            };

            if (sd.Vocals.Count > 0)
            {
                sd.SymbolsHeader = SymbolsHeaderCollection.Read(r);
                sd.SymbolsTexture = SymbolsTextureCollection.Read(r);
                sd.SymbolsDefinition = SymbolDefinitionCollection.Read(r);
            }

            sd.PhraseIterations = PhraseIterationCollection.Read(r);
            sd.PhraseExtraInfo = PhraseExtraInfoByLevelCollection.Read(r);
            sd.NLD = NLinkedDifficultyCollection.Read(r);
            sd.Actions = ActionCollection.Read(r);
            sd.Events = EventCollection.Read(r);
            sd.Tones = ToneCollection.Read(r);
            sd.DNAs = DnaCollection.Read(r);
            sd.Sections = SectionCollection.Read(r);
            sd.Arrangements = ArrangementCollection.Read(r);
            sd.Meta = Metadata.Read(r);

            return sd;
        }

        #endregion Main functions
    }
}
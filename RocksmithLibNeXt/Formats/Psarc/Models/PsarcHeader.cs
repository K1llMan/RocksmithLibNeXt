namespace RocksmithLibNeXt.Formats.Psarc.Models
{
    public class PsarcHeader
    {
        public uint MagicNumber;
        public uint VersionNumber;
        public uint CompressionMethod;
        public uint TotalTOCSize;
        public uint TOCEntrySize;
        public uint NumFiles;
        public uint BlockSizeAlloc;
        public uint ArchiveFlags;

        public PsarcHeader()
        {
            MagicNumber = 1347633490; //'PSAR'
            VersionNumber = 65540; //1.4
            CompressionMethod = 2053925218; //'zlib' (also available 'lzma')
            TOCEntrySize = 30; //bytes
            //NumFiles = 0;
            BlockSizeAlloc = 65536; //Decompression buffer size = 64kb
            ArchiveFlags = 0; //It's bitfield actually, see Psarc.bt
        }
    }
}
namespace SabreTools.Compression.libmspack.CHM
{
    public static class Constants
    {
        public const ushort chmhead_Signature = 0x0000;
        public const ushort chmhead_Version = 0x0004;
        public const ushort chmhead_HeaderLen = 0x0008;
        public const ushort chmhead_Unknown1 = 0x000C;
        public const ushort chmhead_Timestamp = 0x0010;
        public const ushort chmhead_LanguageID = 0x0014;
        public const ushort chmhead_GUID1 = 0x0018;
        public const ushort chmhead_GUID2 = 0x0028;
        public const ushort chmhead_SIZEOF = 0x0038;

        public const ushort chmhst_OffsetHS0 = 0x0000;
        public const ushort chmhst_LengthHS0 = 0x0008;
        public const ushort chmhst_OffsetHS1 = 0x0010;
        public const ushort chmhst_LengthHS1 = 0x0018;
        public const ushort chmhst_SIZEOF = 0x0020;
        public const ushort chmhst3_OffsetCS0 = 0x0020;
        public const ushort chmhst3_SIZEOF = 0x0028;

        public const ushort chmhs0_Unknown1 = 0x0000;
        public const ushort chmhs0_Unknown2 = 0x0004;
        public const ushort chmhs0_FileLen = 0x0008;
        public const ushort chmhs0_Unknown3 = 0x0010;
        public const ushort chmhs0_Unknown4 = 0x0014;
        public const ushort chmhs0_SIZEOF = 0x0018;

        public const ushort chmhs1_Signature = 0x0000;
        public const ushort chmhs1_Version = 0x0004;
        public const ushort chmhs1_HeaderLen = 0x0008;
        public const ushort chmhs1_Unknown1 = 0x000C;
        public const ushort chmhs1_ChunkSize = 0x0010;
        public const ushort chmhs1_Density = 0x0014;
        public const ushort chmhs1_Depth = 0x0018;
        public const ushort chmhs1_IndexRoot = 0x001C;
        public const ushort chmhs1_FirstPMGL = 0x0020;
        public const ushort chmhs1_LastPMGL = 0x0024;
        public const ushort chmhs1_Unknown2 = 0x0028;
        public const ushort chmhs1_NumChunks = 0x002C;
        public const ushort chmhs1_LanguageID = 0x0030;
        public const ushort chmhs1_GUID = 0x0034;
        public const ushort chmhs1_Unknown3 = 0x0044;
        public const ushort chmhs1_Unknown4 = 0x0048;
        public const ushort chmhs1_Unknown5 = 0x004C;
        public const ushort chmhs1_Unknown6 = 0x0050;
        public const ushort chmhs1_SIZEOF = 0x0054;

        public const ushort pmgl_Signature = 0x0000;
        public const ushort pmgl_QuickRefSize = 0x0004;
        public const ushort pmgl_Unknown1 = 0x0008;
        public const ushort pmgl_PrevChunk = 0x000C;
        public const ushort pmgl_NextChunk = 0x0010;
        public const ushort pmgl_Entries = 0x0014;
        public const ushort pmgl_headerSIZEOF = 0x0014;

        public const ushort pmgi_Signature = 0x0000;
        public const ushort pmgi_QuickRefSize = 0x0004;
        public const ushort pmgi_Entries = 0x0008;
        public const ushort pmgi_headerSIZEOF = 0x000C;

        public const ushort lzxcd_Length = 0x0000;
        public const ushort lzxcd_Signature = 0x0004;
        public const ushort lzxcd_Version = 0x0008;
        public const ushort lzxcd_ResetInterval = 0x000C;
        public const ushort lzxcd_WindowSize = 0x0010;
        public const ushort lzxcd_CacheSize = 0x0014;
        public const ushort lzxcd_Unknown1 = 0x0018;
        public const ushort lzxcd_SIZEOF = 0x001C;

        public const ushort lzxrt_Unknown1 = 0x0000;
        public const ushort lzxrt_NumEntries = 0x0004;
        public const ushort lzxrt_EntrySize = 0x0008;
        public const ushort lzxrt_TableOffset = 0x000C;
        public const ushort lzxrt_UncompLen = 0x0010;
        public const ushort lzxrt_CompLen = 0x0018;
        public const ushort lzxrt_FrameLen = 0x0020;
        public const ushort lzxrt_Entries = 0x0028;
        public const ushort lzxrt_headerSIZEOF = 0x0028;
    }
}
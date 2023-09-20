namespace SabreTools.Compression.libmspack
{
    public static class oab
    {
        public const int oabhead_VersionHi = 0x0000;
        public const int oabhead_VersionLo = 0x0004;
        public const int oabhead_BlockMax = 0x0008;
        public const int oabhead_TargetSize = 0x000c;
        public const int oabhead_SIZEOF = 0x0010;

        public const int oabblk_Flags = 0x0000;
        public const int oabblk_CompSize = 0x0004;
        public const int oabblk_UncompSize = 0x0008;
        public const int oabblk_CRC = 0x000c;
        public const int oabblk_SIZEOF = 0x0010;

        public const int patchhead_VersionHi = 0x0000;
        public const int patchhead_VersionLo = 0x0004;
        public const int patchhead_BlockMax = 0x0008;
        public const int patchhead_SourceSize = 0x000c;
        public const int patchhead_TargetSize = 0x0010;
        public const int patchhead_SourceCRC = 0x0014;
        public const int patchhead_TargetCRC = 0x0018;
        public const int patchhead_SIZEOF = 0x001c;

        public const int patchblk_PatchSize = 0x0000;
        public const int patchblk_TargetSize = 0x0004;
        public const int patchblk_SourceSize = 0x0008;
        public const int patchblk_CRC = 0x000c;
        public const int patchblk_SIZEOF = 0x0010;
    }
}
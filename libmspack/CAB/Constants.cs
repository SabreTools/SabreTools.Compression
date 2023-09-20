namespace SabreTools.Compression.libmspack.CAB
{
    public static class Constants
    {
        /* structure offsets */
        public const byte cfhead_Signature = 0x00;
        public const byte cfhead_CabinetSize = 0x08;
        public const byte cfhead_FileOffset = 0x10;
        public const byte cfhead_MinorVersion = 0x18;
        public const byte cfhead_MajorVersion = 0x19;
        public const byte cfhead_NumFolders = 0x1A;
        public const byte cfhead_NumFiles = 0x1C;
        public const byte cfhead_Flags = 0x1E;
        public const byte cfhead_SetID = 0x20;
        public const byte cfhead_CabinetIndex = 0x22;
        public const byte cfhead_SIZEOF = 0x24;
        public const byte cfheadext_HeaderReserved = 0x00;
        public const byte cfheadext_FolderReserved = 0x02;
        public const byte cfheadext_DataReserved = 0x03;
        public const byte cfheadext_SIZEOF = 0x04;
        public const byte cffold_DataOffset = 0x00;
        public const byte cffold_NumBlocks = 0x04;
        public const byte cffold_CompType = 0x06;
        public const byte cffold_SIZEOF = 0x08;
        public const byte cffile_UncompressedSize = 0x00;
        public const byte cffile_FolderOffset = 0x04;
        public const byte cffile_FolderIndex = 0x08;
        public const byte cffile_Date = 0x0A;
        public const byte cffile_Time = 0x0C;
        public const byte cffile_Attribs = 0x0E;
        public const byte cffile_SIZEOF = 0x10;
        public const byte cfdata_CheckSum = 0x00;
        public const byte cfdata_CompressedSize = 0x04;
        public const byte cfdata_UncompressedSize = 0x06;
        public const byte cfdata_SIZEOF = 0x08;

        /* flags */
        public const ushort cffoldCOMPTYPE_MASK = 0x000f;
        public const ushort cffileCONTINUED_FROM_PREV = 0xFFFD;
        public const ushort cffileCONTINUED_TO_NEXT = 0xFFFE;
        public const ushort cffileCONTINUED_PREV_AND_NEXT = 0xFFFF;

        /* CAB data blocks are <= 32768 bytes in uncompressed form. Uncompressed
         * blocks have zero growth. MSZIP guarantees that it won't grow above
         * uncompressed size by more than 12 bytes. LZX guarantees it won't grow
         * more than 6144 bytes. Quantum has no documentation, but the largest
         * block seen in the wild is 337 bytes above uncompressed size.
         */
        public const int CAB_BLOCKMAX = 32768;
        public const int CAB_INPUTMAX = CAB_BLOCKMAX + 6144;

        /* input buffer needs to be CAB_INPUTMAX + 1 byte to allow for max-sized block
         * plus 1 trailer byte added by cabd_sys_read_block() for Quantum alignment.
         *
         * When MSCABD_PARAM_SALVAGE is set, block size is not checked so can be
         * up to 65535 bytes, so max input buffer size needed is 65535 + 1
         */
        public const int CAB_INPUTMAX_SALVAGE = 65535;
        public const int CAB_INPUTBUF = CAB_INPUTMAX_SALVAGE + 1;

        /* There are no more than 65535 data blocks per folder, so a folder cannot
         * be more than 32768*65535 bytes in length. As files cannot span more than
         * one folder, this is also their max offset, length and offset+length limit.
         */
        public const int CAB_FOLDERMAX = 65535;
        public const int CAB_LENGTHMAX = CAB_BLOCKMAX * CAB_FOLDERMAX;
    }
}
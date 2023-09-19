namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// A structure which represents a single file in a cabinet or cabinet set.
    /// 
    /// All fields are READ ONLY.
    /// </summary>
    public class mscabd_file
    {
        /// <summary>
        /// The next file in the cabinet or cabinet set, or NULL if this is the
        /// final file.
        /// </summary>
        public mscabd_file next { get; set; }

        /// <summary>
        /// The filename of the file.
        /// 
        /// A null terminated string of up to 255 bytes in length, it may be in
        /// either ISO-8859-1 or UTF8 format, depending on the file attributes.
        /// </summary>
        public string filename { get; set; }

        /// <summary>
        /// The uncompressed length of the file, in bytes.
        /// </summary>
        public uint length { get; set; }

        /// <summary>
        /// File attributes.
        /// </summary>
        public MSCAB_ATTRIB attribs { get; set; }

        /// <summary>
        /// File's last modified time, hour field.
        /// </summary>
        public char time_h { get; set; }

        /// <summary>
        /// File's last modified time, minute field.
        /// </summary>
        public char time_m { get; set; }

        /// <summary>
        /// File's last modified time, second field.
        /// </summary>
        public char time_s { get; set; }

        /// <summary>
        /// File's last modified date, day field.
        /// </summary>
        public char date_d { get; set; }

        /// <summary>
        /// File's last modified date, month field.
        /// </summary>
        public char date_m { get; set; }

        /// <summary>
        /// File's last modified date, year field.
        /// </summary>
        public int date_y;

        /// <summary>
        /// A pointer to the folder that contains this file.
        /// </summary>
        public mscabd_folder folder { get; set; }

        /// <summary>
        /// The uncompressed offset of this file in its folder.
        /// </summary>
        public uint offset { get; set; }
    }
}
namespace SabreTools.Compression.libmspack
{
    /// <summary>
    /// There is one of these for every cabinet a folder spans
    /// </summary>
    public class mscabd_folder_data
    {
        public mscabd_folder_data next { get; set; }

        /// <summary>
        /// Cabinet file of this folder span
        /// </summary>
        public mscabd_cabinet cab { get; set; }

        /// <summary>
        /// Cabinet offset of first datablock
        /// </summary>
        public long offset { get; set; }
    }
}
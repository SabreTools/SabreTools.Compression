using System;

namespace SabreTools.Compression.libmspack
{
    public partial class mspack
    {
        #region HLP

        /// <summary>
        /// Creates a new HLP compressor.
        /// </summary>
        /// <returns>A <see cref="HLP.Compressor"/> or null</returns>
        public static HLP.Compressor CreateHLPCompressor() => null;

        /// <summary>
        /// Creates a new HLP decompressor.
        /// </summary>
        /// <returns>A <see cref="HLP.Decompressor"/> or null</returns>
        public static HLP.Decompressor CreateHLPDecompressor() => null;

        /// <summary>
        /// Destroys an existing hlp compressor.
        /// </summary>
        /// <param name="self">The <see cref="HLP.Compressor"/> to destroy</param>
        public static void DestroyHLPCompressor(HLP.Compressor self) { }

        /// <summary>
        /// Destroys an existing hlp decompressor.
        /// </summary>
        /// <param name="self">The <see cref="HLP.Decompressor"/> to destroy</param>
        public static void DestroyHLPDecompressor(HLP.Decompressor self) { }

        #endregion

        #region SZDD

        /// <summary>
        /// Creates a new SZDD compressor.
        /// </summary>
        /// <param name="sys">A custom <see cref="mspack_system"/> structure, or null to use the default</param>
        /// <returns>A <see cref="msszdd_compressor"/> or null</returns>
        public static msszdd_compressor mspack_create_szdd_compressor(mspack_system sys) => null;

        /// <summary>
        /// Creates a new SZDD decompressor.
        /// </summary>
        /// <returns>A <see cref="msszdd_decompressor"/> or null</returns>
        public static msszdd_decompressor mspack_create_szdd_decompressor()
        {
            msszdd_decompressor self = new msszdd_decompressor();
            self.system = new mspack_default_system();
            self.error = MSPACK_ERR.MSPACK_ERR_OK;

            return self;
        }

        /// <summary>
        /// Destroys an existing SZDD compressor.
        /// </summary>
        /// <param name="self">The <see cref="msszdd_compressor"/> to destroy</param>
        public static void mspack_destroy_szdd_compressor(msszdd_compressor self) { }

        /// <summary>
        /// Destroys an existing SZDD decompressor.
        /// </summary>
        /// <param name="self">The <see cref="msszdd_decompressor"/> to destroy</param>
        public static void mspack_destroy_szdd_decompressor(msszdd_decompressor self)
        {
            if (self != null)
            {
                mspack_system sys = self.system;
                //sys.free(self);
            }
        }

        #endregion

        /// <summary>
        /// Creates a new KWAJ compressor.
        /// </summary>
        /// <param name="sys">A custom <see cref="mspack_system"/> structure, or null to use the default</param>
        /// <returns>A <see cref="KWAJ.Compressor"/> or null</returns>
        public static KWAJ.Compressor CreateKWAJCompressor(mspack_system sys) => null;

        /// <summary>
        /// Creates a new KWAJ decompressor.
        /// </summary>
        /// <param name="sys">A custom <see cref="mspack_system"/> structure, or null to use the default</param>
        /// <returns>A <see cref="mskwaj_decompressor"/> or null</returns>
        public static mskwaj_decompressor mspack_create_kwaj_decompressor(mspack_system sys) => throw new NotImplementedException();

        /// <summary>
        /// Destroys an existing KWAJ compressor.
        /// </summary>
        /// <param name="self">The <see cref="KWAJ.Compressor"/> to destroy</param>
        public static void DestroyKWAJCompressor(KWAJ.Compressor self) { }

        /// <summary>
        /// Destroys an existing KWAJ decompressor.
        /// </summary>
        /// <param name="self">The <see cref="mskwaj_decompressor"/> to destroy</param>
        public static void mspack_destroy_kwaj_decompressor(mskwaj_decompressor self) => throw new NotImplementedException();

        /// <summary>
        /// Creates a new OAB compressor.
        /// </summary>
        /// <param name="sys">A custom <see cref="mspack_system"/> structure, or null to use the default</param>
        /// <returns>A <see cref="msoab_compressor"/> or null</returns>
        public static msoab_compressor mspack_create_oab_compressor(mspack_system sys) => throw new NotImplementedException();

        /// <summary>
        /// Creates a new OAB decompressor.
        /// </summary>
        /// <param name="sys">A custom <see cref="mspack_system"/> structure, or null to use the default</param>
        /// <returns>A <see cref="msoab_decompressor"/> or null</returns>
        public static msoab_decompressor mspack_create_oab_decompressor(mspack_system sys)
        {
            if (sys == null) sys = new mspack_oab_system();

            msoab_decompressor self = new msoab_decompressor();
            self.system = sys;
            self.buf_size = 4096;
            return self;
        }

        /// <summary>
        /// Destroys an existing OAB compressor.
        /// </summary>
        /// <param name="self">The <see cref="msoab_compressor"/> to destroy</param>
        public static void mspack_destroy_oab_compressor(msoab_compressor self)
        {
            if (self != null)
            {
                mspack_system sys = self.system;
                //sys.free(self);
            }
        }

        /// <summary>
        /// Destroys an existing OAB decompressor.
        /// </summary>
        /// <param name="self">The <see cref="msoab_decompressor"/> to destroy</param>
        public static void mspack_destroy_oab_decompressor(msoab_decompressor self) => throw new NotImplementedException();
    }
}
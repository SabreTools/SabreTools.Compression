namespace SabreTools.Compression.libmspack
{
    public abstract partial class mspack
    {
        /// <summary>
        /// Creates a new CAB compressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="mscab_compressor"/> or NULL</returns>
        public abstract mscab_compressor mspack_create_cab_compressor(mspack_system sys);

        /// <summary>
        /// Creates a new CAB decompressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="mscab_decompressor"/> or NULL</returns>
        public abstract mscab_decompressor mspack_create_cab_decompressor(mspack_system sys);

        /// <summary>
        /// Destroys an existing CAB compressor.
        /// </summary>
        /// <param name="self">The <see cref="mscab_compressor"/> to destroy</param>
        public abstract void mspack_destroy_cab_compressor(mscab_compressor self);

        /// <summary>
        /// Destroys an existing CAB decompressor.
        /// </summary>
        /// <param name="self">The <see cref="mscab_decompressor"/> to destroy</param>
        public abstract void mspack_destroy_cab_decompressor(mscab_decompressor self);

        /// <summary>
        /// Creates a new CHM compressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="mschm_compressor"/> or NULL</returns>
        public abstract mschm_compressor mspack_create_chm_compressor(mspack_system sys);

        /// <summary>
        /// Creates a new CHM decompressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="mschm_decompressor"/> or NULL</returns>
        public abstract mschm_decompressor mspack_create_chm_decompressor(mspack_system sys);

        /// <summary>
        /// Destroys an existing CHM compressor.
        /// </summary>
        /// <param name="self">The <see cref="mschm_compressor"/> to destroy</param>
        public abstract void mspack_destroy_chm_compressor(mschm_compressor self);

        /// <summary>
        /// Destroys an existing CHM decompressor.
        /// </summary>
        /// <param name="self">The <see cref="mschm_decompressor"/> to destroy</param>
        public abstract void mspack_destroy_chm_decompressor(mschm_decompressor self);

        /// <summary>
        /// Creates a new LIT compressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="mslit_compressor"/> or NULL</returns>
        public abstract mslit_compressor mspack_create_lit_compressor(mspack_system sys);

        /// <summary>
        /// Creates a new LIT decompressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="mslit_decompressor"/> or NULL</returns>
        public abstract mslit_decompressor mspack_create_lit_decompressor(mspack_system sys);

        /// <summary>
        /// Destroys an existing LIT compressor.
        /// </summary>
        /// <param name="self">The <see cref="mslit_compressor"/> to destroy</param>
        public abstract void mspack_destroy_lit_compressor(mslit_compressor self);

        /// <summary>
        /// Destroys an existing LIT decompressor.
        /// </summary>
        /// <param name="self">The <see cref="mslit_decompressor"/> to destroy</param>
        public abstract void mspack_destroy_lit_decompressor(mslit_decompressor self);

        /// <summary>
        /// Creates a new HLP compressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="mshlp_compressor"/> or NULL</returns>
        public abstract mshlp_compressor mspack_create_hlp_compressor(mspack_system sys);

        /// <summary>
        /// Creates a new HLP decompressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="mshlp_decompressor"/> or NULL</returns>
        public abstract mshlp_decompressor mspack_create_hlp_decompressor(mspack_system sys);

        /// <summary>
        /// Destroys an existing hlp compressor.
        /// </summary>
        /// <param name="self">The <see cref="mshlp_compressor"/> to destroy</param>
        public abstract void mspack_destroy_hlp_compressor(mshlp_compressor self);

        /// <summary>
        /// Destroys an existing hlp decompressor.
        /// </summary>
        /// <param name="self">The <see cref="mshlp_decompressor"/> to destroy</param>
        public abstract void mspack_destroy_hlp_decompressor(mshlp_decompressor self);

        /// <summary>
        /// Creates a new SZDD compressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="msszdd_compressor"/> or NULL</returns>
        public abstract msszdd_compressor mspack_create_szdd_compressor(mspack_system sys);

        /// <summary>
        /// Creates a new SZDD decompressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="msszdd_decompressor"/> or NULL</returns>
        public abstract msszdd_decompressor mspack_create_szdd_decompressor(mspack_system sys);

        /// <summary>
        /// Destroys an existing SZDD compressor.
        /// </summary>
        /// <param name="self">The <see cref="msszdd_compressor"/> to destroy</param>
        public abstract void mspack_destroy_szdd_compressor(msszdd_compressor self);

        /// <summary>
        /// Destroys an existing SZDD decompressor.
        /// </summary>
        /// <param name="self">The <see cref="msszdd_decompressor"/> to destroy</param>
        public abstract void mspack_destroy_szdd_decompressor(msszdd_decompressor self);

        /// <summary>
        /// Creates a new KWAJ compressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="mskwaj_compressor"/> or NULL</returns>
        public abstract mskwaj_compressor mspack_create_kwaj_compressor(mspack_system sys);

        /// <summary>
        /// Creates a new KWAJ decompressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="mskwaj_decompressor"/> or NULL</returns>
        public abstract mskwaj_decompressor mspack_create_kwaj_decompressor(mspack_system sys);

        /// <summary>
        /// Destroys an existing KWAJ compressor.
        /// </summary>
        /// <param name="self">The <see cref="mskwaj_compressor"/> to destroy</param>
        public abstract void mspack_destroy_kwaj_compressor(mskwaj_compressor self);

        /// <summary>
        /// Destroys an existing KWAJ decompressor.
        /// </summary>
        /// <param name="self">The <see cref="mskwaj_decompressor"/> to destroy</param>
        public abstract void mspack_destroy_kwaj_decompressor(mskwaj_decompressor self);

        /// <summary>
        /// Creates a new OAB compressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="msoab_compressor"/> or NULL</returns>
        public abstract msoab_compressor mspack_create_oab_compressor(mspack_system sys);

        /// <summary>
        /// Creates a new OAB decompressor.
        /// </summary>
        /// <param name="sys">A custom mspack_system structure, or NULL to use the default</param>
        /// <returns>A <see cref="msoab_decompressor"/> or NULL</returns>
        public abstract msoab_decompressor mspack_create_oab_decompressor(mspack_system sys);

        /// <summary>
        /// Destroys an existing OAB compressor.
        /// </summary>
        /// <param name="self">The <see cref="msoab_compressor"/> to destroy</param>
        public abstract void mspack_destroy_oab_compressor(msoab_compressor self);

        /// <summary>
        /// Destroys an existing OAB decompressor.
        /// </summary>
        /// <param name="self">The <see cref="msoab_decompressor"/> to destroy</param>
        public abstract void mspack_destroy_oab_decompressor(msoab_decompressor self);
    }
}
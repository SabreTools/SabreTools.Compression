namespace SabreTools.Compression.libmspack
{
    public unsafe class qtmd_stream : readbits
    {
        /// <summary>
        /// Decoding window
        /// </summary>
        public byte* window { get; set; }

        /// <summary>
        /// Window size
        /// </summary>
        public uint window_size { get; set; }

        /// <summary>
        /// Bytes remaining for current frame
        /// </summary>
        public uint frame_todo { get; set; }

        /// <summary>
        /// High arith coding state
        /// </summary>
        public ushort H { get; set; }

        /// <summary>
        /// Low arith coding state
        /// </summary>
        public ushort L { get; set; }

        /// <summary>
        /// Current arith coding state
        /// </summary>
        public ushort C { get; set; }

        /// <summary>
        /// Have we started decoding a new frame?
        /// </summary>
        public byte header_read { get; set; }

        #region Models

        #region Four literal models, each representing 64 symbols

        /// <summary>
        /// model0 for literals from   0 to  63 (selector = 0)
        /// </summary>
        public qtmd_model model0 { get; set; }

        /// <summary>
        /// model1 for literals from  64 to 127 (selector = 1)
        /// </summary>
        public qtmd_model model1 { get; set; }

        /// <summary>
        /// model2 for literals from 128 to 191 (selector = 2)
        /// </summary>
        public qtmd_model model2 { get; set; }

        /// <summary>
        /// model3 for literals from 129 to 255 (selector = 3)
        /// </summary>
        public qtmd_model model3 { get; set; }

        #endregion

        #region Three match models

        /// <summary>
        /// model4 for match with fixed length of 3 bytes
        /// </summary>
        public qtmd_model model4 { get; set; }

        /// <summary>
        /// model5 for match with fixed length of 4 bytes
        /// </summary>
        public qtmd_model model5 { get; set; }

        /// <summary>
        /// model6 for variable length match, encoded with model6len model
        /// </summary>
        public qtmd_model model6 { get; set; }

        public qtmd_model model6len { get; set; }

        #endregion

        /// <summary>
        /// selector model. 0-6 to say literal (0,1,2,3) or match (4,5,6)
        /// </summary>
        public qtmd_model model7 { get; set; }

        #endregion

        #region Symbol arrays for all models

        public qtmd_modelsym[] m0sym { get; set; } = new qtmd_modelsym[64 + 1];

        public qtmd_modelsym[] m1sym { get; set; } = new qtmd_modelsym[64 + 1];

        public qtmd_modelsym[] m2sym { get; set; } = new qtmd_modelsym[64 + 1];

        public qtmd_modelsym[] m3sym { get; set; } = new qtmd_modelsym[64 + 1];

        public qtmd_modelsym[] m4sym { get; set; } = new qtmd_modelsym[24 + 1];

        public qtmd_modelsym[] m5sym { get; set; } = new qtmd_modelsym[36 + 1];

        public qtmd_modelsym[] m6sym { get; set; } = new qtmd_modelsym[42 + 1];

        public qtmd_modelsym[] m6lsym { get; set; } = new qtmd_modelsym[27 + 1];

        public qtmd_modelsym[] m7sym { get; set; } = new qtmd_modelsym[7 + 1];

        #endregion

        public override void READ_BYTES()
        {
            byte b0, b1;
            READ_IF_NEEDED;
            b0 = *i_ptr++;
            READ_IF_NEEDED;
            b1 = *i_ptr++;
            INJECT_BITS_MSB((b0 << 8) | b1, 16);
        }
    }
}
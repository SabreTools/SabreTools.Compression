/* This file is part of libmspack.
 * (C) 2003-2018 Stuart Caie.
 *
 * libmspack is free software; you can redistribute it and/or modify it under
 * the terms of the GNU Lesser General Public License (LGPL) version 2.1
 *
 * For further details, see the file COPYING.LIB distributed with libmspack
 */

namespace SabreTools.Compression.libmspack
{
    public unsafe static class system
    {
        /// <summary>
        /// Returns the length of a file opened for reading
        /// </summary>
        public static MSPACK_ERR mspack_sys_filelen(mspack_system system, mspack_file file, long* length)
        {
            if (system == null || file == null || length == null) return MSPACK_ERR.MSPACK_ERR_OPEN;

            // Get current offset
            long current = system.tell(file);

            // Seek to end of file
            if (system.seek(file, 0, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_END) != 0)
            {
                return MSPACK_ERR.MSPACK_ERR_SEEK;
            }

            // Get offset of end of file
            *length = system.tell(file);

            // Seek back to original offset
            if (system.seek(file, current, MSPACK_SYS_SEEK.MSPACK_SYS_SEEK_START) != 0)
            {
                return MSPACK_ERR.MSPACK_ERR_SEEK;
            }

            return MSPACK_ERR.MSPACK_ERR_OK;
        }
    }
}
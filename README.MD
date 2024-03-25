# SabreTools.Compression

This library comprises of various compression implementations that are used across multiple projects. Most of the implementations are be ports of existing C and C++ code.

Find the link to the Nuget package [here](https://www.nuget.org/packages/SabreTools.Compression).

## Supported Compressions

| Compression Name | Decompress | Compress |
| --- | --- | --- |
| LZ | Yes | No |
| MSZIP | Yes* | No |

**Note:** If something is marked with a `*` it means that it need testing.

## External Libraries

| Library Name | Use |
| --- | ---|
| [ZLibPort](https://github.com/Nanook/zlib-C-To-CSharp-Port) | Adds zlib code for internal and external use; minor edits have been made |

# SabreTools.Compression

[![Build and Test](https://github.com/SabreTools/SabreTools.Compression/actions/workflows/build_and_test.yml/badge.svg)](https://github.com/SabreTools/SabreTools.Compression/actions/workflows/build_and_test.yml)

This library comprises of various compression implementations that are used across multiple projects. Most of the implementations are be ports of existing C and C++ code.

Find the link to the Nuget package [here](https://www.nuget.org/packages/SabreTools.Compression).

## Releases

For the most recent stable build, download the latest release here: [Releases Page](https://github.com/SabreTools/SabreTools.Compression/releases)

For the latest WIP build here: [Rolling Release](https://github.com/SabreTools/SabreTools.Compression/releases/rolling)

## Supported Compressions

| Compression Name | Decompress | Compress |
| --- | --- | --- |
| Blast | Yes | No |
| LZ | Yes | No |
| LZX | No | No |
| MSZIP | Yes* | No |
| Quantum | No | No |

**Note:** If something is marked with a `*` it means that it need testing.

## External Libraries

| Library Name | Use |
| --- | ---|
| [ZLibPort](https://github.com/Nanook/zlib-C-To-CSharp-Port) | Adds zlib code for internal and external use; minor edits have been made |

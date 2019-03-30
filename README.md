# RetroArchPlaylistGenerator
A command-line utility to generate RetroArch playlists from local ROM directories.

Utilizes Lucene.Net to match ROM filenames to those found in the RetroArch databases. 
This performs significantly better than RetroArch's built in scanner, achieving a greater match rate in less time.

Run from a Command Prompt without any arguments for instructions.

Example Usage: RetroArchPlaylistGenerator -ra="c:\retroarch" -ro="c:\roms\genesis"

Note: If it is unable to find a system match for a given folder, give the folder a more descriptive name, as the RetroArch databases do not contain abbreviated names. For example, change "snes" to "super nintendo".

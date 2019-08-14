using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RetroArchPlaylistGenerator
{
    public class RAPlaylistGenerator
    {
        public void GeneratePlaylist(string systemName, string romFolderPath, string outputFolderPath,
            RARomIndex romIndex, bool rename = false)
        {
            var playlist = new RAPlaylist();

            //if (rename)
            //{
            //    var newRomFolderPath = $@"{new DirectoryInfo(romFolderPath).Parent.FullName}\{systemName}";
            //    romFolderPath = romFolderPath.TrimEnd('\\');

            //    if (!newRomFolderPath.Equals(romFolderPath, StringComparison.OrdinalIgnoreCase))
            //    {
            //        Directory.Move(romFolderPath, newRomFolderPath);
            //        romFolderPath = newRomFolderPath;
            //    }
            //}

            string previousRomName = null;
            var ignoreFiles = new List<string>();

            foreach (var romPath in Helpers.GetFiles(romFolderPath, "((.zip)|(.cue)|(.iso)|(.gdi)|(.cdi)|(.wad)|(.bin))$")
                .OrderByDescending(f => Path.GetExtension(f).Equals(".cue", StringComparison.OrdinalIgnoreCase))
                .ThenBy(f => Path.GetExtension(f).Equals(".bin", StringComparison.OrdinalIgnoreCase)))
            {
                var filename = Path.GetFileName(romPath);

                if (ignoreFiles.Exists(f => f.Equals(filename, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var matched = SelectRom(romPath, romIndex, out var romName);

                if (romName == previousRomName)
                    continue;

                Console.WriteLine(!matched
                    ? $"Matching ROM not found for file: {filename}"
                    : $"Matched: {filename} => {romName}");
                var entry = new RAPlaylistEntry
                {
                    path = romPath,
                    label = romName,
                    core_path = "DETECT",
                    core_name = "DETECT",
                    crc32 = "DETECT",
                    db_name = $"{systemName}.lpl"
                };

                if (rename)
                {
                    foreach (var c in Path.GetInvalidFileNameChars())
                        romName = romName.Replace(c.ToString(), null);

                    var newRomPath = $@"{Path.GetDirectoryName(romPath)}\{romName}{Path.GetExtension(romPath)}";

                    if (!newRomPath.Equals(romPath, StringComparison.OrdinalIgnoreCase))
                    {
                        if (File.Exists(newRomPath))
                        {
                            Console.WriteLine("Skipping file because a file with the same name already exists.");
                            continue;
                        }

                        File.Move(romPath, newRomPath);
                        entry.path = newRomPath;
                    }
                }

                playlist.items.Add(entry);
                previousRomName = romName;

                if (Path.GetExtension(filename).Equals(".cue", StringComparison.OrdinalIgnoreCase))
                {
                    var binFiles = Helpers.ParseCueFile(entry.path);
                    ignoreFiles.AddRange(binFiles);
                }
            }

            playlist.items = playlist.items.OrderBy(i => i.label).ToList();
            File.WriteAllText($@"{outputFolderPath}\{systemName}.lpl",
                JsonConvert.SerializeObject(playlist, Formatting.Indented).Replace("\r\n", "\n"));
        }

        private static bool SelectRom(string romPath, RARomIndex index, out string romName)
        {
            var fileName = romName = Path.GetFileNameWithoutExtension(romPath);
            var matches = index.GetRom(fileName).Where(r => r.Score > .1f).ToList();

            if (!matches.Any())
                return false;

            if (matches.First().Score >= 1)
            {
                romName = matches.First().Name;
                return true;
            }

            Console.WriteLine();
            Console.WriteLine($"Possible matches for file: {fileName}");
            Console.WriteLine();

            for (var i = 0; i < matches.Count; ++i)
                Console.WriteLine($"[{i + 1}] {matches[i].Name} ({matches[i].Score})");

            Console.WriteLine($"[{matches.Count + 1}] NONE OF THE ABOVE");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Which ROM is this? ");
                var input = Console.ReadLine();

                if (int.TryParse(input, out var optionIndex) && optionIndex > 0 && optionIndex <= matches.Count + 1)
                {
                    var none = optionIndex - 1 >= matches.Count;
                    romName = none ? fileName : matches[optionIndex - 1].Name;
                    return !none;
                }
            }
        }
    }
}
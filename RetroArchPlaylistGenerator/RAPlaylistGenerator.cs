using System;
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

            if (rename)
            {
                var newRomFolderPath = $@"{new DirectoryInfo(romFolderPath).Parent.FullName}\{systemName}";
                Directory.Move(romFolderPath, newRomFolderPath);
                romFolderPath = newRomFolderPath;
            }

            foreach (var romPath in Helpers.GetFiles(romFolderPath, "((.zip)|(.cue)|(.iso))$"))
            {
                var romName = SelectRom(romPath, romIndex);

                if (romName == null)
                {
                    Console.WriteLine($"Matching ROM not found for file: {Path.GetFileName(romPath)}");
                    continue;
                }

                Console.WriteLine($"Matched: {Path.GetFileName(romPath)} => {romName}");
                var entry = new RAPlaylistEntry
                {
                    path = romPath,
                    label = romName,
                    core_path = "DETECT",
                    core_name = "DETECT",
                    crc32 = "DETECT",
                    db_name = $"{systemName}.lpl"
                };

                //if (rename)
                //{
                //    var newRomPath = $@"{Path.GetDirectoryName(romPath)}\{romName}{Path.GetExtension(romPath)}";
                //    File.Move(romPath, newRomPath);
                //    entry.path = newRomPath;
                //}

                playlist.items.Add(entry);
            }

            playlist.items = playlist.items.OrderBy(i => i.label).ToList();
            File.WriteAllText($@"{outputFolderPath}\{systemName}.lpl",
                JsonConvert.SerializeObject(playlist, Formatting.Indented).Replace("\r\n", "\n"));
        }

        private static string SelectRom(string romPath, RARomIndex index)
        {
            var matches = index.GetRom(Path.GetFileName(romPath));

            if (!matches.Any())
                return null;

            if (matches.First().Score >= 1)
                return matches.First().Name;

            Console.WriteLine();
            Console.WriteLine($"Multiple possible ROM matches found for file: {Path.GetFileName(romPath)}");
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
                    return optionIndex - 1 >= matches.Count ? null : matches[optionIndex - 1].Name;
            }
        }
    }
}
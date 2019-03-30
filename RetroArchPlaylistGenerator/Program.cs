using System;
using System.IO;
using System.Linq;
using Mono.Options;

namespace RetroArchPlaylistGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var options = new Options();
            var optionSet = new OptionSet
            {
                {
                    "ra=|retroarch=", "The path to the RetroArch install directory.",
                    v => options.RetroArchFolderPath = v
                },
                {
                    "ro=|roms=", "The path to the folder containing the ROMs that you want to generate a playlist of.",
                    v => options.RomsFolderPath = v
                },
                {
                    "r|rename",
                    "If set, will rename the ROM folder to match the RetroArch system name.",
                    v => options.RenameRoms = true
                },
                {"h|?|help", v => options.Help = true}
            };

            try
            {
                optionSet.Parse(args);
            }
            catch
            {
                options.Help = true;
            }

            if (options.Help || !args.Any())
            {
                optionSet.WriteOptionDescriptions(Console.Out);
                return;
            }

            var systemIndex = new RASystemIndex(options.RetroArchFolderPath);
            var romFolderName = new DirectoryInfo(options.RomsFolderPath).Name;
            var systemName = SelectSystem(systemIndex, romFolderName);

            if (systemName == null)
            {
                Console.WriteLine($"No system matches found for folder: {romFolderName}");
                return;
            }

            using (var romIndex = new RARomIndex($@"{options.RetroArchFolderPath}\database\rdb\{systemName}.rdb"))
            {
                var playlistGenerator = new RAPlaylistGenerator();
                playlistGenerator.GeneratePlaylist(
                    systemName, 
                    options.RomsFolderPath,
                    $@"{options.RetroArchFolderPath}\playlists\",
                    romIndex,
                    options.RenameRoms);
            }
        }

        private static string SelectSystem(RASystemIndex index, string romFolderName)
        {
            var matches = index.GetSystems(romFolderName);

            if (!matches.Any())
                return null;

            if (matches.First().Score >= 1)
                return matches.First().Name;

            Console.WriteLine("Multiple possible system matches found.");
            Console.WriteLine();

            for (var i = 0; i < matches.Count; ++i)
                Console.WriteLine($"[{i + 1}] {matches[i].Name} ({matches[i].Score})");

            Console.WriteLine($"[{matches.Count + 1}] NONE OF THE ABOVE");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Which system do these ROMs belong to? ");
                var input = Console.ReadLine();

                if (int.TryParse(input, out var optionIndex) && optionIndex > 0 && optionIndex <= matches.Count + 1)
                    return optionIndex - 1 >= matches.Count ? null : matches[optionIndex - 1].Name;
            }
        }

        public class Options
        {
            public string RetroArchFolderPath;
            public string RomsFolderPath;
            public bool RenameRoms;
            public bool Help;
        }
    }
}
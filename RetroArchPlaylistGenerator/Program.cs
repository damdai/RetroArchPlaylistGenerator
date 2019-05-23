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
                    "i=|install=", "The path to the RetroArch install directory.",
                    v => options.RetroArchInstallDirectoryPath = v
                },
                {
                    "r=|roms=", "The path to the folder containing the ROMs that you want to generate a playlist of.",
                    v => options.RomsDirectoryPath = v
                },
                {
                    "rename",
                    "If set, will rename the ROM folder to match the RetroArch system name.",
                    v => options.Rename = true
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

            var systemIndex = new RASystemIndex(options.RetroArchInstallDirectoryPath);
            var romFolderName = new DirectoryInfo(options.RomsDirectoryPath).Name;
            var systemName = SelectSystem(systemIndex, romFolderName);

            if (systemName == null)
            {
                Console.WriteLine($"No system matches found for folder: {romFolderName}");
                return;
            }

            using (var romIndex = new RARomIndex($@"{options.RetroArchInstallDirectoryPath}\database\rdb\{systemName}.rdb"))
            {
                var playlistGenerator = new RAPlaylistGenerator();
                playlistGenerator.GeneratePlaylist(
                    systemName, 
                    options.RomsDirectoryPath,
                    $@"{options.RetroArchInstallDirectoryPath}\playlists\",
                    romIndex,
                    options.Rename);
            }
        }

        private static string SelectSystem(RASystemIndex index, string romFolderName)
        {
            var matches = index.GetSystems(romFolderName);

            if (!matches.Any())
                return null;

            if (matches.Count(m => m.Score >= 1) == 1)
                return matches.First(m => m.Score >= 1).Name;

            Console.WriteLine("Possible system matches:");
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
            public string RetroArchInstallDirectoryPath;
            public string RomsDirectoryPath;
            public bool Rename;
            public bool Help;
        }
    }
}
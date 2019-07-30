using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RetroArchPlaylistGenerator
{
    public static class Helpers
    {
        public static IEnumerable<string> GetFiles(string directoryPath, string regexFilter)
        {
            return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                .Where(filePath => Regex.IsMatch(filePath, regexFilter));
        }

        internal static void DeleteFilesInFolder(string path)
        {
            if (!Directory.Exists(path))
                return;

            Array.ForEach(Directory.GetFiles(path, "*.*"), File.Delete);
        }

        internal static List<string> ParseCueFile(string cueFilePath)
        {
            var matches = Regex.Matches(File.ReadAllText(cueFilePath), "FILE \"(?<filename>.+)\"");
            var list = new List<string>();

            foreach (Match match in matches)
                list.Add(match.Groups["filename"].Value);

            return list;
        }
    }
}
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
    }
}
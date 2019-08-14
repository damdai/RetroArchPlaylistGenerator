using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace RetroArchPlaylistGenerator
{
    public class RARomIndex : IDisposable
    {
        private const string IndexLocation = @".\Index\Roms";
        private const string NameSimplificationRegex = @"[^a-z0-9 \(\)]";
        private const string CamelCaseSplitRegex = @"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])";

        private StandardAnalyzer _analyzer;
        private FSDirectory _indexDir;
        private IndexSearcher _searcher;

        public RARomIndex(string rdbFilePath)
        {
            GenerateIndex(rdbFilePath);
        }

        public void Dispose()
        {
            _indexDir?.Dispose();
            _searcher?.Dispose();
            _analyzer?.Dispose();
        }

        private void GenerateIndex(string rdbFilePath)
        {
            Console.WriteLine("Generating ROM index...");
            var indexPath = $@"{IndexLocation}\{Path.GetFileNameWithoutExtension(rdbFilePath)}";
            Helpers.DeleteFilesInFolder(indexPath);
            _indexDir = FSDirectory.Open(indexPath);
            _analyzer = new StandardAnalyzer(Version.LUCENE_30);

            using (var writer = new IndexWriter(_indexDir, _analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                foreach (var dbEntry in ParseDatabase(rdbFilePath))
                {
                    var doc = new Document();
                    var simplifiedName = Regex.Replace(dbEntry.Name, NameSimplificationRegex, "", RegexOptions.IgnoreCase);
                    simplifiedName = Regex.Replace(simplifiedName, CamelCaseSplitRegex, " ", RegexOptions.IgnorePatternWhitespace);
                    doc.Add(new Field("SimplifiedName", simplifiedName, Field.Store.YES, Field.Index.ANALYZED));
                    doc.Add(new Field("Name", dbEntry.Name, Field.Store.YES, Field.Index.NOT_ANALYZED));

                    if (dbEntry.RomName != null)
                        doc.Add(new Field("RomName", dbEntry.RomName, Field.Store.YES, Field.Index.NOT_ANALYZED));

                    writer.AddDocument(doc);
                }

                writer.Optimize();
            }

            _searcher = new IndexSearcher(_indexDir);
        }

        private static IEnumerable<RADatabaseEntry> ParseDatabase(string rdbFilePath)
        {
            using (var stream = File.OpenRead(rdbFilePath))
            {
                var header = stream.ReadBytes(16);

                if (!header.SequenceEqual(new byte[]
                    {0x52, 0x41, 0x52, 0x43, 0x48, 0x44, 0x42, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}))
                    throw new Exception("Invalid database file.");

                while (stream.Peek() != 0xc0)
                    yield return new RADatabaseEntry(stream);
            }
        }

        public List<(string Name, float Score)> GetRom(string romFileName)
        {
            var collector = TopScoreDocCollector.Create(10, true);

            //First, match on RomName.
            var query1 = new TermQuery(new Term("RomName", romFileName));
            _searcher.Search(query1, collector);
            var hits = collector.TopDocs().ScoreDocs;
            var docs = hits.Select(h => (Doc: _searcher.Doc(h.Doc), h.Score)).Where(h => h.Score > 1);

            if (!docs.Any())
            {
                //If not found, match on Name.
                var parser = new QueryParser(Version.LUCENE_30, "SimplifiedName", _analyzer);
                var simplifiedName = Regex.Replace(romFileName, NameSimplificationRegex, "", RegexOptions.IgnoreCase);
                simplifiedName = Regex.Replace(simplifiedName, CamelCaseSplitRegex, " ", RegexOptions.IgnorePatternWhitespace);
                var query2 = parser.Parse(simplifiedName);
                _searcher.Search(query2, collector);
                hits = collector.TopDocs().ScoreDocs;
                docs = hits.Select(h => (Doc: _searcher.Doc(h.Doc), h.Score));
            }

            return docs.Select(d => (Name: d.Doc.GetField("Name").StringValue, d.Score))
                .OrderByDescending(d => d.Score).ThenBy(d => d.Name).ToList();
        }
    }
}
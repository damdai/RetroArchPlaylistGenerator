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
using Directory = System.IO.Directory;
using Version = Lucene.Net.Util.Version;

namespace RetroArchPlaylistGenerator
{
    public class RASystemIndex
    {
        private const string IndexLocation = @".\Index\Systems";

        public RASystemIndex(string retroarchFolderPath)
        {
            GenerateIndex(retroarchFolderPath);
        }

        private static void GenerateIndex(string retroarchFolderPath)
        {
            Console.WriteLine("Generating system index...");
            Helpers.DeleteFilesInFolder(IndexLocation);
            var dbDir = $@"{retroarchFolderPath}\database\rdb\";

            using (var indexOutputDir = FSDirectory.Open(IndexLocation))
            {
                using (var analyzer = new StandardAnalyzer(Version.LUCENE_30))
                {
                    using (var writer = new IndexWriter(indexOutputDir, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
                    {
                        foreach (var dbPath in Directory.EnumerateFiles(dbDir, "*.rdb"))
                        {
                            var systemName = Path.GetFileNameWithoutExtension(dbPath);
                            var doc = new Document();
                            doc.Add(new Field("Name", systemName, Field.Store.YES, Field.Index.ANALYZED));
                            writer.AddDocument(doc);
                        }

                        writer.Optimize();
                    }
                }
            }
        }

        public List<(string Name, float Score)> GetSystems(string query)
        {
            using (var indexDir = FSDirectory.Open(IndexLocation))
            {
                using (var searcher = new IndexSearcher(indexDir))
                {
                    using (var analyzer = new StandardAnalyzer(Version.LUCENE_30))
                    {
                        var nameParser = new QueryParser(Version.LUCENE_30, "Name", analyzer);
                        var nameQuery = nameParser.Parse(Regex.Replace(query,
                            @"[^a-z0-9!']", " ", RegexOptions.IgnoreCase));
                        var collector = TopScoreDocCollector.Create(3, true);
                        searcher.Search(nameQuery, collector);
                        var hits = collector.TopDocs().ScoreDocs;
                        var docs = hits.Select(h => (Doc: searcher.Doc(h.Doc), Score: h.Score));
                        return docs.Select(d => (Name: d.Doc.GetField("Name").StringValue, Score: d.Score)).ToList();
                    }
                }
            }
        }
    }
}
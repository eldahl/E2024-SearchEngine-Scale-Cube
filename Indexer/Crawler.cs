using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Indexer
{
    public class Crawler
    {
        private readonly char[] sep = " \\\n\t\"$'!,?;.:-_**+=)([]{}<>/@&%€#".ToCharArray();

        private Dictionary<string, int> words = new Dictionary<string, int>();
        private Dictionary<string, int> documents = new Dictionary<string, int>();

        private readonly HttpClient _api;

        public Crawler()
        {
            _api = new HttpClient { BaseAddress = new Uri("http://localhost:8080") };
        }

        // Return a dictionary containing all words (as the key) in the file
        // [f] and the value is the number of occurrences of the key in file.
        private ISet<string> ExtractWordsInFile(FileInfo f)
        {
            ISet<string> res = new HashSet<string>();
            var content = File.ReadAllLines(f.FullName);
            foreach (var line in content)
            {
                foreach (var aWord in line.Split(sep, StringSplitOptions.RemoveEmptyEntries))
                {
                    res.Add(aWord);
                }
            }

            return res;
        }

        private ISet<int> GetWordIdFromWords(ISet<string> src)
        {
            ISet<int> res = new HashSet<int>();

            foreach (var p in src)
            {
                res.Add(words[p]);
            }
            return res;
        }

        // Method to post a document to the API
        private void PostDocument(int id, string url)
        {
            var documentMessage = new HttpRequestMessage(HttpMethod.Post, $"Document?id={id}&url={Uri.EscapeDataString(url)}");
            var res = _api.SendAsync(documentMessage).Result;
            Console.WriteLine(res.StatusCode);
        }

        // Method to post words to the API
        private void PostWords(Dictionary<string, int> newWords)
        {
            var wordMessage = new HttpRequestMessage(HttpMethod.Post, "Word");
            wordMessage.Content = JsonContent.Create(newWords);
            var res = _api.SendAsync(wordMessage).Result;
            Console.WriteLine(res.StatusCode);
        }

        // Method to post occurrences to the API
        private void PostOccurrences(int docId, ISet<int> wordIds)
        {
            var occurrenceMessage = new HttpRequestMessage(HttpMethod.Post, $"Occurrence?docId={docId}");
            occurrenceMessage.Content = JsonContent.Create(wordIds);
            var res = _api.SendAsync(occurrenceMessage).Result;
            Console.WriteLine(res.StatusCode);
        }

        // Return a dictionary of all the words (the key) in the files contained
        // in the directory [dir]. Only files with an extension in
        // [extensions] is read. The value part of the return value is
        // the number of occurrences of the key.
        public void IndexFilesIn(DirectoryInfo dir, List<string> extensions)
        {
            Console.WriteLine("Crawling " + dir.FullName);

            foreach (var file in dir.EnumerateFiles())
            {
                if (extensions.Contains(file.Extension))
                {
                    documents.Add(file.FullName, documents.Count + 1);

                    // Post the document to the API
                    PostDocument(documents[file.FullName], file.FullName);

                    Dictionary<string, int> newWords = new Dictionary<string, int>();
                    ISet<string> wordsInFile = ExtractWordsInFile(file);

                    foreach (var aWord in wordsInFile)
                    {
                        if (!words.ContainsKey(aWord))
                        {
                            words.Add(aWord, words.Count + 1);
                            newWords.Add(aWord, words[aWord]);
                        }
                    }

                    // Post the new words to the API
                    PostWords(newWords);

                    // Post the occurrences to the API
                    PostOccurrences(documents[file.FullName], GetWordIdFromWords(wordsInFile));
                }
            }

            foreach (var d in dir.EnumerateDirectories())
            {
                IndexFilesIn(d, extensions);
            }
        }
    }
}

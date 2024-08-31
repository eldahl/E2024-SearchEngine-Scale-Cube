using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ConsoleSearch
{
    public class SearchLogic
    {
        private readonly HttpClient _api;
        private Dictionary<string, int> _words;

        public SearchLogic()
        {
            _api = new HttpClient { BaseAddress = new Uri("http://localhost:8080") };
            _words = GetAllWords();
        }

        // Method to get all words from the API
        private Dictionary<string, int> GetAllWords()
        {
            var response = _api.GetAsync("/Word").Result;
            response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(content);
            return JsonSerializer.Deserialize<Dictionary<string, int>>(content);
        }

        public int GetIdOf(string word)
        {
            if (_words.ContainsKey(word))
                return _words[word];
            return -1;
        }

        // Updated method to get documents using HttpClient
        public Dictionary<int, int> GetDocuments(List<int> wordIds)
        {
            var url = "Document/GetByWordIds";
            var response = _api.PostAsync(url, new StringContent(JsonSerializer.Serialize(wordIds),Encoding.UTF8, "application/json")).Result;
            response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<Dictionary<int, int>>(content);
        }

        // Updated method to get document details using HttpClient
        public List<string> GetDocumentDetails(List<int> docIds)
        {
            var url = "Document/GetByDocIds";
            Console.WriteLine(url);
            var response = _api.PostAsync(url, new StringContent(JsonSerializer.Serialize(docIds), Encoding.UTF8, "application/json")).Result;
            response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<List<string>>(content);
        }
    }
}

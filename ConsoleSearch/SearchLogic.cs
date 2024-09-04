using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Polly;
using WordService;

namespace ConsoleSearch
{
    public class SearchLogic
    {
        private readonly HttpClient _api;
        private Dictionary<string, int> _words;

        public SearchLogic()
        {
            _api = new HttpClient { BaseAddress = new Uri("http://localhost:8080") };
            _words = GetAllWords().Result;
        }

        // Method to get all words from the API
        private Task<Dictionary<string, int>> GetAllWords()
        {
            var retryPolicy = Policy.Handle<Exception>()
                .RetryAsync(2);
            var cbPolicy = Policy.Handle<Exception>()
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30),
                    (exception, duration) =>
                    {
                        Console.WriteLine(
                            "[GetAllWords] WordService unavailable after 3 retries, waiting for 30 seconds.");
                    },
                    () => Console.WriteLine("[GetAllWords] Trying to resume operation...")
                );

            var fallbackPolicy = Policy.Handle<Exception>()
                .FallbackAsync((action) =>
                {
                    Console.WriteLine("[GetAllWords] Fallback to empty list.");
                    return null;
                });

            var policy = fallbackPolicy.WrapAsync(retryPolicy.WrapAsync(cbPolicy));

            return policy.ExecuteAsync(async () =>
            {
                Console.WriteLine("Executing operation");
                var response = await _api.GetAsync("/Word");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
                return JsonSerializer.Deserialize<Dictionary<string, int>>(content);
            });
        }

        public int GetIdOf(string word)
        {
            if (_words.ContainsKey(word))
                return _words[word];
            return -1;
        }

        // Updated method to get documents using HttpClient
        public Task<Dictionary<int, int>> GetDocuments(List<int> wordIds)
        {
            var retryPolicy = Policy.Handle<Exception>()
                .RetryAsync(2);
            var cbPolicy = Policy.Handle<Exception>()
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30),
                    (exception, duration) =>
                    {
                        Console.WriteLine(
                            "[GetDocuments] WordService unavailable after 3 retries, waiting for 30 seconds.");
                    },
                    () => Console.WriteLine("[GetDocuments] Trying to resume operation...")
                );

            var fallbackPolicy = Policy.Handle<Exception>()
                .FallbackAsync((action) =>
                {
                    Console.WriteLine("[GetDocuments] Fallback to empty list.");
                    return null;
                });

            var policy = fallbackPolicy.WrapAsync(retryPolicy.WrapAsync(cbPolicy));

            return policy.ExecuteAsync(async () =>
            {
                var url = "/Document/GetByWordIds";
                var json = JsonSerializer.Serialize(wordIds);
                var SC = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _api.PostAsync(url, SC);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<int, int>>(content);
            });
        }

        // Updated method to get document details using HttpClient
        public Task<List<string>> GetDocumentDetails(List<int> docIds)
        {
            var retryPolicy = Policy.Handle<Exception>()
                .RetryAsync(2);
            var cbPolicy = Policy.Handle<Exception>()
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30),
                    (exception, duration) =>
                    {
                        Console.WriteLine(
                            "[GetDocumentDetails] WordService unavailable after 3 retries, waiting for 30 seconds.");
                    },
                    () => Console.WriteLine("[GetDocumentDetails] Trying to resume operation...")
                );

            var fallbackPolicy = Policy.Handle<Exception>()
                .FallbackAsync((action) =>
                {
                    Console.WriteLine("[GetDocumentDetails] Fallback to empty list.");
                    return null;
                });

            var policy = fallbackPolicy.WrapAsync(retryPolicy.WrapAsync(cbPolicy));
            return policy.ExecuteAsync(async () =>
            {
                var url = "/Document/GetByDocIds";
                Console.WriteLine(url);
                var response = await _api.PostAsync(url,
                    new StringContent(JsonSerializer.Serialize(docIds), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(content);
            });
        }
    }
}
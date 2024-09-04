using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleSearch
{
    public class App
    {
        public void Run()
        {
            SearchLogic mSearchLogic = new SearchLogic();
            Console.WriteLine("Console Search");

            while (true)
            {
                Console.WriteLine("enter search terms - q for quit [default: hello]");
                string input = Console.ReadLine() ?? "hello"; // Search for "hello" by default
                if (input.Equals("q")) break;

                var wordIds = new List<int>();
                var searchTerms = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                foreach (var t in searchTerms)
                {
                    int id = mSearchLogic.GetIdOf(t);
                    if (id != -1)
                    {
                        wordIds.Add(id);
                    }
                    else
                    {
                        Console.WriteLine($"{t} will be ignored");
                    }
                }

                DateTime start = DateTime.Now;

                var docIds = mSearchLogic.GetDocuments(wordIds).Result;

                // get details for the first 10             
                var top10 = docIds.Keys.Take(10).ToList();

                TimeSpan used = DateTime.Now - start;

                int idx = 0;
                var docDetails = mSearchLogic.GetDocumentDetails(top10).Result;
                foreach (var doc in docDetails)
                {
                    Console.WriteLine($"{idx + 1}: {doc} -- contains {docIds[top10[idx]]} search terms");
                    idx++;
                }
                Console.WriteLine($"Documents: {docIds.Count}. Time: {used.TotalMilliseconds}");

                Thread.Sleep(1000);
            }
        }
    }
}

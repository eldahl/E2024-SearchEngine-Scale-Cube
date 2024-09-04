using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using Microsoft.Data.SqlClient;

namespace WordService
{
    public class Database
    {
        private readonly Coordinator _coordinator;

        // Public constructor
        public Database(Coordinator coordinator)
        {
            _coordinator = coordinator;
        }

        // Private method to execute SQL commands
        private void Execute(string sql, IDbConnection _connection)
        {
            using var trans = _connection.BeginTransaction();
            var cmd = _connection.CreateCommand();
            cmd.Transaction = trans;
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            trans.Commit();
        }

        // Method to delete the existing database (used in Indexer)
        public void DeleteDatabase()
        {
            foreach (var conn in _coordinator.GetAllConnections())
            {
                Execute("DROP TABLE IF EXISTS Occurrences",conn);
                Execute("DROP TABLE IF EXISTS Words",conn);
                Execute("DROP TABLE IF EXISTS Documents",conn);
            }
        }

        // Method to recreate the database schema (used in Indexer)
        public void RecreateDatabase()
        {
            foreach (var conn in _coordinator.GetAllConnections())
            {
                Execute("CREATE TABLE Documents(id INTEGER PRIMARY KEY, url VARCHAR(500))",conn);
                Execute("CREATE TABLE Words(id INTEGER PRIMARY KEY, name VARCHAR(500))",conn);
                Execute("CREATE TABLE Occurrences(wordId INTEGER, docId INTEGER)",conn);
            }
        }

        // Method to insert a document into the database (used in Indexer)
        public void InsertDocument(int id, string url)
        {
            var _connection = _coordinator.GetDocumentConnection();
            var insertCmd = _connection.CreateCommand();
            insertCmd.CommandText = "INSERT INTO Documents(id, url) VALUES(@id,@url)";

            var pName = new SqlParameter("url", url);
            insertCmd.Parameters.Add(pName);

            var pCount = new SqlParameter("id", id);
            insertCmd.Parameters.Add(pCount);

            insertCmd.ExecuteNonQuery();
        }

        // Method to insert all words into the database (used in Indexer)
        public void InsertAllWords(Dictionary<string, int> res)
        {
            foreach (var p in res)
            {
                var connection = _coordinator.GetWordConnection(p.Key);
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = @"INSERT INTO Words(id, name) VALUES(@id,@name)";

                    var paramName = command.CreateParameter();
                    paramName.ParameterName = "name";
                    command.Parameters.Add(paramName);

                    var paramId = command.CreateParameter();
                    paramId.ParameterName = "id";
                    command.Parameters.Add(paramId);
                    paramName.Value = p.Key;
                    paramId.Value = p.Value;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }

        // Method to insert all occurrences of words in a document (used in Indexer)
        public void InsertAllOcc(int docId, ISet<int> wordIds)
        {
            var _connection = _coordinator.GetOccurrenceConnection();
            using (var transaction = _connection.BeginTransaction())
            {
                var command = _connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"INSERT INTO Occurrences(wordId, docId) VALUES(@wordId,@docId)";

                var paramwordId = command.CreateParameter();
                paramwordId.ParameterName = "wordId";
                command.Parameters.Add(paramwordId);

                var paramDocId = command.CreateParameter();
                paramDocId.ParameterName = "docId";
                paramDocId.Value = docId;
                command.Parameters.Add(paramDocId);

                foreach (var p in wordIds)
                {
                    paramwordId.Value = p;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        // Method to retrieve documents based on word IDs (used in ConsoleSearch)
        public Dictionary<int, int> GetDocuments(List<int> wordIds)
        {
            var _connection = _coordinator.GetOccurrenceConnection();
            var res = new Dictionary<int, int>();

            var sql = @"SELECT docId, COUNT(wordId) AS count FROM Occurrences WHERE wordId IN " + AsString(wordIds) + " GROUP BY docId ORDER BY count DESC;";
            Console.WriteLine(sql);
            foreach (var wid in wordIds)
            { 
                Console.WriteLine(wid);
            }
            
            var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = sql;

            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var docId = reader.GetInt32(0);
                    var count = reader.GetInt32(1);

                    res.Add(docId, count);
                }
            }
            Console.WriteLine(res);
            foreach (var kv in res)
            {
                Console.WriteLine("key: " + kv.Key + " | value: " + kv.Value);
            }
            return res;
        }

        // Method to retrieve all words from the database (used in both Indexer and ConsoleSearch)
        public Dictionary<string, int> GetAllWords()
        {
            
            Dictionary<string, int> res = new Dictionary<string, int>();
            foreach (var conn in _coordinator.GetAllWordConnections())
            {
                var selectCmd = conn.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM Words";

                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var w = reader.GetString(1);

                        res.Add(w, id);
                    }
                }
            }

            return res;
        }

        // Method to retrieve document details by document IDs (used in ConsoleSearch)
        public List<string> GetDocDetails(List<int> docIds)
        {
            var _connection = _coordinator.GetDocumentConnection();
            List<string> res = new List<string>();

            var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Documents WHERE id IN " + AsString(docIds);

            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var url = reader.GetString(1);

                    res.Add(url);
                }
            }
            return res;
        }

        // Utility method to format list of integers as SQL-compatible string
        private string AsString(List<int> x)
        {
            return string.Concat("(", string.Join(',', x.Select(i => i.ToString())), ")");
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace WordService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly Database _database;

        // Constructor injection of the Database service
        public DocumentController(Database database)
        {
            _database = database;
        }

        [HttpPost("GetByDocIds")]
        public List<string> GetByDocIds([FromBody] List<int> docIds)
        {
            return _database.GetDocDetails(docIds);
        }

        [HttpPost("GetByWordIds")]
        public Dictionary<int, int> GetByWordIds([FromBody] List<int> wordIds)
        {
            return _database.GetDocuments(wordIds);
        }

        [HttpPost]
        public void Post(int id, string url)
        {
            _database.InsertDocument(id, url);
        }
    }
}

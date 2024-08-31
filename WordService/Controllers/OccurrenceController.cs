using Microsoft.AspNetCore.Mvc;

namespace WordService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OccurrenceController : ControllerBase
    {
        private readonly Database _database;

        // Constructor injection of the Database service
        public OccurrenceController(Database database)
        {
            _database = database;
        }


        [HttpPost]
        public void Post(int docId, [FromBody] ISet<int> wordIds)
        {
            _database.InsertAllOcc(docId, wordIds);
        }
    }
}

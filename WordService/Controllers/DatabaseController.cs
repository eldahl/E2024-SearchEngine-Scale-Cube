using Microsoft.AspNetCore.Mvc;

namespace WordService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly Database _database;

        // Constructor injection of the Database service
        public DatabaseController(Database database)
        {
            _database = database;
        }

        [HttpDelete]
        public void Delete()
        {
            _database.DeleteDatabase();
        }

        [HttpPost]
        public void Post()
        {
            _database.RecreateDatabase();
        }
    }
}

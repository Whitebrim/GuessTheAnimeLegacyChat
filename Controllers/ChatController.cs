using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Messages;

namespace Chat.Controllers
{
    [Route("api")]
    public class ChatController : ControllerBase
    {
        public AppDb Db { get; }

        public ChatController(AppDb db)
        {
            Db = db;
        }

        // GET api
        [HttpGet]
        public async Task<IActionResult> GetLatest()
        {
            await Db.Connection.OpenAsync();
            var query = new MessageQuery(Db);
            var result = await query.LatestPostsAsync();
            var json = await Task.Run(() => JsonConvert.SerializeObject(result));
            return new OkObjectResult(json);
        }

        // GET api/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            await Db.Connection.OpenAsync();
            var query = new MessageQuery(Db);
            var result = await query.FindOneAsync(id);
            if (result is null)
                return new NotFoundResult();
            var json = await Task.Run(() => JsonConvert.SerializeObject(result));
            return new OkObjectResult(json);
        }

        // POST api
        [HttpPost]
        public async Task<IActionResult> Post(string userId, string text)
        {
            MessageBase body = new MessageBase(userId, text);
            await Db.Connection.OpenAsync();
            body.Db = Db;
            await body.InsertAsync();
            var query = new MessageQuery(Db);
            var result = await query.LatestPostsAsync();
            var json = await Task.Run(() => JsonConvert.SerializeObject(result));
            return new OkObjectResult(json);
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Controllers
{
    [Route("api")]
    public class ChatController : ControllerBase
    {
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
            return new OkObjectResult(result);
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
            return new OkObjectResult(result);
        }

        // POST api
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Message body)
        {
            await Db.Connection.OpenAsync();
            body.Db = Db;
            await body.InsertAsync();
            return new OkObjectResult(body);
        }

        // PUT api/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOne(int id, [FromBody]Message body)
        {
            await Db.Connection.OpenAsync();
            var query = new MessageQuery(Db);
            var result = await query.FindOneAsync(id);
            if (result is null)
                return new NotFoundResult();
            result.nickname = body.nickname;
            result.messageBody = body.messageBody;
            result.messageDate = body.messageDate;
            await result.UpdateAsync();
            return new OkObjectResult(result);
        }

        // DELETE api/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOne(int id)
        {
            await Db.Connection.OpenAsync();
            var query = new MessageQuery(Db);
            var result = await query.FindOneAsync(id);
            if (result is null)
                return new NotFoundResult();
            await result.DeleteAsync();
            return new OkResult();
        }

        // DELETE api
        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            await Db.Connection.OpenAsync();
            var query = new MessageQuery(Db);
            await query.DeleteAllAsync();
            return new OkResult();
        }

        public AppDb Db { get; }
    }
}

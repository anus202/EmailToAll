using EmailToAll.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailToAll.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class HistoryController : Controller
    {
        private readonly EmailHistoryService _service;

        public HistoryController(EmailHistoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<JsonResult> List([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var res = await _service.ListAsync(User.GetUserId(), search, page, pageSize);
                return Json(new { success = true, data = res });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<JsonResult> Get(int id)
        {
            try
            {
                var res = await _service.GetAsync(User.GetUserId(), id);
                return Json(new { success = true, data = res });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

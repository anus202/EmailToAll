using EmailToAll.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailToAll.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly EmailHistoryService _service;

        public DashboardController(EmailHistoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<JsonResult> Get()
        {
            try
            {
                var res = await _service.DashboardAsync(User.GetUserId());
                return Json(new { success = true, data = res });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

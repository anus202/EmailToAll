using EmailToAll.Payload;
using EmailToAll.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailToAll.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class EmailSetupController : Controller
    {
        private readonly EmailSetupService _service;

        public EmailSetupController(EmailSetupService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<JsonResult> Get()
        {
            try
            {
                var res = await _service.GetAsync(User.GetUserId());
                return Json(new { success = true, data = res });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Save")]
        public async Task<JsonResult> Save([FromBody] EmailSetupPayload data)
        {
            try
            {
                var res = await _service.SaveAsync(User.GetUserId(), data);
                return Json(new { success = true, message = "Email setup saved", data = res });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

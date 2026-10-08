using EmailToAll.Payload;
using EmailToAll.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailToAll.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class EmailController : Controller
    {
        // A little above SendEmailService.MaxAttachmentBytes to leave room for the form fields.
        private const long RequestLimit = 30 * 1024 * 1024;

        private readonly SendEmailService _service;

        public EmailController(SendEmailService service)
        {
            _service = service;
        }

        [HttpPost("Send")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(RequestLimit)]
        [RequestFormLimits(MultipartBodyLengthLimit = RequestLimit)]
        public async Task<JsonResult> Send([FromForm] SendEmailPayload data)
        {
            try
            {
                var res = await _service.SendAsync(User.GetUserId(), data);
                return Json(new { success = true, message = "Email sent", data = res });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

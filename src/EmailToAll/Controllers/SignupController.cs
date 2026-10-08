using EmailToAll.Models;
using EmailToAll.Payload;
using EmailToAll.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailToAll.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SignupController : Controller
    {
        private readonly SignupService _service;

        public SignupController(SignupService signupService)
        {
            _service = signupService;
        }
        [AllowAnonymous]

        [HttpPost("Signup")]
        public async Task<JsonResult> SignupForm([FromBody] Signup signup)
        {
            try
            {
                var res = await _service.Signup(signup);

                if (res != null)
                {
                    return Json(new { success = true, data = res });
                }

                return Json(new { success = false, message = "Signup process failed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("OptVerification")]
        public async Task<JsonResult> OptVerification([FromBody] SignupTokenValidate payload)
        {
            try
            {
                var res = await _service.OptVerification(payload);

                if (res != null)
                {
                    return Json(new { success = true, data = res });
                }

                return Json(new { success = false, message = "Signup process failed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AllowAnonymous]

        [HttpPost("resendOtp")]
        public async Task<JsonResult> ResendOtp([FromBody] ResendOtp payload)
        {
            try
            {
                var res = await _service.ResendOtp(payload);

                if (res != null)
                {
                    return Json(new { success = true, data = res });
                }

                return Json(new { success = false, message = "Signup process failed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
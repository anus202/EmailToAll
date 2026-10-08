using EmailToAll.Payload;
using EmailToAll.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailToAll.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private readonly LoginService _service;

        public LoginController(LoginService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<JsonResult> Login([FromBody] LoginPayload data)
        {
            try
            {
                var res = await _service.Login(data);
                if (res == null)
                {
                    return Json(new { success = false, message = "Invalid email or password" });
                }
                else
                {
                    return Json(new { success = true, message = "Login successful", data = res });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("ForgetPassword")]
        public async Task<JsonResult> ForgetPassword([FromBody] ForgetPass data)
        {
            try
            {
                var res = await _service.ForgetPassword(data);
                if (res == null)
                {
                    return Json(new { success = false, data = res});
                }
                else
                {
                    return Json(new { success = true, data = res });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [AllowAnonymous]
        [HttpPost("LoginOptVerification")]
        public async Task<JsonResult> LoginOptVerification([FromBody] LoginTokenValidate data)
        {
            try
            {
                var res = await _service.OptVerification(data);
                if (res == null)
                {
                    return Json(new { success = false, message = "Invalid email or password" });
                }
                else
                {
                    return Json(new { success = true, data = res });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("PasswordReset")]
        public async Task<JsonResult> PasswordReset([FromBody] LoginPayload data)
        {
            try
            {
                var res = await _service.PasswordReset(data);
                if (res == null)
                {
                    return Json(new { success = false, message = "Invalid email or password" });
                }
                else
                {
                    return Json(new { success = true, data = res });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

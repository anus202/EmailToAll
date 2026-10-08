namespace EmailToAll.Payload
{
    public class LoginPayload
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class ForgetPass
    {
        public string Email { get; set; } = string.Empty;
    }

    public class LoginTokenValidate
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class PasswordResetPayload
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

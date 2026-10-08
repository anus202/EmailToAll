namespace EmailToAll.Payload
{
    public class SignupTokenValidate
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
    public class ResendOtp
    {
        public string Email { get; set; } = string.Empty;
    }
}

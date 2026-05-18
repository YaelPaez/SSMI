namespace SSMI.Services;

public class JwtOptions
{
    public string Issuer { get; set; } = "SSMI";
    public string Audience { get; set; } = "SSMI";
    public string Key { get; set; } = string.Empty;
    public string CookieName { get; set; } = "ssmi_auth";
    public int ExpirationMinutes { get; set; } = 120;
}

namespace Cyb_lab.Options;

public class PasswordPolicyOptions
{
	public const string SectionName = "PasswordPolicy";

	public int RequiredLength { get; set; }
	public int RequiredUniqueChars { get; set; }
	public bool RequireNonAlphanumeric { get; set; }
	public bool RequireLowercase { get; set; }
	public bool RequireUppercase { get; set; }
	public bool RequireDigit { get; set; }
	public TimeSpan ExpirationTime { get; set; }
}

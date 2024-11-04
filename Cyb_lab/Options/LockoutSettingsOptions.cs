namespace Cyb_lab.Options;

public class LockoutSettingsOptions
{
	public const string SectionName = "Lockout";

	public TimeSpan DefaultLockoutTimeSpan {get; set;}
	public int MaxFailedAccessAttempts { get; set; }
	public bool AllowedForNewUsers {get; set;}
	public TimeSpan InactivityTime { get; set;}
}

using Cyb_lab.Data;

namespace Cyb_lab.Models;

public class PasswordHistoryEntry
{
	public string PasswordId { get; set; }
	public string UserId { get; set; }
	public string PasswordHash { get; set; }
	public DateTime DateChanged { get; set; }

	public ApplicationUser User { get; set; }

	public PasswordHistoryEntry()
	{
		PasswordId = Guid.NewGuid().ToString();
	}
}

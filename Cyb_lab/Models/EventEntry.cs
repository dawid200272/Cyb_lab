using Cyb_lab.Data;

namespace Cyb_lab.Models;

public class EventEntry
{
	public string Id { get; set; }

	public string? UserId { get; set; }
	public ApplicationUser? User { get; set; }

	public DateTime Date { get; set; }
	public string Action { get; set; }
	public string Description { get; set; }

	public EventEntry()
	{
		Id = Guid.NewGuid().ToString();
		Date = DateTime.UtcNow;
	}
}

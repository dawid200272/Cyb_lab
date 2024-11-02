using Cyb_lab.Data;

namespace Cyb_lab.Models;

public class EventEntryViewModel
{
	public string User { get; set; }
	public DateTime Date { get; set; }
	public string Action { get; set; }
	public string Description { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace Cyb_lab.ViewModels;

public class SimpleUserViewModel
{
	[Required]
	public string Id { get; set; }

	[Required]
	public string Name { get; set; }
	
	[Required]
	public IList<string> Roles { get; set; } = [];
}

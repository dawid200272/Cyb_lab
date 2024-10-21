using System.ComponentModel.DataAnnotations;

namespace Cyb_lab.ViewModels;

public class SimpleUserViewModel
{
	public string Id { get; set; }

	[Required]
	public string Name { get; set; }
}

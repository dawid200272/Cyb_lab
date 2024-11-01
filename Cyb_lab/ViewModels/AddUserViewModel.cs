using System.ComponentModel.DataAnnotations;

namespace Cyb_lab.ViewModels;

public class AddUserViewModel
{
	[Required]
	[Display(Name = "User name")]
	public string UserName { get; set;}

	[Required]
	[DataType(DataType.Password)]
	public string Password { get; set; }

	[DataType(DataType.Password)]
	[Display(Name = "Confirm password")]
	[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
	public string ConfirmPassword { get; set; }

	[Required]
	[Display(Name = "Enable onetime password")]
	public bool OnetimePasswordEnabled { get; set; }
}

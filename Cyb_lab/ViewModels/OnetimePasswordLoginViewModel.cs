using System.ComponentModel.DataAnnotations;

namespace Cyb_lab.ViewModels;

public class OnetimePasswordLoginViewModel
{
	public string? Id { get; set; }

	[Required]
	[Display(Name = "User Name")]
	public string UserName { get; set; }

	[Display(Name = "Onetime Password")]
	public double? OnetimePassword {  get; set; }

	public string? OnetimePasswordFunction { get; set; }
	public int? A { get; set; }
	public int? X {  get; set; }

	// TODO: Delete when finished
	public double? FunctionValue { get; set; }
}

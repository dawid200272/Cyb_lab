using Microsoft.AspNetCore.Identity;

namespace Cyb_lab.Data;

public class ApplicationUser : IdentityUser
{
	public bool FirstLogin { get; set; } = true;

	public ApplicationUser() : base() { }

	public ApplicationUser(string userName) : base(userName)
	{
	}
}

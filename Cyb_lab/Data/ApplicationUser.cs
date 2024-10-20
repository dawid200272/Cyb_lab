using Microsoft.AspNetCore.Identity;

namespace Cyb_lab.Data;

public class ApplicationUser : IdentityUser
{
	public DateTime LastPasswordChangeDate { get; set; }
	public bool FirstLogin { get; set; } = true;
	public bool Disabled { get; set; } = false;

    public ApplicationUser() : base() { }

	public ApplicationUser(string userName) : base(userName)
	{
	}
}

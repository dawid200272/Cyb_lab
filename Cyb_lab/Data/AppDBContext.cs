using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cyb_lab.Data;

public class AppDBContext : IdentityDbContext<ApplicationUser>
{
	public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }
}

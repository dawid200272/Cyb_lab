using Cyb_lab.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cyb_lab.Data;

public class AppDBContext : IdentityDbContext<ApplicationUser>
{
	public DbSet<PasswordHistoryEntry> PasswordHistory { get; set; }
	public DbSet<EventEntry> EventLogs { get; set; }

	public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.Entity<PasswordHistoryEntry>()
			.HasKey(p => new { p.PasswordId, p.UserId });

		base.OnModelCreating(builder);
	}
}

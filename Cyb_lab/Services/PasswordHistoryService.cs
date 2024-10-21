using Cyb_lab.Data;
using Cyb_lab.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cyb_lab.Services;

public class PasswordHistoryService
{
	private readonly AppDBContext _dbContext;
	private readonly DbSet<PasswordHistoryEntry> _history;
	private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

	public PasswordHistoryService(AppDBContext dbContext,
		IPasswordHasher<ApplicationUser> passwordHasher)
	{
		_dbContext = dbContext;
		_history = _dbContext.Set<PasswordHistoryEntry>();

		_passwordHasher = passwordHasher;
	}

	public void AddEntry(PasswordHistoryEntry entry)
	{
		_history.Add(entry);
		_dbContext.SaveChanges();
	}

	public bool IsPasswordUnique(ApplicationUser user, string password, int maxLastPasswordCheck = 0)
	{
		var usedPasswords = _history
			.Where(p => p.User == user)
			.OrderByDescending(p => p.DateChanged);

		if (maxLastPasswordCheck != 0)
		{
			usedPasswords = (IOrderedQueryable<PasswordHistoryEntry>)usedPasswords
				.Take(maxLastPasswordCheck);
		}

		foreach (var usedPassword in usedPasswords)
		{
			var comparisonResult = _passwordHasher.VerifyHashedPassword(user, usedPassword.PasswordHash, password);

			if (comparisonResult == PasswordVerificationResult.Success)
			{
				return false;
			}
		}

		return true;
	}
}

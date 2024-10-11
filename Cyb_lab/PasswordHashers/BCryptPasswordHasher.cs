using Microsoft.AspNetCore.Identity;

namespace Cyb_lab.PasswordHashers;

public class BCryptPasswordHasher<TUser> : IPasswordHasher<TUser>
	where TUser : IdentityUser
{
	private const int WorkFactor = 13;

	public string HashPassword(TUser user, string password)
	{
		return BC.EnhancedHashPassword(password, WorkFactor);
	}

	public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
	{
		if (BC.EnhancedVerify(providedPassword, hashedPassword))
		{
			return PasswordVerificationResult.Success;
		}

		return PasswordVerificationResult.Failed;
	}
}

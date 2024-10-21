using Cyb_lab.Data;
using Cyb_lab.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cyb_lab.Controllers;

[Authorize]
public class AccountController : Controller
{
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;

	public AccountController(SignInManager<ApplicationUser> signInManager,
		UserManager<ApplicationUser> userManager)
	{
		_signInManager = signInManager;
		_userManager = userManager;
	}

	[HttpGet]
	[AllowAnonymous]
	public IActionResult Index()
	{
		if (_signInManager.IsSignedIn(User))
		{
			return RedirectToAction(nameof(HomeController.Index), "Home", new { isLogedIn = true });
		}

		//return View();
		return RedirectToAction(nameof(AccountController.Login), "Account");
	}

	public IActionResult ToggleLock(string id)
	{
        var user = _userManager.Users.First(x => x.Id == id);
		_userManager.SetLockoutEnabledAsync(user, !user.LockoutEnabled);
		_userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

		//user.LockoutEnabled = !user.LockoutEnabled;
		//user.LockoutEnd = DateTimeOffset.MaxValue;

        return RedirectToAction(nameof(AdminController.UserDetails), "Admin" , new { id });
	}

    [HttpGet]
	[AllowAnonymous]
	public IActionResult Login()
	{
		return View();
	}

	[HttpPost]
	[AllowAnonymous]
	public async Task<IActionResult> Login(LoginViewModel viewModel)
	{
		if (!ModelState.IsValid)
		{
			return View(viewModel);
		}

		var result = await _signInManager.PasswordSignInAsync(viewModel.UserName, viewModel.Password, isPersistent: false, lockoutOnFailure: false);

		if (result.Succeeded)
		{
			var user = await _userManager.GetUserAsync(User);

    //        if (user.Disabled)
    //        {
    //            ModelState.AddModelError(string.Empty, "Account is disabled");
				//return View(viewModel);
    //        }

            if (user!.FirstLogin)
			{
				return RedirectToAction(nameof(AccountController.ChangePassword), "Account");
			}

			// add date to compare, it'll be in password policy somewhere
			if (user.LastPasswordChangeDate >= DateTime.UtcNow) 
            {
                return RedirectToAction(nameof(AccountController.ChangePassword), "Account");
            }

			return RedirectToAction(nameof(HomeController.Index), "Home");
		}
		if (result.IsLockedOut)
		{
            ModelState.AddModelError(string.Empty, "Account is disabled");
            return View(viewModel);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
		return View(viewModel);
	}

	[HttpPost]
	public async Task<IActionResult> Logout()
	{
		await _signInManager.SignOutAsync();
		return RedirectToAction(nameof(HomeController.Index), "Home");
	}

	[HttpGet]
	public IActionResult ChangePassword()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
	{
		if (!ModelState.IsValid)
		{
			return View(viewModel);
		}

		var user = await _userManager.GetUserAsync(User);

		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		var changePasswordResult = await _userManager.ChangePasswordAsync(user, viewModel.OldPassword, viewModel.NewPassword);

		if (!changePasswordResult.Succeeded)
		{
			foreach (var error in changePasswordResult.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View(viewModel);
		}

		user.LastPasswordChangeDate = DateTime.UtcNow;

		await _signInManager.RefreshSignInAsync(user);

		// TODO: add status message

		if (user.FirstLogin)
		{
			user.FirstLogin = false;

			var result = await _userManager.UpdateAsync(user);
		}

		return RedirectToAction(nameof(HomeController.Index), "Home");
	}
}

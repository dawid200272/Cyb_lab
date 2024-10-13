using Cyb_lab.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cyb_lab.Controllers;

[Authorize]
public class AccountController : Controller
{
	private readonly SignInManager<IdentityUser> _signInManager;
	private readonly UserManager<IdentityUser> _userManager;

	public AccountController(SignInManager<IdentityUser> signInManager,
		UserManager<IdentityUser> userManager)
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
			return RedirectToAction(nameof(HomeController.Index), "Home");
		}
		if (result.IsLockedOut)
		{
			// Handle lockout scenario
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

		await _signInManager.RefreshSignInAsync(user);

		// TODO: add status message

		return RedirectToAction(nameof(HomeController.Index), "Home");
	}
}

using Cyb_lab.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cyb_lab.Controllers;

[Authorize]
public class AccountController : Controller
{
	private readonly SignInManager<IdentityUser> _signInManager;

	public AccountController(SignInManager<IdentityUser> signInManager)
	{
		_signInManager = signInManager;
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

	public async Task<IActionResult> Logout()
	{
		await _signInManager.SignOutAsync();
		return RedirectToAction(nameof(HomeController.Index), "Home");
	}

	// change password
}

using Cyb_lab.Data;
using Cyb_lab.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Cyb_lab.Controllers;
public class HomeController : Controller
{
	private readonly ILogger<HomeController> _logger;
	private readonly UserManager<ApplicationUser> _userManager;

	public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
	{
		_logger = logger;
		_userManager = userManager;
	}

	public IActionResult Index(bool isLogedIn = false)
	{
		if (isLogedIn)
		{
			return View();
		}

		return RedirectToAction(nameof(AccountController.Index), "Account");
	}

	public async Task<IActionResult> ShowFiles()
	{
		var user = await _userManager.GetUserAsync(User);

		if (user.LicenseActivated || !EndOfTheMonth())
		{
			return View();
		}
		else
		{
			return RedirectToAction(nameof(AccountController.ActivateLicense), "Account");
		}
	}

	private bool EndOfTheMonth()
	{
		var currentDate = DateTime.Now;

		if (currentDate.Day == DateTime.DaysInMonth(currentDate.Year, currentDate.Month))
		{
			return true;
		}
		return false;
	}

	public IActionResult Privacy()
	{
		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}
}

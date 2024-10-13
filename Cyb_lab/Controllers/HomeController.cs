using Cyb_lab.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Cyb_lab.Controllers;
public class HomeController : Controller
{
	private readonly ILogger<HomeController> _logger;

	public HomeController(ILogger<HomeController> logger)
	{
		_logger = logger;
	}

	public IActionResult Index(bool isLogedIn = false)
	{
		if (isLogedIn)
		{
			return View();
		}

		return RedirectToAction(nameof(AccountController.Index), "Account");
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

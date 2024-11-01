using Cyb_lab.Data;
using Cyb_lab.Models;
using Cyb_lab.Options;
using Cyb_lab.Services;
using Cyb_lab.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Cyb_lab.Controllers;

[Authorize]
public class AccountController : Controller
{
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly PasswordHistoryService _passwordHistoryService;
	private readonly EventLogsService _eventLogsService;
	private readonly IOptionsMonitor<PasswordPolicyOptions> _passwordOptionsMonitor;

	private const string _onetimePasswordFunctionString = "lg(a/x)";
	private Func<int, int, double> _onetimePasswordFunction =
		(int a, int x) =>
	{
		return Math.Log10((double)a / x);
	};
	private Random _random;

	public AccountController(SignInManager<ApplicationUser> signInManager,
		UserManager<ApplicationUser> userManager,
		PasswordHistoryService passwordHistoryService,
		IOptionsMonitor<PasswordPolicyOptions> passwordOptionsMonitor,
		EventLogsService eventLogsService)
	{
		_signInManager = signInManager;
		_userManager = userManager;
		_passwordHistoryService = passwordHistoryService;
		_passwordOptionsMonitor = passwordOptionsMonitor;
		_eventLogsService = eventLogsService;

		var time = DateTime.UtcNow.Second;
		_random = new Random(time);
	}

	[HttpGet]
	[AllowAnonymous]
	public IActionResult Index()
	{
		if (_signInManager.IsSignedIn(User))
		{
			return RedirectToAction(nameof(HomeController.Index), "Home", new { isLogedIn = true });
		}

		return RedirectToAction(nameof(AccountController.Login), "Account");
	}

	public IActionResult ToggleLock(string id)
	{
		var user = _userManager.Users.First(x => x.Id == id);
		user.Disabled = !user.Disabled;

		_userManager.UpdateAsync(user);

		var toggleLockEvent = new EventEntry()
		{
			UserId = null, // TODO: Add admin id here
			User = null, // TODO: Add ref to admin here
			Date = DateTime.UtcNow,
			Action = nameof(ToggleLock),
			Description = $"{nameof(user.Disabled)} property of user '{user.UserName}' has been set to '{user.Disabled}'",
		};

		_eventLogsService.AddEntry(toggleLockEvent);

		return RedirectToAction(nameof(AdminController.UserDetails), "Admin", new { id });
	}

	public async Task<IActionResult> ToggleOnetimePassword(string id)
	{
		var user = await _userManager.FindByIdAsync(id);

		if (user is null)
		{
			ModelState.AddModelError(string.Empty, $"No user with {id} was found");
			return View(nameof(AdminController.UserDetails));
		}

		user.OnetimePasswordEnabled = !user.OnetimePasswordEnabled;

		await _userManager.UpdateAsync(user);

		var toggleOnetimePasswordEvent = new EventEntry()
		{
			UserId = null, // TODO: Add admin id here
			User =  null, // TODO: Add ref to admin here
			Date = DateTime.UtcNow,
			Action = nameof(ToggleOnetimePassword),
			Description = $"{nameof(user.OnetimePasswordEnabled)} property of user '{user.UserName}' has been set to '{user.OnetimePasswordEnabled}'",
		};

		_eventLogsService.AddEntry(toggleOnetimePasswordEvent);

		return RedirectToAction(nameof(AdminController.UserDetails), "Admin", new { id });
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

		var user = await _userManager.FindByNameAsync(viewModel.UserName);

		var loginEvent = new EventEntry()
		{
			UserId = user?.Id,
			User = user,
			Date = DateTime.UtcNow,
			Action = nameof(Login),
		};

		var LoginSucceededMessage = "Login succeeded";
		var loginFailedMessage = "Login failed";

		if (user is null)
		{
			var notFoundMessage = "No user found";

			ModelState.AddModelError(string.Empty, notFoundMessage);

			loginEvent.Description = $"{loginFailedMessage} : {notFoundMessage}";
			_eventLogsService.AddEntry(loginEvent);

			return View(viewModel);
		}

		if (user.Disabled)
		{
			var disabledMessage = "Account is disabled";

			ModelState.AddModelError(string.Empty, disabledMessage);

			loginEvent.Description = $"{loginFailedMessage} : {disabledMessage}";
			_eventLogsService.AddEntry(loginEvent);

			return View(viewModel);
		}

		if (user.OnetimePasswordEnabled)
		{
			return RedirectToAction(nameof(OnetimePasswordLogin), new { userId = user.Id});
		}

		if (string.IsNullOrWhiteSpace(viewModel.Password))
		{
			var passwordRequiredMessage = "Password required";

			ModelState.AddModelError(string.Empty, passwordRequiredMessage);

			loginEvent.Description = $"{loginFailedMessage} : {passwordRequiredMessage}";
			_eventLogsService.AddEntry(loginEvent);

			return View(viewModel);
		}

		var result = await _signInManager.PasswordSignInAsync(viewModel.UserName, viewModel.Password, isPersistent: false, lockoutOnFailure: true);

		if (result.Succeeded)
		{
			if (user!.FirstLogin)
			{
				var firstLoginMessage = $"First login of user '{user.UserName}'";

				loginEvent.Description = $"{LoginSucceededMessage} : {firstLoginMessage}";
				_eventLogsService.AddEntry(loginEvent);

				return RedirectToAction(nameof(AccountController.ChangePassword), "Account");
			}

			var passwordExpirationTime = _passwordOptionsMonitor.CurrentValue.ExpirationTime;

			if (DateTime.UtcNow - user.LastPasswordChangeDate >= passwordExpirationTime)
			{
				var passwordExpiredMessage = "Password expired";

				// TODO: change that to sth more reasonable
				loginEvent.Description = $"{LoginSucceededMessage} : {passwordExpiredMessage}";
				_eventLogsService.AddEntry(loginEvent);

				return RedirectToAction(nameof(ChangePassword), "Account");
			}

			loginEvent.Description = LoginSucceededMessage;
			_eventLogsService.AddEntry(loginEvent);

			return RedirectToAction(nameof(HomeController.Index), "Home");
		}
		if (result.IsLockedOut)
		{
			var lockedMessage = "Account locked";

			ModelState.AddModelError(string.Empty, "Too many login attempts, try again later");

			loginEvent.Description = $"{loginFailedMessage} : {lockedMessage}";
			_eventLogsService.AddEntry(loginEvent);

			return View(viewModel);
		}

		var invalidLoginMessage = "Invalid login attempt";

		ModelState.AddModelError(string.Empty, invalidLoginMessage);

		loginEvent.Description = $"{loginFailedMessage} : {invalidLoginMessage}";
		_eventLogsService.AddEntry(loginEvent);

		return View(viewModel);
	}

	[HttpPost]
	public async Task<IActionResult> Logout()
	{
		var user = await _userManager.GetUserAsync(User);

		await _signInManager.SignOutAsync();

		var logoutEvent = new EventEntry()
		{
			UserId = user!.Id,
			User = user,
			Date = DateTime.UtcNow,
			Action = nameof(Logout),
			Description = $"User '{user.UserName}' logged out",
		};

		_eventLogsService.AddEntry(logoutEvent);

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

		if (!_passwordHistoryService.IsPasswordUnique(user, viewModel.NewPassword))
		{
			ModelState.AddModelError(string.Empty, "Old passsword use is not allowed");
			return View(viewModel);
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
		}

		var result = await _userManager.UpdateAsync(user);

		var passwordEntry = new PasswordHistoryEntry()
		{
			UserId = user.Id,
			User = user,
			DateChanged = DateTime.UtcNow,
			PasswordHash = user.PasswordHash!
		};

		_passwordHistoryService.AddEntry(passwordEntry);

		return RedirectToAction(nameof(HomeController.Index), "Home");
	}

	[HttpGet]
	[AllowAnonymous]
	public async Task<IActionResult> OnetimePasswordLogin(string userId)
	{
		var user = await _userManager.FindByIdAsync(userId);

		var viewModel = new OnetimePasswordLoginViewModel();

		if (user is null)
		{
			ModelState.AddModelError(string.Empty, "No user found");
			return View(viewModel);
		}

		viewModel.UserName = user.UserName!;

		await GenerateOnetimePassword(viewModel, user);

		return View(viewModel);
	}

	[HttpPost]
	[AllowAnonymous]
	public async Task<IActionResult> OnetimePasswordLogin(OnetimePasswordLoginViewModel viewModel)
	{
		var user = await _userManager.FindByNameAsync(viewModel.UserName);

		if (user is null)
		{
			ModelState.AddModelError(string.Empty, "No user found");
			return View(viewModel);
		}

		if (user.OnetimePasswordValue is null)
		{
			ModelState.AddModelError(string.Empty, "Click 'Login' button to generate new onetime password");

			await GenerateOnetimePassword(viewModel, user);

			return View(viewModel);
		}

		if (viewModel.OnetimePassword != user.OnetimePasswordValue)
		{
			await GenerateOnetimePassword(viewModel, user);

			return View(viewModel);
		}

		await _signInManager.SignInAsync(user, false);

		user.OnetimePasswordValue = null;
		await _userManager.UpdateAsync(user);

		return RedirectToAction(nameof(HomeController.Index), "Home");
	}

	private async Task GenerateOnetimePassword(OnetimePasswordLoginViewModel viewModel, ApplicationUser user)
	{
		int decimalPlaces = 2;

		int a = user.UserName!.Length;

		const int minInt = 1;
		const int maxInt = 100;

		int x = _random.Next(minInt, maxInt);

		var functionResult = _onetimePasswordFunction.Invoke(a, x);
		user.OnetimePasswordValue = Math.Round(functionResult, decimalPlaces);

		viewModel.OnetimePasswordFunction = _onetimePasswordFunctionString;
		viewModel.A = a;
		viewModel.X = x;

		// TODO: Delete when finished
		viewModel.FunctionValue = functionResult;

		await _userManager.UpdateAsync(user);
	}
}

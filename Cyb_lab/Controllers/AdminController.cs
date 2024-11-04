using Cyb_lab.Data;
using Cyb_lab.Helpers;
using Cyb_lab.Models;
using Cyb_lab.Options;
using Cyb_lab.Services;
using Cyb_lab.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Cyb_lab.Controllers;

[Authorize("Administrator")]
public class AdminController : Controller
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IOptionsMonitor<IdentityOptions> _identityOptionsMonitor;
	private readonly IOptionsMonitor<PasswordPolicyOptions> _passwordOptionsMonitor;
	private readonly EventLogsService _eventLogsService;
	private readonly IOptionsMonitor<LockoutSettingsOptions> _lockoutOptionsMonitor;

	public AdminController(UserManager<ApplicationUser> userManager,
		IOptionsMonitor<IdentityOptions> identityOptionsMonitor,
		IOptionsMonitor<PasswordPolicyOptions> passwordOptionsMonitor,
		EventLogsService eventLogsService,
		IOptionsMonitor<LockoutSettingsOptions> lockoutOptionsMonitor)
	{
		_userManager = userManager;
		_identityOptionsMonitor = identityOptionsMonitor;
		_passwordOptionsMonitor = passwordOptionsMonitor;
		_eventLogsService = eventLogsService;
		_lockoutOptionsMonitor = lockoutOptionsMonitor;
	}

	public IActionResult Panel()
	{
		return View();
	}

	public async Task<IActionResult> UserList()
	{
		var tempUserList = _userManager.Users.ToList();

		var userList = new List<SimpleUserViewModel>();
		foreach (var item in tempUserList)
		{
			var userRoles = await _userManager.GetRolesAsync(item);

			userList.Add(new SimpleUserViewModel()
			{
				Id = item.Id,
				Name = item.UserName,
				Roles = userRoles
			});
		}

		return View(userList);
	}

	public async Task<IActionResult> UserDetails(string id)
	{
		var user = _userManager.Users.First(x => x.Id == id);

		var userRoles = await _userManager.GetRolesAsync(user);

		var userVM = new UserDetailsViewModel()
		{
			Id = user.Id,
			Name = user.UserName,
			Lockout = user.Disabled,
			OnetimePasswordEnabled = user.OnetimePasswordEnabled,
			Roles = userRoles,
		};
		// get user by id
		return View(userVM);
	}

	public IActionResult PasswordPolicy()
	{
		return View();
	}

	// add a new user account (with role 'User')
	// change password policy options
	// browse list of user accounts
	// ...

	[HttpGet]
	public IActionResult AddUser()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> AddUser(AddUserViewModel viewModel)
	{
		if (!ModelState.IsValid)
		{
			return View(viewModel);
		}

		var addUserEvent = new EventEntry()
		{
			UserId = null, // TODO: Add admin id here
			User = null, // TODO: Add ref to admin here
			Date = DateTime.UtcNow,
			Action = nameof(AddUser),
		};

		var newUser = new ApplicationUser(viewModel.UserName);

		var result = await _userManager.CreateAsync(newUser, viewModel.Password);

		if (!result.Succeeded)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View(viewModel);
		}

		newUser.OnetimePasswordEnabled = viewModel.OnetimePasswordEnabled;

		await _userManager.AddToRoleAsync(newUser, UserRoles.User.ToString());

		addUserEvent.Description = $"User '{newUser.UserName}' has been added";

		_eventLogsService.AddEntry(addUserEvent);

		return RedirectToAction(nameof(UserList));
	}

	[HttpGet]
	public async Task<IActionResult> EditUser(string? Id)
	{
		if (Id is null)
		{
			return NotFound("No user id was provided");
		}

		var user = await _userManager.FindByIdAsync(Id);

		if (user is null)
		{
			return NotFound("User with given id cannot be found");
		}

		var viewModel = new SimpleUserViewModel()
		{
			Id = user.Id,
			Name = user.UserName!
		};

		return View(viewModel);
	}

	[HttpPost]
	public async Task<IActionResult> EditUser(SimpleUserViewModel viewModel)
	{
		if (!ModelState.IsValid)
		{
			return View(viewModel);
		}

		var editUserEvent = new EventEntry()
		{
			UserId = null, // TODO: Add admin id here
			User = null, // TODO: Add ref to admin here
			Date = DateTime.UtcNow,
			Action = nameof(EditUser),
		};

		var user = await _userManager.FindByIdAsync(viewModel.Id);

		if (user is null)
		{
			return NotFound($"Unable to load user with given ID.");
		}

		var result = await _userManager.SetUserNameAsync(user, viewModel.Name);

		if (!result.Succeeded)
		{
			return View(viewModel);
		}

		editUserEvent.Description = $"User '{user.UserName}' has been edited";

		_eventLogsService.AddEntry(editUserEvent);

		return RedirectToAction(nameof(UserList));
	}

	[HttpPost]
	public async Task<IActionResult> DeleteUser(string id)
	{
		var user = await _userManager.FindByIdAsync(id);

		var deleteUserEvent = new EventEntry()
		{
			UserId = null, // TODO: Add admin id here
			User = null, // TODO: Add ref to admin here
			Date = DateTime.UtcNow,
			Action = nameof(DeleteUser),
		};

		if (user is null)
		{
			return NotFound($"Unable to delete user with given ID.");
		}

		var result = await _userManager.DeleteAsync(user);

		if (!result.Succeeded)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View(nameof(UserList));
		}

		deleteUserEvent.Description = $"User '{user.UserName}' has been deleted";

		_eventLogsService.AddEntry(deleteUserEvent);

		return RedirectToAction(nameof(UserList));
	}

	[HttpGet]
	public async Task<IActionResult> ResetUserPassword(string id)
	{
		var user = await _userManager.FindByIdAsync(id);

		if (user is null)
		{
			return NotFound($"Unable to find user with given ID.");
		}

		var viewModel = new ResetPasswordViewModel()
		{
			UserId = user.Id,
		};

		return View(viewModel);
	}

	[HttpPost]
	public async Task<IActionResult> ResetUserPassword(ResetPasswordViewModel viewModel)
	{
		if (!ModelState.IsValid)
		{
			return View(viewModel);
		}

		var resetUserPasswordEvent = new EventEntry()
		{
			UserId = null, // TODO: Add admin id here
			User = null, // TODO: Add ref to admin here
			Date = DateTime.UtcNow,
			Action = nameof(ResetUserPassword),
		};

		var user = await _userManager.FindByIdAsync(viewModel.UserId);

		if (user is null)
		{
			return NotFound($"Unable to find user with given ID.");
		}

		var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

		var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, viewModel.NewPassword);

		if (!resetResult.Succeeded)
		{
			foreach (var error in resetResult.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View(viewModel);
		}

		resetUserPasswordEvent.Description = $"User '{user.UserName}'s' password has been reset";

		_eventLogsService.AddEntry(resetUserPasswordEvent);

		return RedirectToAction(nameof(UserDetails), new { id = viewModel.UserId });
	}

	[HttpGet]
	public IActionResult ChangePasswordPolicy()
	{
		var identityPasswordOptions = _identityOptionsMonitor.CurrentValue.Password;
		var passwordPolicyOptions = _passwordOptionsMonitor.CurrentValue;

		var viewModel = new PasswordPolicyOptions()
		{
			RequiredLength = passwordPolicyOptions.RequiredLength,
			RequiredUniqueChars = passwordPolicyOptions.RequiredUniqueChars,
			RequireNonAlphanumeric = passwordPolicyOptions.RequireNonAlphanumeric,
			RequireLowercase = passwordPolicyOptions.RequireLowercase,
			RequireUppercase = passwordPolicyOptions.RequireUppercase,
			RequireDigit = passwordPolicyOptions.RequireDigit,
			ExpirationTime = passwordPolicyOptions.ExpirationTime,
		};

		return View(viewModel);
	}

	[HttpPost]
	public IActionResult ChangePasswordPolicy(PasswordPolicyOptions viewModel)
	{
		if (!ModelState.IsValid)
		{
			return View(viewModel);
		}

		var changePasswordPolicyEvent = new EventEntry()
		{
			UserId = null, // TODO: Add admin id here
			User = null, // TODO: Add ref to admin here
			Date = DateTime.UtcNow,
			Action = nameof(ChangePasswordPolicy),
		};

		UpdatePasswordOptionsInJsonFile(viewModel);

		_userManager.Options.Password = new PasswordOptions()
		{
			RequiredLength = viewModel.RequiredLength,
			RequiredUniqueChars = viewModel.RequiredUniqueChars,
			RequireNonAlphanumeric = viewModel.RequireNonAlphanumeric,
			RequireLowercase = viewModel.RequireLowercase,
			RequireUppercase = viewModel.RequireUppercase,
			RequireDigit = viewModel.RequireDigit,
		};

		changePasswordPolicyEvent.Description = "Password policy has been changed";

		_eventLogsService.AddEntry(changePasswordPolicyEvent);

		return RedirectToAction(nameof(Panel));
	}

	private void UpdatePasswordOptionsInJsonFile(PasswordPolicyOptions newOptions)
	{
		var jsonObj = SettingsHelpers.GetDynamicJson();

		var key = PasswordPolicyOptions.SectionName;

		#region Setting Values in jsonObj
		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.RequiredLength)}", jsonObj, newOptions.RequiredLength);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.RequiredUniqueChars)}", jsonObj, newOptions.RequiredUniqueChars);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.RequireNonAlphanumeric)}", jsonObj, newOptions.RequireNonAlphanumeric);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.RequireLowercase)}", jsonObj, newOptions.RequireLowercase);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.RequireUppercase)}", jsonObj, newOptions.RequireUppercase);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.RequireDigit)}", jsonObj, newOptions.RequireDigit);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.ExpirationTime)}", jsonObj, newOptions.ExpirationTime); 
		#endregion

		SettingsHelpers.WriteInAppSettings(jsonObj);
	}

	private void UpdateLockoutOptionsInJsonFile(LockoutSettingsOptions newOptions)
	{
		var jsonObj = SettingsHelpers.GetDynamicJson();

		var key = LockoutSettingsOptions.SectionName;

		#region Setting Values in jsonObj
		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.DefaultLockoutTimeSpan)}", jsonObj, newOptions.DefaultLockoutTimeSpan);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.MaxFailedAccessAttempts)}", jsonObj, newOptions.MaxFailedAccessAttempts);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.AllowedForNewUsers)}", jsonObj, newOptions.AllowedForNewUsers);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.InactivityTime)}", jsonObj, newOptions.InactivityTime);
		#endregion

		SettingsHelpers.WriteInAppSettings(jsonObj);
	}

	[HttpGet]
	public IActionResult Logs()
	{
		var viewModel = new List<EventEntryViewModel>();

		var eventEntries = _eventLogsService.GetLogs();

		foreach (var entry in eventEntries)
		{
			viewModel.Add(new EventEntryViewModel()
			{
				User = entry.User?.UserName ?? "ADMIN",
				Date = entry.Date,
				Action = entry.Action,
				Description = entry.Description,
			});
		}

		return View(viewModel);
	}

	[HttpGet]
	public IActionResult ChangeAccountSettings()
	{
		var lockoutOptions = _lockoutOptionsMonitor.CurrentValue;

		var viewModel = new LockoutSettingsOptions()
		{
			DefaultLockoutTimeSpan = lockoutOptions.DefaultLockoutTimeSpan,
			MaxFailedAccessAttempts = lockoutOptions.MaxFailedAccessAttempts,
			AllowedForNewUsers = lockoutOptions.AllowedForNewUsers,
			InactivityTime = lockoutOptions.InactivityTime,
		};

		return View(viewModel);
	}

	[HttpPost]
	public IActionResult ChangeAccountSettings(LockoutSettingsOptions viewModel)
	{
		if (!ModelState.IsValid)
		{
			return View(viewModel);
		}

		var changeAccountSettingsEvent = new EventEntry()
		{
			UserId = null, // TODO: Add admin id here
			User = null, // TODO: Add ref to admin here
			Date = DateTime.UtcNow,
			Action = nameof(ChangeAccountSettings),
		};

		UpdateLockoutOptionsInJsonFile(viewModel);

		_userManager.Options.Lockout = new LockoutOptions()
		{
			DefaultLockoutTimeSpan = viewModel.DefaultLockoutTimeSpan,
			MaxFailedAccessAttempts = viewModel.MaxFailedAccessAttempts,
			AllowedForNewUsers = viewModel.AllowedForNewUsers,
		};

		changeAccountSettingsEvent.Description = "Account settings has been changed";

		_eventLogsService.AddEntry(changeAccountSettingsEvent);

		return RedirectToAction(nameof(Panel));
	}
}

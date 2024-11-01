using Cyb_lab.Data;
using Cyb_lab.Helpers;
using Cyb_lab.Options;
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

	public AdminController(UserManager<ApplicationUser> userManager,
		IOptionsMonitor<IdentityOptions> identityOptionsMonitor,
		IOptionsMonitor<PasswordPolicyOptions> passwordOptionsMonitor)
	{
		_userManager = userManager;
		_identityOptionsMonitor = identityOptionsMonitor;
		_passwordOptionsMonitor = passwordOptionsMonitor;
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

		return RedirectToAction(nameof(UserList));
	}

	[HttpPost]
	public async Task<IActionResult> DeleteUser(string id)
	{
		var user = await _userManager.FindByIdAsync(id);

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

		UpdateJsonFile(viewModel);

		_userManager.Options.Password = new PasswordOptions()
		{
			RequiredLength = viewModel.RequiredLength,
			RequiredUniqueChars = viewModel.RequiredUniqueChars,
			RequireNonAlphanumeric = viewModel.RequireNonAlphanumeric,
			RequireLowercase = viewModel.RequireLowercase,
			RequireUppercase = viewModel.RequireUppercase,
			RequireDigit = viewModel.RequireDigit,
		};

		return RedirectToAction(nameof(Panel));
	}

	private void UpdateJsonFile(PasswordPolicyOptions newOptions)
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

	public IActionResult Logs()
	{
		//get all logs from db
		return View();
	}
}

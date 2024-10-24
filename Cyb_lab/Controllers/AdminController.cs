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
			Roles = userRoles
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

		//await _userManager.UpdateNormalizedUserNameAsync(user);

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
			RequiredLength = identityPasswordOptions.RequiredLength,
			RequiredUniqueChars = identityPasswordOptions.RequiredUniqueChars,
			RequireNonAlphanumeric = identityPasswordOptions.RequireNonAlphanumeric,
			RequireLowercase = identityPasswordOptions.RequireLowercase,
			RequireUppercase = identityPasswordOptions.RequireUppercase,
			RequireDigit = identityPasswordOptions.RequireDigit,
			ExpirationTime = TimeSpan.Parse("00:15:00"), // TODO: change this to real value
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

		// TODO: Get current value for password policy options and store it as old version or sth

		var oldPassswordOptions = _passwordOptionsMonitor.CurrentValue;

		// TODO: Compare old version of password options with view model values
		// Modify current value for password policy options with values from view model

		//IDictionary<string, Type> propertyList = ComparePasswordOptions(oldPassswordOptions, viewModel);

		// TODO: Update .json file

		//UpdateJsonFile(propertyList);

		UpdateJsonFile(viewModel);

		// TODO: Set current value for password policy options as ...
		// TODO: 

		var passwordPolicyOptions = _passwordOptionsMonitor.CurrentValue;

		_userManager.Options.Password = new PasswordOptions()
		{
			RequiredLength = passwordPolicyOptions.RequiredLength,
			RequiredUniqueChars = passwordPolicyOptions.RequiredUniqueChars,
			RequireNonAlphanumeric = passwordPolicyOptions.RequireNonAlphanumeric,
			RequireLowercase = passwordPolicyOptions.RequireLowercase,
			RequireUppercase = passwordPolicyOptions.RequireUppercase,
			RequireDigit = passwordPolicyOptions.RequireDigit,
		};



		_identityOptionsMonitor.CurrentValue.Password.RequireNonAlphanumeric = viewModel.RequireNonAlphanumeric;
		_identityOptionsMonitor.CurrentValue.Password.RequireDigit = viewModel.RequireDigit;

		var identityPasswordOptions = _identityOptionsMonitor.CurrentValue.Password;

		_userManager.Options.Password = identityPasswordOptions;

		return RedirectToAction(nameof(Panel));
	}

	private void UpdateJsonFile(PasswordPolicyOptions newOptions)
	{
		var jsonObj = SettingsHelpers.GetDynamicJson();

		var key = PasswordPolicyOptions.SectionName;

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.RequiredLength)}", jsonObj, newOptions.RequiredLength);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.RequiredUniqueChars)}", jsonObj, newOptions.RequiredUniqueChars);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.RequireNonAlphanumeric)}", jsonObj, newOptions.RequireNonAlphanumeric);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.RequireLowercase)}", jsonObj, newOptions.RequireLowercase);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.RequireUppercase)}", jsonObj, newOptions.RequireUppercase);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.RequireDigit)}", jsonObj, newOptions.RequireDigit);

		SettingsHelpers.SetValueRecursively($"{key}:{nameof(newOptions.ExpirationTime)}", jsonObj, newOptions.ExpirationTime);

		SettingsHelpers.WriteInAppSettings(jsonObj);
	}

	//private static void UpdateJsonFile(IDictionary<string, Type> propertyList)
	//{
	//	var propertiesByType = propertyList.GroupBy(p => p.Value);

	//	var jsonObj = SettingsHelpers.GetDynamicJson();

	//	//     foreach (var group in propertiesByType)
	//	//     {
	//	//var groupPropertyList = group.ToList();

	//	//var propertiesToUpdate = groupPropertyList.Select(p => { p.Key,  })

	//	//SettingsHelpers.SetValueRangeRecursivelyInDynamicJson()
	//	//     }

	//	//SettingsHelpers.AddOrUpdateAppSettings
	//}

	//private IDictionary<string, Type> ComparePasswordOptions(PasswordPolicyOptions oldPassswordOptions, PasswordPolicyOptions viewModel)
	//{
	//	IDictionary<string, Type> propertyList = new Dictionary<string, Type>();

	//	if (oldPassswordOptions.RequiredLength != viewModel.RequiredLength)
	//	{
	//		propertyList.Add(nameof(viewModel.RequiredLength), typeof(int));
	//	}

	//	if (oldPassswordOptions.RequiredUniqueChars != viewModel.RequiredUniqueChars)
	//	{
	//		propertyList.Add(nameof(viewModel.RequiredUniqueChars), typeof(int));
	//	}

	//	if (oldPassswordOptions.RequireNonAlphanumeric != viewModel.RequireNonAlphanumeric)
	//	{
	//		propertyList.Add(nameof(viewModel.RequireNonAlphanumeric), typeof(bool));
	//	}

	//	if (oldPassswordOptions.RequireLowercase != viewModel.RequireLowercase)
	//	{
	//		propertyList.Add(nameof(viewModel.RequireLowercase), typeof(bool));
	//	}

	//	if (oldPassswordOptions.RequireUppercase != viewModel.RequireUppercase)
	//	{
	//		propertyList.Add(nameof(viewModel.RequireUppercase), typeof(bool));
	//	}

	//	if (oldPassswordOptions.RequireDigit != viewModel.RequireDigit)
	//	{
	//		propertyList.Add(nameof(viewModel.RequireDigit), typeof(bool));
	//	}

	//	if (oldPassswordOptions.ExpirationTime != viewModel.ExpirationTime)
	//	{
	//		propertyList.Add(nameof(viewModel.ExpirationTime), typeof(TimeSpan));
	//	}

	//	return propertyList;
	//}
}

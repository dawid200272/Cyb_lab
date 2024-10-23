using Cyb_lab.Data;
using Cyb_lab.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cyb_lab.Controllers;

[Authorize("Administrator")]
public class AdminController : Controller
{
	private readonly UserManager<ApplicationUser> _userManager;

	public AdminController(UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;
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
}

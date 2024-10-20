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

    public IActionResult UserList()
	{
		var tempUserList = _userManager.Users.ToList();

		var userList = new List<SimpleUserViewModel>();
		foreach (var item in tempUserList)
		{
			userList.Add(new SimpleUserViewModel()
			{
				Id = item.Id,
				Name = item.UserName
			});
        }

		return View(userList);
	}
    public IActionResult UserDetails(string id)
    {
		var user = _userManager.Users.First(x => x.Id == id);

		var userVM = new UserDetailsViewModel()
		{
			Id = user.Id,
			Name = user.UserName,
            Lockout = user.LockoutEnabled
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
			return View(viewModel);
		}

		await _userManager.AddToRoleAsync(newUser, UserRoles.User.ToString());

		return RedirectToAction(nameof(HomeController.Index), "Home");
	}
}

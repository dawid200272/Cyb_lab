using Cyb_lab.Data;
using Cyb_lab.Options;
using Cyb_lab.PasswordHashers;
using Cyb_lab.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddEnvironmentVariables();

var passwordPolicyOptionsConfig = builder.Configuration.GetSection(PasswordPolicyOptions.SectionName);

builder.Services.AddOptions<PasswordPolicyOptions>()
	.Bind(passwordPolicyOptionsConfig)
	.ValidateDataAnnotations()
	.ValidateOnStart();

var passwordPolicyOptions = passwordPolicyOptionsConfig.Get<PasswordPolicyOptions>();

// Add services to the container.
builder.Services.AddControllersWithViews();

#region DB Related
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddDbContext<AppDBContext>(options =>
	options.UseSqlite(connectionString));
#endregion

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
	{
		if (passwordPolicyOptions is not null)
		{
			options.Password = new PasswordOptions()
			{
				RequiredLength = passwordPolicyOptions.RequiredLength,
				RequiredUniqueChars = passwordPolicyOptions.RequiredUniqueChars,
				RequireNonAlphanumeric = passwordPolicyOptions.RequireNonAlphanumeric,
				RequireLowercase = passwordPolicyOptions.RequireLowercase,
				RequireUppercase = passwordPolicyOptions.RequireUppercase,
				RequireDigit = passwordPolicyOptions.RequireDigit,
			};
		}
	})
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<AppDBContext>();

builder.Services.AddTransient<IPasswordHasher<ApplicationUser>, BCryptPasswordHasher<ApplicationUser>>();

builder.Services.AddRazorPages();

#region Roles and Policies
builder.Services.AddAuthorizationBuilder()
	.AddPolicy(UserRoles.Administrator.ToString(), policy =>
		policy.RequireRole(UserRoles.Administrator.ToString()))
	.AddPolicy(UserRoles.User.ToString(), policy =>
	policy.RequireRole(UserRoles.User.ToString()));
#endregion

builder.Services.AddTransient<PasswordHistoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

CreateDbRolesIfNotExist(app);
CreateAdminAccountIfNotExist(app);

app.Run();

static async void CreateDbRolesIfNotExist(IHost host)
{
	using var scope = host.Services.CreateScope();

	var services = scope.ServiceProvider;

	var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

	var roleEnumValues = Enum.GetValues<UserRoles>();

	try
	{
		foreach (var role in roleEnumValues)
		{
			if (!await roleManager.RoleExistsAsync(role.ToString()))
			{
				await roleManager.CreateAsync(new IdentityRole(role.ToString()));
			}
		}
	}
	catch (Exception ex)
	{
		//var logger = services.GetRequiredService<ILogger>();
		//logger.LogError(ex, "An error occurred during creating roles in DB.");
	}
}

static async void CreateAdminAccountIfNotExist(IHost host)
{
	const string AdminAccountName = "ADMIN";
	const string DefaultAdminPass = "Admin1@";

	using var scope = host.Services.CreateScope();

	var services = scope.ServiceProvider;

	var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

	try
	{
		if (await userManager.FindByNameAsync(AdminAccountName) is null)
		{
			var adminUser = new ApplicationUser(AdminAccountName);

			var result = await userManager.CreateAsync(adminUser, DefaultAdminPass);

			try
			{
				await userManager.AddToRoleAsync(adminUser, UserRoles.Administrator.ToString());
			}
			catch (Exception ex)
			{
				//var logger = services.GetRequiredService<ILogger>();
				//logger.LogError(ex, "An error occurred during adding admin account to admin role.");
			}
		}
	}
	catch (Exception ex)
	{
		//var logger = services.GetRequiredService<ILogger>();
		//logger.LogError(ex, "An error occurred during creating admin account in DB.");
	}
}

public enum UserRoles
{
	Administrator,
	User
}

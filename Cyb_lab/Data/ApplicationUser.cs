using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Cyb_lab.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string UserCreatedUserName { get; set; }
    }
}

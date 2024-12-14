using Microsoft.AspNetCore.Identity;

namespace RoboticsManager.Lib.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add any additional user properties here
        public string FullName { get; set; } = Random.Shared.Next().ToString();
    }
}

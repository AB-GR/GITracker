
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GITracker.Web.Data
{
	public class AppDbContext : IdentityDbContext<ApplicationUser>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options)	: base(options)
		{
		}
	}

	public class ApplicationUser : IdentityUser
	{
		public Gender Gender { get; set; }
	}

	public enum Gender
	{
		Female,
		Male
	}
}

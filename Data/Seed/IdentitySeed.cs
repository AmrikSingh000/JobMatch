

using Microsoft.AspNetCore.Identity;

namespace JobMatch.Data.Seed
{
    public static class IdentitySeed
    {

        public static async Task EnsureSeedAsync(IServiceProvider sp)
        {
            var roles = new[] { "Administrator", "Recruiter", "Jobseeker" };
            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var r in roles)
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            var userMgr = sp.GetRequiredService<UserManager<IdentityUser>>();
            var adminEmail = "admin@jobmatch.local";
            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userMgr.CreateAsync(admin, "Admin!123");
                await userMgr.AddToRoleAsync(admin, "Administrator");
            }
        }
    }
}
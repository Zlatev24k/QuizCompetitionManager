using Microsoft.AspNetCore.Identity;

namespace QuizCompetitionManager.Data
{
    public static class SeedData
    {
        public const string AdminRole = "Admin";
        public const string TeamRole = "Team";

        public static async Task EnsureSeededAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Roles
            if (!await roleManager.RoleExistsAsync(AdminRole))
                await roleManager.CreateAsync(new IdentityRole(AdminRole));

            if (!await roleManager.RoleExistsAsync(TeamRole))
                await roleManager.CreateAsync(new IdentityRole(TeamRole));

            // Admin 
            var adminEmail = "admin@quiz.local";
            var adminPassword = "Admin!23456";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new Exception("Admin user creation failed: " + errors);
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
                await userManager.AddToRoleAsync(adminUser, AdminRole);
        }
    }
}

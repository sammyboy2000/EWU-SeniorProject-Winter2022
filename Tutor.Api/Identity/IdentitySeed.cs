﻿using Microsoft.AspNetCore.Identity;
using Tutor.Api.Models;

namespace Tutor.Api.Identity
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Admin User
            await SeedAdminUserAsync(userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            }
            if (!await roleManager.RoleExistsAsync(Roles.Grant))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Grant));
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<AppUser> userManager)
        {
            // Seed Admin User
            if (await userManager.FindByNameAsync("sshaw16@ewu.edu") == null)
            {
                AppUser user = new AppUser
                {
                    UserName = "sshaw16@ewu.edu",
                    Email = "sshaw16@ewu.edu",
                };

                IdentityResult result = userManager.CreateAsync(user, "P@ssw0rd123").Result;

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, Roles.Admin);
                    await userManager.AddToRoleAsync(user, Roles.Grant);
                }
            }
        }
    }
}
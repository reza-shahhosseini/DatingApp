using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using API.Entities;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager,RoleManager<AppRole> roleManager){
            if(await userManager.Users.AnyAsync()) return;

            var userData=await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            var users=JsonSerializer.Deserialize<List<AppUser>>(userData);
            if(users==null) return;

            var roles=new List<AppRole>{
                new AppRole{Name="Member"},
                new AppRole{Name="Moderator"},
                new AppRole{Name="Admin"},
            };
            foreach(var role in roles){
                await roleManager.CreateAsync(role);
            }


            foreach(var user in users){
                user.UserName = user.UserName.ToLower();
                await userManager.CreateAsync(user,"Pa$$w0rd");
                await userManager.AddToRoleAsync(user,"Member");
            }

            var adminUser = new AppUser{
                UserName="admin",
            };
            await userManager.CreateAsync(adminUser,"Pa$$w0rd");
            await userManager.AddToRolesAsync(adminUser,new[]{"Admin","Moderator"});
        }
    }
}
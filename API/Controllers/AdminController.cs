using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
namespace API.Controllers
{
    public class AdminController:BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        public AdminController(UserManager<AppUser> userManager){
            _userManager=userManager;
        }
        [HttpGet("users-with-roles")]
        [Authorize(Policy="RequireAdminRole")]
        public async Task<ActionResult> GetUsersWithRoles(){
            var users = await _userManager.Users
            .Include(u=>u.UserRoles)
            .ThenInclude(ur=>ur.Role)
            .OrderBy(u=>u.UserName)
            .Select(user=>new {
                user.Id,
                Username=user.UserName,
                Roles=user.UserRoles.Select(ur=>ur.Role.Name).ToList(),
            })
            .ToListAsync();
            return Ok(users);
        }

        [HttpGet("photos-to-moderate")]
        [Authorize(Policy="ModeratePhotoRole")]
        public IActionResult GetPhotosForModeration(){
            return Ok("Only admins and moderators can see this.");
        }

        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username,[FromQuery] string roles){
            var user = await _userManager.FindByNameAsync(username);
            if(user==null)return NotFound("Couldn't find user.");
            var selectedRoles = roles.Split(",").ToArray();
            var currentUserRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.AddToRolesAsync(user,selectedRoles.Except(currentUserRoles));
            if(!result.Succeeded) return BadRequest("Failed to add to roles.");
            result = await _userManager.RemoveFromRolesAsync(user,currentUserRoles.Except(selectedRoles));
            if(!result.Succeeded) return BadRequest("Failed to remove from roles.");

            return Ok(await _userManager.GetRolesAsync(user));

        }

        
    }
}
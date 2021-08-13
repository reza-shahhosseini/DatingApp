using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    public class AccountController:BaseApiController
    {
        
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        
        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager , ITokenService tokenService,IMapper mapper){
            _tokenService = tokenService;
            _mapper=mapper;
            _userManager=userManager;
            _signInManager=signInManager;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto){
            if(await UserExists(registerDto.Username)) return BadRequest("Username is already taken.");

            var user = _mapper.Map<AppUser>(registerDto);
            // using var hmac=new HMACSHA512();
            
            user.UserName=registerDto.Username.ToLower();
            // user.PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            // user.PasswordSalt=hmac.Key;
            

            // _context.Users.Add(user);
            // await _context.SaveChangesAsync();
            var result = await _userManager.CreateAsync(user,registerDto.Password);
            if(!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user,"Member");
            if(!roleResult.Succeeded)return BadRequest(roleResult.Errors);

            return new UserDto{
                Username=user.UserName,
                Token=await _tokenService.CreateToken(user),
                KnownAs=user.KnownAs,
                Gender=user.Gender,
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
            var user=await _userManager.Users
            .Include(user=>user.Photos)
            .SingleOrDefaultAsync(user=>user.UserName==loginDto.Username.ToLower());
            if(user==null) return Unauthorized("Invalid Username.");

            //using var hmac=new HMACSHA512(user.PasswordSalt);
            //var computedHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            // for(int i=0;i<computedHash.Length;i++){
            //     if(computedHash[i]!=user.PasswordHash[i]) 
            //         return Unauthorized("Invalid Password.");
            // }

            var result=await _signInManager.CheckPasswordSignInAsync(user,loginDto.Password,false);
            if(!result.Succeeded) return Unauthorized();

            return new UserDto{
                Username=user.UserName,
                Token=await _tokenService.CreateToken(user),
                PhotoUrl=user.Photos.FirstOrDefault(ph=>ph.IsMain)?.Url,
                KnownAs=user.KnownAs,
                Gender=user.Gender,
            };
        }

        private async Task<bool> UserExists(string username){
            return await _userManager.Users.AnyAsync(user=>user.UserName==username.ToLower());
        }
    }
}
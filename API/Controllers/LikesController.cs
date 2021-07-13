using System.Threading.Tasks;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using API.DTOs;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _userRepository=userRepository;
            _likesRepository=likesRepository;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username){
            var sourceUserId=User.GetUserId();
            var likedUser=await _userRepository.GetUserByUsernameAsync(username);
            var sourceUserWithLikes=await _likesRepository.GetUserWithLikes(sourceUserId);

            if(likedUser == null) return NotFound();
            if(sourceUserWithLikes.UserName==username) return BadRequest("You cannot like yourself.");

            var like=await _likesRepository.GetUserLike(sourceUserId,likedUser.Id);
            if(like!=null) return BadRequest("You already liked this user.");

            like=new Like{
                LikingUserId=sourceUserId,
                LikedUserId=likedUser.Id
            };

            sourceUserWithLikes.LikedUsers.Add(like);

            if(await _userRepository.SaveAllAsync()) return Ok();
            return BadRequest("Filed to like user.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams){
            likesParams.UserId=User.GetUserId();
            var users=await _likesRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);
            return Ok(users);
        }
    }
}
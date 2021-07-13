using API.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using API.Entities;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using API.Extensions;
using API.Helpers;

namespace API.Data
{
    public class LikesRepository:ILikesRepository
    {
        private readonly DataContext _context;

        public LikesRepository(DataContext context){
            _context=context;
        }

        public async Task<Like> GetUserLike(int likingUserId,int likedUserId){
            return await _context.Likes.FindAsync(likingUserId, likedUserId);
        }

        public async Task<AppUser> GetUserWithLikes(int userId){
            return await _context.Users.Include(user=>user.LikedUsers).FirstOrDefaultAsync(user=>user.Id==userId);
        }
        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams){
            var users=_context.Users.OrderBy(user=>user.UserName).AsQueryable();
            var likes=_context.Likes.AsQueryable();

            if(likesParams.Predicate == "liked"){
                likes=likes.Where(like=>like.LikingUserId==likesParams.UserId);
                users=likes.Select(like=>like.LikedUser);
            }

            if(likesParams.Predicate == "likedBy"){
                likes=likes.Where(like=>like.LikedUserId==likesParams.UserId);
                users=likes.Select(like=>like.LikingUser);
            }

            var likedUsers = users.Select(user=>new LikeDto{
                Id=user.Id,
                Username=user.UserName,
                KnownAs=user.KnownAs,
                Age=user.DateOfBirth.CalculateAge(),
                PhotoUrl=user.Photos.FirstOrDefault(photo=>photo.IsMain).Url,
                City=user.City
            });

            return await PagedList<LikeDto>.CreateAsync(likedUsers,likesParams.PageNumber,likesParams.PageSize);
        }
    }
}
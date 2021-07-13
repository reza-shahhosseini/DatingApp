using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using System.Collections.Generic;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<Like> GetUserLike(int likingUserId, int likedUserId);
        Task<AppUser> GetUserWithLikes(int userId);
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
    }
}
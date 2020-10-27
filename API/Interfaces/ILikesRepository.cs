using System.Collections.Generic;
using System.Threading.Tasks;
using API.Dtos;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int likedUserId);

        Task<AppUser> GetUserWithLikes(int userId);

        Task<PagedList<LikeDto>> GetUserLikes(LikesParameters likesParameters);
    }
}
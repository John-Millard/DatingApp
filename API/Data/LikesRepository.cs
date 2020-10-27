using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext dataContext;

        public LikesRepository(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await this.dataContext.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParameters likesParameters)
        {
            var users = this.dataContext.Users.OrderBy(user => user.UserName).AsQueryable();
            var likes = this.dataContext.Likes.AsQueryable();

            if (likesParameters.Predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == likesParameters.UserId);
                users = likes.Select(like => like.LikedUser);
            }
            
            if (likesParameters.Predicate == "LikedBy")
            {
                likes = likes.Where(like => like.LikedUserId == likesParameters.UserId);
                users = likes.Select(like => like.SourceUser);
            }

            var likedUsers = users.Select(user => new LikeDto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(photo => photo.IsMain).Url,
                City = user.City,
                Id = user.Id,
            });

            return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParameters.PageNumber, likesParameters.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await this.dataContext.Users
                .Include(user => user.LikedUsers)
                .FirstOrDefaultAsync(user => user.Id == userId);
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository userRepository;

        private readonly ILikesRepository likesRepository;

        public LikesController(
            IUserRepository userRepository,
            ILikesRepository likesRepository)
        {
            this.likesRepository = likesRepository;
            this.userRepository = userRepository;
        }

        [HttpPost("{userName}")]
        public async Task<ActionResult> AddLike(string userName)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await this.userRepository.GetUserByUserNameAsync(userName);
            var sourceUser = await this.likesRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == userName) return BadRequest("You can't like yourself.");

            var userLike = await this.likesRepository.GetUserLike(sourceUserId, likedUser.Id);

            if (userLike != null) return BadRequest("You already like this user.");

            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id,
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await this.userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user.");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery] LikesParameters likesParameters)
        {
            likesParameters.UserId = User.GetUserId();

           var users = await this.likesRepository.GetUserLikes(likesParameters); 

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

           return Ok(users);
        }
    }
}
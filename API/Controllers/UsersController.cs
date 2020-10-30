using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;

        private readonly IMapper mapper;

        private readonly IPhotoService photoService;

        public UsersController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IPhotoService photoService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParameters userParameters)
        {
            var userGender = await this.unitOfWork.UserRepository.GetUserGender(User.GetUserName());
            userParameters.CurrentUserName = User.GetUserName();

            if (string.IsNullOrEmpty(userParameters.Gender))
                userParameters.Gender = userGender == "male" ? "female" : "male";


            var users = await this.unitOfWork.UserRepository.GetMembersAsync(userParameters);

            this.Response.AddPaginationHeader(
                users.CurrentPage,
                users.PageSize,
                users.TotalCount,
                users.TotalPages);

            return Ok(users);
        }

        // api/users/john
        [HttpGet("{userName}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string userName)
        {
            var user = await this.unitOfWork.UserRepository.GetMemberAsync(userName);

            return user;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var userName = User.GetUserName();

            var user = await this.unitOfWork.UserRepository.GetUserByUserNameAsync(userName);

            this.mapper.Map(memberUpdateDto, user);

            this.unitOfWork.UserRepository.Update(user);

            if (await this.unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update user.");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var userName = User.GetUserName();

            var user = await this.unitOfWork.UserRepository.GetUserByUserNameAsync(userName);

            var result = await this.photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };

            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await this.unitOfWork.Complete())
            {
                var photoDto = this.mapper.Map<PhotoDto>(photo);
                return CreatedAtRoute("GetUser", new { userName = userName }, photoDto);
            }

            return BadRequest("Problem adding photo.");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var userName = User.GetUserName();
            var user = await this.unitOfWork.UserRepository.GetUserByUserNameAsync(userName);

            var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

            if (photo.IsMain) return BadRequest("This is already your main photo.");

            var currentMainPhoto = user.Photos.FirstOrDefault(photo => photo.IsMain);

            if (currentMainPhoto != null) currentMainPhoto.IsMain = false;

            photo.IsMain = true;

            if (await this.unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to set main photo.");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var userName = User.GetUserName();
            var user = await this.unitOfWork.UserRepository.GetUserByUserNameAsync(userName);

            var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You cannot delete your main photo.");

            if (photo.PublicId != null)
            {
                var result = await this.photoService.DeletePhotoAsync(photo.PublicId);

                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await this.unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to delete the photo.");
        }
    }
}
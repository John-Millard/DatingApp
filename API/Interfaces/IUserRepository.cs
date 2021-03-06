using System.Collections.Generic;
using System.Threading.Tasks;
using API.Dtos;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);

        Task<IEnumerable<AppUser>> GetUsersAsync();

        Task<AppUser> GetUserByIdAsync(int id);

        Task<AppUser> GetUserByUserNameAsync(string userName);

        Task<PagedList<MemberDto>> GetMembersAsync(UserParameters userParameters);

        Task<MemberDto> GetMemberAsync(string userName);

        Task<string> GetUserGender(string userName);
    }
}
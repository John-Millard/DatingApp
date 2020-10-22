using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext dataContext;

        private readonly IMapper mapper;

        public UserRepository(DataContext dataContext,
            IMapper mapper)
        {
            this.mapper = mapper;
            this.dataContext = dataContext;
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await this.dataContext.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUserName(string userName)
        {
            return await this.dataContext.Users
                .Include(user => user.Photos)
                .SingleOrDefaultAsync(user => user.UserName == userName);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await this.dataContext.Users
                .Include(user => user.Photos)
                .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await this.dataContext.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            this.dataContext.Entry(user).State = EntityState.Modified;
        }

        public async Task<MemberDto> GetMemberAsync(string userName)
        {
            return await this.dataContext.Users
                .Where(user => user.UserName == userName)
                .ProjectTo<MemberDto>(this.mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
            return await this.dataContext.Users
                .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}
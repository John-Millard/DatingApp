using System.Threading.Tasks;
using API.Interfaces;
using AutoMapper;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext dataContext;

        private readonly IMapper mapper;

        public UnitOfWork(DataContext dataContext, IMapper mapper)
        {
            this.mapper = mapper;
            this.dataContext = dataContext;
        }

        public IUserRepository UserRepository => new UserRepository(this.dataContext, this.mapper);

        public IMessageRepository MessageRepository => new MessageRepository(this.dataContext, this.mapper);

        public ILikesRepository LikesRepository => new LikesRepository(this.dataContext);

        public async Task<bool> Complete()
        {
            return await this.dataContext.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return this.dataContext.ChangeTracker.HasChanges();
        }
    }
}
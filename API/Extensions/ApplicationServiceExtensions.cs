using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<PresenceTracker>();
            serviceCollection.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            serviceCollection.AddScoped<ITokenService, TokenService>();
            serviceCollection.AddScoped<IPhotoService, PhotoService>();
            serviceCollection.AddScoped<ILikesRepository, LikesRepository>();
            serviceCollection.AddScoped<IMessageRepository, MessageRepository>();
            serviceCollection.AddScoped<LogUserActivity>();
            serviceCollection.AddScoped<IUserRepository, UserRepository>();
            serviceCollection.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            serviceCollection.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            });
            
            return serviceCollection;
        }
    }
}
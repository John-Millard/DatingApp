using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext dataContext;

        private readonly IMapper mapper;

        public MessageRepository(DataContext dataContext, IMapper mapper)
        {
            this.mapper = mapper;
            this.dataContext = dataContext;
        }

        public void AddGroup(Group group)
        {
            this.dataContext.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            this.dataContext.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            this.dataContext.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await this.dataContext.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await this.dataContext.Groups
                .Include(connection => connection.Connections)
                .Where(connections => connections.Connections.Any(x => x.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await this.dataContext.Messages
                .Include(message => message.SenderDeleted)
                .Include(message => message.Recipient)
                .SingleOrDefaultAsync(message => message.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await this.dataContext.Groups
                .Include(group => group.Connections)
                .FirstOrDefaultAsync(group => group.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParameters messageParameters)
        {
            var query = this.dataContext.Messages
                .OrderByDescending(message => message.MessageSent)
                .ProjectTo<MessageDto>(this.mapper.ConfigurationProvider)
                .AsQueryable();

            query = messageParameters.Container switch
            {
                "Inbox" => query.Where(message => message.RecipientUserName == messageParameters.UserName && message.RecipientDeleted == false),
                "Outbox" => query.Where(message => message.SenderUserName == messageParameters.UserName && message.SenderDeleted == false),
                _ => query.Where(message => message.RecipientUserName == messageParameters.UserName && message.DateRead == null && message.RecipientDeleted == false),
            };

            return await PagedList<MessageDto>.CreateAsync(query, messageParameters.PageNumber, messageParameters.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var messages = await this.dataContext.Messages
                .Where(message => (message.Recipient.UserName == currentUserName && message.Sender.UserName == recipientUserName && message.RecipientDeleted == false) ||
                    (message.Recipient.UserName == recipientUserName && message.Sender.UserName == currentUserName && message.SenderDeleted == false))
                .OrderBy(message => message.MessageSent)
                .ProjectTo<MessageDto>(this.mapper.ConfigurationProvider)
                .ToListAsync();
            
            var unreadMessages = messages.Where(message => message.DateRead == null && message.RecipientUserName == currentUserName)
                .ToList();
            
            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }

            return messages;
        }

        public void RemoveConnection(Connection connection)
        {
            this.dataContext.Connections.Remove(connection);
        }
    }
}
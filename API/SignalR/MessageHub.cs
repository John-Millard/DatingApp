using System;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository messageRepository;

        private readonly IMapper mapper;

        private readonly IUserRepository userRepository;

        private readonly IHubContext<PresenceHub> presenceHub;

        private readonly PresenceTracker presenceTracker;

        public MessageHub(IMessageRepository messageRepository, IMapper mapper, IUserRepository userRepository, IHubContext<PresenceHub> presenceHub, PresenceTracker presenceTracker)
        {
            this.presenceTracker = presenceTracker;
            this.presenceHub = presenceHub;
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.messageRepository = messageRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            var otherUserName = httpContext.Request.Query["user"].ToString();

            var groupName = GetGroupName(Context.User.GetUserName(), otherUserName);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await this.AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await this.messageRepository.GetMessageThread(Context.User.GetUserName(), otherUserName);

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await this.RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var userName = Context.User.GetUserName();

            if (userName == createMessageDto.RecipientUserName.ToLower())
                throw new HubException("You cannot send messages to yourself.");

            var sender = await this.userRepository.GetUserByUserNameAsync(userName);
            var recipient = await this.userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName);

            if (recipient == null)
                throw new HubException("Not found user.");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUserName = sender.UserName,
                RecipientUserName = recipient.UserName,
                Content = createMessageDto.Content,
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await this.messageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(connection => connection.UserName == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await this.presenceTracker.GetConnectionsForUser(recipient.UserName);

                if (connections != null)
                {
                    await this.presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new { userName = sender.UserName, knownAs = sender.KnownAs });
                }
            }

            this.messageRepository.AddMessage(message);

            if (await this.messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", this.mapper.Map<MessageDto>(message));
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await this.messageRepository.GetMessageGroup(groupName);

            var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());

            if (group == null)
            {
                group = new Group(groupName);
                this.messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await this.messageRepository.SaveAllAsync()) return group;

            throw new HubException("Failed to join group.");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await this.messageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            
            this.messageRepository.RemoveConnection(connection);
            if (await this.messageRepository.SaveAllAsync()) return group;

            throw new HubException("Failed to remove from group.");
        }

        private string GetGroupName(string callerUserName, string otherUserName)
        {
            var stringCompare = string.CompareOrdinal(callerUserName, otherUserName) < 0;

            return stringCompare ? $"{callerUserName}-{otherUserName}" : $"{otherUserName}-{callerUserName}";
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository userRepository;

        private readonly IMessageRepository messageRepository;

        private readonly IMapper mapper;

        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            this.mapper = mapper;
            this.messageRepository = messageRepository;
            this.userRepository = userRepository;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var userName = User.GetUserName();

            if (userName == createMessageDto.RecipientUserName.ToLower())
                return BadRequest("You cannot send messages to yourself.");

            var sender = await this.userRepository.GetUserByUserNameAsync(userName);
            var recipient = await this.userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUserName = sender.UserName,
                RecipientUserName = recipient.UserName,
                Content = createMessageDto.Content,
            };

            this.messageRepository.AddMessage(message);

            if (await this.messageRepository.SaveAllAsync()) return Ok(this.mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser(
            [FromQuery] MessageParameters messageParameters)
        {
            messageParameters.UserName = User.GetUserName();

            var messages = await this.messageRepository.GetMessagesForUser(messageParameters);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalCount);

            return messages;
        }

        [HttpGet("thread/{userName}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string userName)
        {
            var currentUserName = User.GetUserName();

            return Ok(await this.messageRepository.GetMessageThread(currentUserName, userName));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var userName = User.GetUserName();

            var message = await this.messageRepository.GetMessage(id);

            if (message.Sender.UserName != userName && message.Recipient.UserName != userName)
                return Unauthorized();

            if (message.Sender.UserName == userName)
                message.SenderDeleted = true;
            
            if (message.Recipient.UserName == userName)
                message.RecipientDeleted = true;
            
            if (message.SenderDeleted && message.RecipientDeleted)
                this.messageRepository.DeleteMessage(message);
            
            if (await messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message.");
        }
    }
}
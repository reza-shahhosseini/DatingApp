using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
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
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        public MessagesController(IUserRepository userRepository,IMessageRepository messageRepository,IMapper mapper)
        {
            _userRepository = userRepository;
            _messageRepository=messageRepository;
            _mapper=mapper;
        }

        public async Task<ActionResult> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();
            if (createMessageDto.RecipientUsername.ToLower() == username)
            {
                return BadRequest("You cannot send message to yourself.");
            }

            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            if(recipient==null) return NotFound();

            var sender=await _userRepository.GetUserByUsernameAsync(username);

            var message = new Message{
                Sender = sender,
                Recipient=recipient,
                SenderUsername= sender.UserName,
                RecipientUsername=recipient.UserName,
                Content=createMessageDto.Content,
            };
            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync()) 
                return Ok(_mapper.Map<MessageDto>(message));
            
            return BadRequest("Failed to send message.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams){
            messageParams.Username = User.GetUsername();
            var messages = await _messageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages.CurrentPage,messages.PageSize,messages.TotalCount,messages.TotalPages);
            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username){
            return Ok(await _messageRepository.GetMessageThread(User.GetUsername() , username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id){

            var message=await _messageRepository.GetMessage(id);

            var username=User.GetUsername();

            if(message.SenderUsername!=username && message.RecipientUsername!=username)
                return Unauthorized();

            if(message.SenderUsername==username) message.SenderDeleted=true;
            if(message.RecipientUsername==username) message.RecipientDeleted=true;

            if(message.SenderDeleted && message.RecipientDeleted) 
                _messageRepository.DeleteMessage(message);

            if(await _messageRepository.SaveAllAsync()) return Ok();
            else return BadRequest("Problem deleting the message.");
        }
    }
}
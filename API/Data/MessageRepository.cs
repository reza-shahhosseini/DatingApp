using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
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
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int messageId)
        {
            return await _context.Messages
            .Include(m=>m.Sender)
            .Include(m=>m.Recipient)
            .SingleOrDefaultAsync(m=>m.Id == messageId);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(m => m.Recipient.UserName == messageParams.Username && m.RecipientDeleted==false),
                "Outbox" => query.Where(m => m.Sender.UserName == messageParams.Username && m.SenderDeleted==false),
                _ => query.Where(m => m.Recipient.UserName == messageParams.Username && m.MessageRead == null && m.RecipientDeleted==false)
            };
            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
            return await PagedList<MessageDto>.CreateAsync(messages,messageParams.PageNumber,messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername,string recipientUsername)
        {
            var messages=await _context.Messages
            .Include(m=>m.Sender).ThenInclude(s=>s.Photos)
            .Include(m=>m.Recipient).ThenInclude(r=>r.Photos)
            .Where(
                m=>m.Recipient.UserName==currentUsername && 
                m.Sender.UserName == recipientUsername &&
                m.RecipientDeleted==false
                || 
                m.Sender.UserName==currentUsername && 
                m.Recipient.UserName==recipientUsername &&
                m.SenderDeleted==false
            )
            .ToListAsync();

            var unreadMessages=messages.Where(m=>m.MessageRead==null && m.Recipient.UserName==currentUsername).ToList();
            if(unreadMessages.Any()){
                foreach(var unreadMessage in unreadMessages){
                    unreadMessage.MessageRead=DateTime.Now;
                }
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
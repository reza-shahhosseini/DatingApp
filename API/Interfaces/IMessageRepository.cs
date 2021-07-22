using API.Entities;
using System.Threading.Tasks;
using API.Helpers;
using System.Collections.Generic;
using API.DTOs;
namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessage(int messageId);
        Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername,string recipientUsername);
        Task<bool> SaveAllAsync();
    }
}
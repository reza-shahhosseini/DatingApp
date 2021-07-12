using API.Interfaces;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using API.DTOs;
using System.Linq;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using API.Helpers;
using System;

namespace API.Data
{
    public class UserRepository:IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        
        public UserRepository(DataContext context,IMapper mapper){
            _context=context;
            _mapper=mapper;
        } 

        public async Task<AppUser> GetUserByIdAsync(int id){
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username){
            return await _context.Users.Include(user=>user.Photos).SingleOrDefaultAsync(user=>user.UserName==username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync(){
            return await _context.Users.Include(u=>u.Photos).ToListAsync();
        }

        public async Task<bool> SaveAllAsync(){
            return await _context.SaveChangesAsync()>0;
        }

        public void Update(AppUser user){
            _context.Entry(user).State=EntityState.Modified;
        }

        public async Task<MemberDto> GetMemberAsync(string username){
            return await _context.Users
            .Where(user=>user.UserName==username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams){
            var query = _context.Users.AsQueryable();
            query=query.Where(u=>u.UserName!=userParams.CurrentUsername);
            query=query.Where(u=>u.Gender==userParams.Gender);

            var minDob=DateTime.Today.AddYears(-userParams.MaxAge-1);
            var maxDob=DateTime.Today.AddYears(-userParams.MinAge);

            query=query.Where(u=>u.DateOfBirth>=minDob && u.DateOfBirth<=maxDob);

            query=userParams.OrderBy switch{
                "created"=>query.OrderByDescending(u=>u.Created),
                _ => query.OrderByDescending(u=>u.LastActive)
            };
                
            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking(), userParams.PageNumber,userParams.PageSize);
        }
    }
}
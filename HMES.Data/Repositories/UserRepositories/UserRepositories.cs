using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMES.Data.Enums;

namespace HMES.Data.Repositories.UserRepositories
{
    public class UserRepositories : GenericRepositories<User>, IUserRepositories
    {
        public UserRepositories(HmesContext context)
        : base(context)
        {
        }
        
        public async Task<User?> GetUserByEmail(string email){
            return await Context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> CheckUserByIdAndRole(Guid id, string role)
        {
            return await Context.Users.AnyAsync(u => u.Id == id && u.Role == role);
        }

        public async Task<User?> GetUserById(Guid id)
        {
            return await Context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<int> GetUserCount()
        {
            return await Context.Users.Where(u => u.Role.Equals(RoleEnums.Customer.ToString())).CountAsync();
        }

        public async Task<(List<User> Products, int TotalItems)> GetAllUsersAsync(string? keyword, Guid userId, string? role, string? status, int pageIndex, int pageSize)
        {
            var query = Context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t => t.Name.Contains(keyword));
            }

            if (userId != Guid.Empty)
            {
                query = query.Where(t => t.Id != userId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }
            
            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(t => t.Role == role);
            }
            
            query = query.Where(t => t.Role != "Admin");
            
            int totalItems = await query.CountAsync();
            var users = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (users, totalItems);
        }
    }
}

using System;
using System.Threading.Tasks;
using ImageOptimizer.Data;
using ImageOptimizer.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ImageOptimizer.Services
{
    public class UserManagementService : IDisposable
    {
        private bool _disposed;
        private readonly ApplicationDbContext _applicationDbContext;

        public UserManagementService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task CreateApiKeyAsync(ApplicationUser applicationUser)
        {
            var apiKey = new ApiKey
            {
                ApplicationUserId = applicationUser.Id,
                Key = Guid.NewGuid().ToString("N")
            };

            _applicationDbContext.ApiKeys.Add(apiKey);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task RemoveApiKeyAsync(int id)
        {
            var apiKey = await _applicationDbContext.ApiKeys.FirstOrDefaultAsync(x => x.Id == id);
            if (apiKey == null)
                return;

            _applicationDbContext.ApiKeys.Remove(apiKey);
            await _applicationDbContext.SaveChangesAsync();
        }
        
        public async Task<RolePermission> GetUserPermissionsByIdAsync(string applicationUserId)
        {
            var firstUserRole = await _applicationDbContext.UserRoles.FirstOrDefaultAsync(x => x.UserId == applicationUserId);
            return await _applicationDbContext.RolePermissions.FirstOrDefaultAsync(x => x.RoleId == firstUserRole.RoleId);
        }

        public async Task<string> GetUserRoleNameByIdAsync(string applicationUserId)
        {
            var firstUserRole = await _applicationDbContext.UserRoles.FirstOrDefaultAsync(x => x.UserId == applicationUserId);
            var role = await _applicationDbContext.Roles.FirstOrDefaultAsync(x => x.Id == firstUserRole.RoleId);
            return role.Name;
        }

        public async Task<UserMonthlyOptimization> GetUserMonthlyOptimizationsByIdAsync(string applicationUserId)
        {
            var userMonthlyOptimizedImages = await _applicationDbContext.UserMonthlyOptimizations
                .Where(date => date.Date.Year == DateTime.UtcNow.Year)
                .Where(date => date.Date.Month == DateTime.UtcNow.Month)
                .FirstOrDefaultAsync(x => x.ApplicationUserId == applicationUserId);

            if (userMonthlyOptimizedImages != null)
                return userMonthlyOptimizedImages;

            return new UserMonthlyOptimization
            {
                ApplicationUserId = applicationUserId,
                Date = DateTime.UtcNow,
                MonthlyOptimizedImages = 0
            };
        }

        public async Task<Address> GetUserAddressAsync(string applicationUserId)
        {
            return await _applicationDbContext.Addresses.FirstOrDefaultAsync(x => x.ApplicationUserId == applicationUserId);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _applicationDbContext?.Dispose();
             }

            _disposed = true;
        }
    }
}

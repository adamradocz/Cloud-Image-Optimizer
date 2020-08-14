using ImageOptimizer.Data;
using ImageOptimizer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageOptimizer.Services
{
    public class SubscriptionPlanManagementService : IDisposable
    {
        private bool _disposed;
        private readonly ApplicationDbContext _applicationDbContext;

        public SubscriptionPlanManagementService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<SubscriptionPlan>> GetSubscriptionPlansAsync()
        {
            return await _applicationDbContext.SubscriptionPlans.OrderBy(x => x.Price).ToListAsync();
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ImageOptimizer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ImageOptimizer.Models;
using Microsoft.AspNetCore.Hosting;

namespace ImageOptimizer.Data
{
    public class ApplicationDbContextSeedData : IDisposable
    {
        private bool _disposed;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserManagementService _userManagementService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ApplicationDbContextSeedData(
            ApplicationDbContext applicationDbContext,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            UserManagementService userManagementService,
            IHostingEnvironment hostingEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _userManagementService = userManagementService;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task EnsureSeedData()
        {
            if (await IsExistingDatabase())
                return;

            await EnsureCreateRoles();
            await EnsureCreateRolePermissions();
            await EnsureCreateAdmin();
            await EnsureCreateCountries();
            await EnsureCreateSubscriptionPlans();
        }

        private async Task<bool> IsExistingDatabase()
        {
            return await _roleManager.Roles.AnyAsync();
        }

        private async Task EnsureCreateRoles()
        {
            string[] roles = {"Admin", "Free", "Basic", "Professional", "Professional+" };            
            foreach (var role in roles)
            {
                var identityRole = new IdentityRole(role);
                await _roleManager.CreateAsync(identityRole);
            }
        }
        
        private async Task EnsureCreateRolePermissions()
        {
            // Admin
            var adminRole = await _roleManager.FindByNameAsync("Admin");
            var adminRolePermission = new RolePermission()
            {
                RoleId = adminRole.Id,
                AllowedFileTypes = "image/jpeg;image/png;image/gif;image/webp;image/vnd.ms-photo;image/jxr;image/apng",
                AllowedFileExtensions = ".jpg;.jpeg;.png;.gif;.webp;.jxr",
                AllowedImageSize = 33554432, // 32MB
                ImageLimitPerMonth = 10000,
                CanOptimizeLossy = true,
                CanConvertToWebp = true,
                CanConvertToJxr = true,
                CanConvertToApng = true,
                CanConvertToJpeg2000 = true,
                OptimizationLevel = (int)OptimizationLevel.Insane,
                CanUseMultisite = true
            };
            _applicationDbContext.RolePermissions.Add(adminRolePermission);

            // Free
            var freeRole = await _roleManager.FindByNameAsync("Free");
            var freeRolePermission = new RolePermission()
            {
                RoleId = freeRole.Id,
                AllowedFileTypes = "image/jpeg;image/png;image/gif",
                AllowedFileExtensions = ".jpg;.jpeg;.png;.gif",
                AllowedImageSize = 102400, // 100KB
                ImageLimitPerMonth = 100,
                CanOptimizeLossy = false,
                CanConvertToWebp = false,
                CanConvertToJxr = false,
                CanConvertToApng = false,
                CanConvertToJpeg2000 = false,
                OptimizationLevel = (int)OptimizationLevel.Low,
                CanUseMultisite = false
            };
            _applicationDbContext.RolePermissions.Add(freeRolePermission);
            
            // Basic
            var basicRole = await _roleManager.FindByNameAsync("Basic");
            var basicRolePermission = new RolePermission()
            {
                RoleId = basicRole.Id,
                AllowedFileTypes = "image/jpeg;image/png;image/gif",
                AllowedFileExtensions = ".jpg;.jpeg;.png;.gif",
                AllowedImageSize = 1048576, // 1MB
                ImageLimitPerMonth = 1000,
                CanOptimizeLossy = true,
                CanConvertToWebp = true,
                CanConvertToJxr = false,
                CanConvertToApng = false,
                CanConvertToJpeg2000 = false,
                OptimizationLevel = (int)OptimizationLevel.Normal,
                CanUseMultisite = false
            };
            _applicationDbContext.RolePermissions.Add(basicRolePermission);

            // Professional
            var professionalRole = await _roleManager.FindByNameAsync("Professional");
            var professionalRolePermission = new RolePermission()
            {
                RoleId = professionalRole.Id,
                AllowedFileTypes = "image/jpeg;image/png;image/gif;image/webp;image/vnd.ms-photo;image/jxr;image/apng",
                AllowedFileExtensions = ".jpg;.jpeg;.png;.gif;.webp;.jxr",
                AllowedImageSize = 5242880, // 5MB
                ImageLimitPerMonth = 1000,
                CanOptimizeLossy = true,
                CanConvertToWebp = true,
                CanConvertToJxr = true,
                CanConvertToApng = true,
                CanConvertToJpeg2000 = true,
                OptimizationLevel = (int)OptimizationLevel.High,
                CanUseMultisite = true
            };
            _applicationDbContext.RolePermissions.Add(professionalRolePermission);

            // Professional
            var professionalPlusRole = await _roleManager.FindByNameAsync("Professional+");
            var professionalPlusRolePermission = new RolePermission()
            {
                RoleId = professionalPlusRole.Id,
                AllowedFileTypes = "image/jpeg;image/png;image/gif;image/webp;image/vnd.ms-photo;image/jxr;image/apng",
                AllowedFileExtensions = ".jpg;.jpeg;.png;.gif;.webp;.jxr",
                AllowedImageSize = 33554432, // 32MB
                ImageLimitPerMonth = 5000,
                CanOptimizeLossy = true,
                CanConvertToWebp = true,
                CanConvertToJxr = true,
                CanConvertToApng = true,
                CanConvertToJpeg2000 = true,
                OptimizationLevel = (int)OptimizationLevel.Best,
                CanUseMultisite = true
            };
            _applicationDbContext.RolePermissions.Add(professionalPlusRolePermission);

            await _applicationDbContext.SaveChangesAsync();
        }

        private async Task EnsureCreateAdmin()
        {
            var user = new ApplicationUser { FirstName = "Admin", LastName = "Admin", UserName = "admin@admin.com", Email = "admin@admin.com" };
            var result = await _userManager.CreateAsync(user, "Pa$$w0rd");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                await _userManagementService.CreateApiKeyAsync(user);
            }
        }

        private async Task EnsureCreateCountries()
        {            
            var countiresFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Resources", "Countries.json");
            using (var reader = new StreamReader(countiresFilePath))
            {
                var countriesAsJson = await reader.ReadToEndAsync();
                var countries = JsonConvert.DeserializeObject<List<Country>>(countriesAsJson);

                foreach (var country in countries)
                {
                    _applicationDbContext.Countries.Add(country);
                }

                await _applicationDbContext.SaveChangesAsync();
            }
        }

        private async Task EnsureCreateSubscriptionPlans()
        {
            var basicRole = await _roleManager.FindByNameAsync("Basic");
            var basic = new SubscriptionPlan
            {
                Name = "Basic",
                Price = 9.9m,
                PaymentGatewaySubscriptionPlanId = "basic",
                RoleId = basicRole.Id
            };

            var professionalRole = await _roleManager.FindByNameAsync("Professional");
            var professional = new SubscriptionPlan
            {
                Name = "Professional",
                Price = 29.9m,
                PaymentGatewaySubscriptionPlanId = "professional",
                RoleId = professionalRole.Id
            };

            _applicationDbContext.SubscriptionPlans.Add(basic);
            _applicationDbContext.SubscriptionPlans.Add(professional);
            await _applicationDbContext.SaveChangesAsync();
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
                _userManager?.Dispose();
                _roleManager?.Dispose();
                _userManagementService?.Dispose();
            }

            _disposed = true;
        }
    }
}

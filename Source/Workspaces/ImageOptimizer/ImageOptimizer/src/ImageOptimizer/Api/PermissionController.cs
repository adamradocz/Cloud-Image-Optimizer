using System.Threading.Tasks;
using ImageOptimizer.Data;
using ImageOptimizer.Models;
using ImageOptimizer.Services;
using ImageOptimizer.Models.PermissionViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ImageOptimizer.Api
{
    [Route("api/[controller]")]
    public class PermissionController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserManagementService _userManagementService;

        public PermissionController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            UserManagementService userManagementService)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _userManagementService = userManagementService;
        }
        
        // GET: api/Permission
        [HttpGet]
        public async Task<IActionResult> GetPermissionsByApiKey([FromHeader] string apiKey)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Bad Request.", ModelState });
            }

            var userApiKey = await _applicationDbContext.ApiKeys.FirstOrDefaultAsync(x => x.Key == apiKey);
            if (userApiKey == null)
            {
                return BadRequest(new { Message = "Wrong API Key." });
            }
            
            var userPermissions = await _userManagementService.GetUserPermissionsByIdAsync(userApiKey.ApplicationUserId);
            var permissionViewModel = new GetPermissionsByApiKeyViewModel(userPermissions)
            {
                RoleName = await _userManagementService.GetUserRoleNameByIdAsync(userApiKey.ApplicationUserId),
                Message = "High five! :)"
            };

            return new ObjectResult(permissionViewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _applicationDbContext?.Dispose();
                _userManager?.Dispose();
                _userManagementService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
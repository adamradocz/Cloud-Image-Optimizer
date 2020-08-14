using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ImageOptimizer.Data;
using ImageOptimizer.Models;
using ImageOptimizer.Services;
using ImageOptimizer.Models.SubscriptionViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ImageOptimizer.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserManagementService _userManagementService;
        private readonly BraintreeManagementService _braintreeManagementService;
        private readonly SubscriptionPlanManagementService _subscriptionPlanManagementService;

        public SubscriptionController(
            ApplicationDbContext applicationDbContext,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            UserManagementService userManagementService,
            BraintreeManagementService braintreeManagementService,
            SubscriptionPlanManagementService subscriptionPlanManagementService)
        {
            _applicationDbContext = applicationDbContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _userManagementService = userManagementService;
            _braintreeManagementService = braintreeManagementService;
            _subscriptionPlanManagementService = subscriptionPlanManagementService;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
        
        public async Task<IActionResult> BillingInformation()
        {
            ViewData["ReturnUrl"] = Url.Action(nameof(PaymentInformation), nameof(SubscriptionController).Replace("Controller",""));

            var model = new BillingInformationViewModel();
            var user = await GetCurrentUserAsync();
            var address = await _userManagementService.GetUserAddressAsync(user.Id);
            if (address != null)
            {
                var country = await _applicationDbContext.Countries.FirstOrDefaultAsync(x => x.Id == address.CountryId);
                model.BillingAddress = address;
                model.Country = country.EnglishName;
            }
            return View(model);
        }
        
        public async Task<IActionResult> PaymentInformation()
        {
            var user = await GetCurrentUserAsync();
            var model = new PaymentInformationViewModel
            {
                ClientToken = _braintreeManagementService.GenerateClientToken(user.Id),
                SubscriptionPlans = await GetSubscriptionPlansInSelectList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubscription()
        {
            var formCollection = await HttpContext.Request.ReadFormAsync();
            var nonce = formCollection["payment_method_nonce"];
            var paymentGatewaySubscriptionPlanId = formCollection["PaymentGatewaySubscriptionPlanId"];
            var result = _braintreeManagementService.CreateSubscription(nonce, paymentGatewaySubscriptionPlanId);

            if (result.IsSuccess())
            {
                var user = await GetCurrentUserAsync();
                var subscriptionPlan = await GetSubscriptionPlanByPlanId(paymentGatewaySubscriptionPlanId);                
                var subscription = new Subscription
                {
                    PaymentGatewaySubscriptionId = result.Target.Id,
                    Status = "Active",
                    SubscriptionPlanId = subscriptionPlan.Id,
                    ApplicationUserId = user.Id,
                };

                await ModifyUserRoleAsync(user, subscriptionPlan);
                //await CreateNewInvoice(user, subscriptionPlan, result);

                _applicationDbContext.Subscriptions.Add(subscription);
                await _applicationDbContext.SaveChangesAsync();
            }

            return View(result);
        }
        
        public async Task<IActionResult> CancelSubscription()
        {
            var user = await GetCurrentUserAsync();
            var subscription = await _applicationDbContext.Subscriptions.FirstOrDefaultAsync(x => x.ApplicationUserId == user.Id);
            var result = _braintreeManagementService.CancelSubscription(subscription.PaymentGatewaySubscriptionId);
            string message;

            if (result.IsSuccess())
            {
                subscription.Status = "Canceled";
                _applicationDbContext.Subscriptions.Attach(subscription);
                await _applicationDbContext.SaveChangesAsync();
                await RemoveUserRolesAsync(user);
                
                // Add to Free role
                await _userManager.AddToRoleAsync(user, "Free");
                message = "Your subscription has been cancelled.";
            }
            else
            {
                message = "Something went wrong.";
            }

            return RedirectToAction(nameof(ManageController.Billing), nameof(ManageController).Replace("Controller", ""), new { Message = message } );
        }

        #region Helpers

        private async Task<IEnumerable<SelectListItem>> GetSubscriptionPlansInSelectList()
        {
            var subscriptionPlans = await _subscriptionPlanManagementService.GetSubscriptionPlansAsync();
            return subscriptionPlans.Select(x => new SelectListItem { Text = x.Name, Value = x.PaymentGatewaySubscriptionPlanId });
        }

        private async Task<IEnumerable<SelectListItem>> GetCountriesAsync()
        {
            var countries = await _applicationDbContext.Countries.OrderBy(x => x.EnglishName).ToListAsync();
            return countries.Select(c => new SelectListItem { Text = c.EnglishName, Value = c.Id.ToString() });
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(HttpContext.User);
        }

        private async Task<SubscriptionPlan> GetSubscriptionPlanByPlanId(string planId)
        {
            return await _applicationDbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.PaymentGatewaySubscriptionPlanId == planId);
        }

        private async Task ModifyUserRoleAsync(ApplicationUser user, SubscriptionPlan plan)
        {
            await RemoveUserRolesAsync(user);

            // Add to subscription role
            var role = await _roleManager.FindByIdAsync(plan.RoleId);
            await _userManager.AddToRoleAsync(user, role.Name);
        }

        private async Task RemoveUserRolesAsync(ApplicationUser user)
        {
            // Remove from roles except Admin
            var userRoles = await _userManager.GetRolesAsync(user);
            userRoles.Remove("Admin");
            await _userManager.RemoveFromRolesAsync(user, userRoles);
        }

        private async Task CreateNewInvoice(ApplicationUser user, SubscriptionPlan plan, Braintree.Result<Braintree.Subscription> result)
        {
            var order = new Invoice
            {
                ApplicationUserId = user.Id,
                Item = plan.Name,
                NetAmount = decimal.Round(plan.Price/1.27m, 2),
                VatAmount = decimal.Round(plan.Price/0.27m, 2),
                GrossAmount = plan.Price,
                TransactionId = result.Target.Id
            };
            _applicationDbContext.Invoices.Add(order);
            await _applicationDbContext.SaveChangesAsync();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _applicationDbContext?.Dispose();
                _userManager?.Dispose();
                _userManagementService?.Dispose();
                _subscriptionPlanManagementService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

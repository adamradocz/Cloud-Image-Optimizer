using Braintree;
using ImageOptimizer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace ImageOptimizer.Services
{
    public class BraintreeManagementService
    {
        private readonly BraintreeGateway _braintreeGateway;

        public BraintreeManagementService(IHostingEnvironment hostingEnvironment, IOptions<AppSettings> appSettings)
        {
            var braintreeEnvironment = hostingEnvironment.IsDevelopment() ? Environment.SANDBOX : Environment.PRODUCTION;
            _braintreeGateway = new BraintreeGateway
            {
                Environment = braintreeEnvironment,
                MerchantId = appSettings.Value.PaymentGateway.Braintree.MerchantId,
                PublicKey = appSettings.Value.PaymentGateway.Braintree.PublicKey,
                PrivateKey = appSettings.Value.PaymentGateway.Braintree.PrivateKey
            };
        }

        public string GenerateClientToken(string applicationUserId)
        {
            return _braintreeGateway.ClientToken.generate(
                new ClientTokenRequest
                {
                    CustomerId = applicationUserId
                });
        }

        public Result<Customer> CreateCustomer(ApplicationUser user, Models.Address address)
        {
            var request = new CustomerRequest
            {
                Id = user.Id,
                FirstName = address.FirstName,
                LastName = address.LastName,
                Company = address.CompanyName,
                Email = user.Email
            };
            return _braintreeGateway.Customer.Create(request);
        }

        public Result<Customer> UpdateCustomer(ApplicationUser user, Models.Address address)
        {
            var request = new CustomerRequest
            {
                FirstName = address.FirstName,
                LastName = address.LastName,
                Company = address.CompanyName,
                Email = user.Email
            };
            return _braintreeGateway.Customer.Update(user.Id, request);
        }

        public Result<Braintree.Subscription> CreateSubscription(string nonce, string planId)
        {
            var request = new SubscriptionRequest
            {
                PaymentMethodNonce = nonce,
                PlanId = planId
            };
            return _braintreeGateway.Subscription.Create(request);
        }
        
        public Result<Braintree.Subscription> CancelSubscription(string paymentGatewaySubscriptionId)
        {
            return _braintreeGateway.Subscription.Cancel(paymentGatewaySubscriptionId);
        }
    }
}

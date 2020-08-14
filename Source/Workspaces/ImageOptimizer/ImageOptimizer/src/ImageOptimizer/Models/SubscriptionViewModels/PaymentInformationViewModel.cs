using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ImageOptimizer.Models.SubscriptionViewModels
{
    public class PaymentInformationViewModel
    {
        [Required]
        [Display(Name = "Subscription plan")]
        public string PaymentGatewaySubscriptionPlanId { get; set; }
        public IEnumerable<SelectListItem> SubscriptionPlans { get; set; }
        public string ClientToken { get; set; }
    }
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ImageOptimizer.Models
{
    public class SubscriptionPlan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string PaymentGatewaySubscriptionPlanId { get; set; }
        public string RoleId { get; set; }
        public virtual IdentityRole Role { get; set; }
    }
}

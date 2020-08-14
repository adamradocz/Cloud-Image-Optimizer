using System;
using System.ComponentModel.DataAnnotations;

namespace ImageOptimizer.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string PaymentGatewaySubscriptionId { get; set; }

        public int SubscriptionPlanId { get; set; }
        public virtual SubscriptionPlan SubscriptionPlan { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; } = DateTime.UtcNow;
    }
}

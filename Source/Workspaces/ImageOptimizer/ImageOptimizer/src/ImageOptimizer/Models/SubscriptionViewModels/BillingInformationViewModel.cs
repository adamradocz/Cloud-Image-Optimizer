namespace ImageOptimizer.Models.SubscriptionViewModels
{
    public class BillingInformationViewModel : AddressBase
    {
        public AddressBase BillingAddress { get; set; }
        public string Country { get; set; }
    }
}

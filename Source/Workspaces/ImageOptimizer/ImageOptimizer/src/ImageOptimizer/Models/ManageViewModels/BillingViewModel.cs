namespace ImageOptimizer.Models.ManageViewModels
{
    public class BillingViewModel
    {
        public string PlanName { get; set; }
        public AddressBase BillingAddress { get; set; }
        public string Country { get; set; }
        public string Message { get; set; }
    }
}

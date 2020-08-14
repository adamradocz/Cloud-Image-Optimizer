namespace ImageOptimizer.Models
{
    public class AppSettings
    {
        public PaymentGatewaySettings PaymentGateway { get; set; }

        public class PaymentGatewaySettings
        {
            public BraintreeSettings Braintree { get; set; }

            public class BraintreeSettings
            {
                public string MerchantId { get; set; }
                public string PublicKey { get; set; }
                public string PrivateKey { get; set; }
            }
        }
    }
}

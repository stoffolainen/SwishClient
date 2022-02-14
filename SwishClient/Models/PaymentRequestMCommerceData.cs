namespace SwishClient.Models
{
    public class PaymentRequestMCommerceData
    {
        public string PayeePaymentReference { get; set; }
        public string CallbackUrl { get; set; }
        public string PayeeAlias { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string Message { get; set; }
    }
}

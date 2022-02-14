using System;

namespace SwishClient.Models
{
    public class CheckPaymentRequestStatusResponse
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string Id { get; set; }
        public string PayeePaymentReference { get; set; }
        public string PaymentReference { get; set; }
        public string CallbackUrl { get; set; }
        public string PayerAlias { get; set; }
        public string PayeeAlias { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DatePaid { get; set; }
    }
}

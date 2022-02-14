namespace SwishClient.Options
{
    public class ClientOptions
    {
        public string BaseUri { get; set; }
        public string PayeeAlias { get; set; }
        public string CallbackUrl { get; set; }
        public string PayeePaymentReference { get; set; }
        public string Currency { get; set; }
    }
}

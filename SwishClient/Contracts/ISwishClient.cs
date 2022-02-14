using SwishClient.Models;
using System.Threading.Tasks;

namespace SwishClient.Contracts
{
    public interface ISwishClient
    {
        Task<CheckPaymentRequestStatusResponse> CheckPaymentStatusAsync(string url);
        Task<PaymentRequestMCommerceResponse> MakePaymentRequestMCommerceAsync(int amount, string message);
        Task<PaymentRequestECommerceResponse> MakePaymentRequestAsync(string phonenumber, int amount, string message);
    }
}

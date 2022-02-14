using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SwishClient.Contracts;
using SwishClient.Models;
using SwishClient.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SwishClient.Clients
{
    public class SwishClient : ISwishClient
    {
        private readonly ClientOptions options;
        private readonly HttpClient client;

        public SwishClient(IOptions<ClientOptions> options, HttpClient client)
        {
            this.options = options.Value;
            this.client = client;
        }

        public async Task<CheckPaymentRequestStatusResponse> CheckPaymentStatusAsync(string url)
        {
            try
            {
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (!string.IsNullOrEmpty(content))
                    {
                        return new CheckPaymentRequestStatusResponse()
                        {
                            ErrorCode = "Error",
                            ErrorMessage = await response.Content.ReadAsStringAsync()
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return JsonConvert.DeserializeObject<CheckPaymentRequestStatusResponse>(content);
                }
            }
            catch (Exception ex)
            {
                return new CheckPaymentRequestStatusResponse()
                {
                    ErrorCode = "Exception",
                    ErrorMessage = ex.ToString()
                };
            }
        }

        public async Task<PaymentRequestECommerceResponse> MakePaymentRequestAsync(string phonenumber, int amount, string message)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(
                    new PaymentRequestECommerceData
                    {
                        PayeePaymentReference = options.PayeePaymentReference,
                        CallbackUrl = options.CallbackUrl,
                        PayerAlias = phonenumber,
                        PayeeAlias = options.PayeeAlias,
                        Amount = amount.ToString(),
                        Currency = options.Currency,
                        Message = message
                    }),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync($"{options.BaseUri}/swish-cpcapi/api/v1/paymentrequests", content);

                var errorMessage = string.Empty;
                var location = string.Empty;

                if (response.IsSuccessStatusCode)
                {
                    var headers = response.Headers.ToList();

                    if (headers.Any(x => x.Key == "Location"))
                    {
                        location = response.Headers.GetValues("Location").FirstOrDefault();
                    }
                }
                else
                {
                    errorMessage = await response.Content.ReadAsStringAsync();
                }

                return new PaymentRequestECommerceResponse()
                {
                    Error = errorMessage,
                    Location = location
                };
            }
            catch (Exception ex)
            {
                return new PaymentRequestECommerceResponse()
                {
                    Error = ex.ToString(),
                    Location = ""
                };
            }
        }
    }
}

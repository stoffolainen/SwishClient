using Newtonsoft.Json;
using SwishClient.Contracts;
using SwishClient.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SwishClient.Clients
{
    public class SwishClient : ISwishClient
    {
        private readonly string baseUri;
        private readonly string payeeAlias;
        private readonly string callbackUrl;
        private readonly string payeePaymentReference;
        private readonly string currency;

        private readonly HttpClient client;

        public SwishClient(
            string baseUri,
            string callbackUrl,
            string payeePaymentReference,
            string payeeAlias,
            string currency,
            HttpClient client)
        {            
            this.baseUri = baseUri;
            this.callbackUrl = callbackUrl;
            this.payeeAlias = payeeAlias;
            this.payeePaymentReference = payeePaymentReference;
            this.currency = currency;
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
                var content = new StringContent(JsonConvert.SerializeObject(new PaymentRequestECommerceData
                {
                    PayeePaymentReference = payeePaymentReference,
                    CallbackUrl = callbackUrl,
                    PayerAlias = phonenumber,
                    PayeeAlias = payeeAlias,
                    Amount = amount.ToString(),
                    Currency = currency,
                    Message = message
                }));

                var response = await client.PostAsync($"{baseUri}/swish-cpcapi/api/v1/paymentrequests", content);

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

        public async Task<PaymentRequestMCommerceResponse> MakePaymentRequestMCommerceAsync(int amount, string message)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(new PaymentRequestMCommerceData
                {
                    PayeePaymentReference = payeePaymentReference,
                    CallbackUrl = callbackUrl,
                    PayeeAlias = payeeAlias,
                    Amount = amount.ToString(),
                    Currency = currency,
                    Message = message
                }));

                var response = await client.PostAsync($"{baseUri}/swish-cpcapi/api/v1/paymentrequests", content);

                var errorMessage = string.Empty;
                var PaymentRequestToken = string.Empty;
                var Location = string.Empty;

                if (response.IsSuccessStatusCode)
                {
                    var headers = response.Headers.ToList();

                    if (headers.Any(x => x.Key == "PaymentRequestToken"))
                    {
                        PaymentRequestToken = response.Headers.GetValues("PaymentRequestToken").FirstOrDefault();
                    }

                    if (headers.Any(x => x.Key == "Location"))
                    {
                        Location = response.Headers.GetValues("Location").FirstOrDefault();
                    }
                }
                else
                {
                    errorMessage = await response.Content.ReadAsStringAsync();
                }

                return new PaymentRequestMCommerceResponse()
                {
                    Error = errorMessage,
                    Token = PaymentRequestToken,
                    Location = Location
                };
            }
            catch (Exception ex)
            {
                return new PaymentRequestMCommerceResponse()
                {
                    Error = ex.ToString(),
                    Token = "",
                    Location = ""
                };
            }
        }
    }
}

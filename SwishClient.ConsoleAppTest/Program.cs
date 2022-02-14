using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SwishClient.Contracts;
using System.Security.Cryptography.X509Certificates;

namespace SwishClient.ConsoleAppTest
{
    public class Program
    {
        private const string certificatePath = "\\TestCert\\Swish_Merchant_TestCertificate_1234679304.p12";
        private const string certificatePassword = "swish";

        private static ISwishClient swishClient;

        public static async Task Main(string[] args)
        {
            var host = BuildHost();
            swishClient = host.Services.GetRequiredService<ISwishClient>();

            var (succeeded, errorMessage) = await TestPaymentAsync();
            if (!succeeded)
            {
                Console.WriteLine(errorMessage);
            }
            else
            {
                Console.WriteLine("Test suceeded!");
            }

            Console.ReadLine();
        }

        private static async Task<(bool succeeded, string? errorMessage)> TestPaymentAsync()
        {
            var response = await swishClient.MakePaymentRequestAsync("1234679304", 1, "Test");
            if (string.IsNullOrEmpty(response?.Error))
            {
                Thread.Sleep(5000);

                var status = await swishClient.CheckPaymentStatusAsync(response?.Location);
                if (string.IsNullOrEmpty(status?.ErrorMessage))
                {
                    if (status?.Status == "PAID")
                    {
                        return (true, null);
                    }

                    return (false, status?.Status);
                }

                return (false, status?.ErrorMessage);
            }

            return (false, response?.Error);
        }

        private static IHost BuildHost() =>
            Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services = ConfigureServices(services);
            }).Build();

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<Options.ClientOptions>(x =>
            {
                x.PayeeAlias = "1234679304";
                x.PayeePaymentReference = "01234679304";
                x.CallbackUrl = "https://tabetaltmedswish.se/Test/Callback/"; ;
                x.BaseUri = "https://mss.cpc.getswish.net";
                x.Currency = "SEK";
            });

            services.AddHttpClient<ISwishClient, Clients.SwishClient>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return SetupHttpClientHandler();
                });

            return services;
        }

        private static HttpClientHandler SetupHttpClientHandler()
        {
            var httpClientHandler = new HttpClientHandler();

            using (var store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadWrite);

                var certs = new X509Certificate2Collection();
                certs.Import(Environment.CurrentDirectory + certificatePath, certificatePassword, X509KeyStorageFlags.DefaultKeySet);

                foreach (var cert in certs)
                {
                    if (cert.HasPrivateKey)
                    {
                        httpClientHandler.ClientCertificates.Add(cert);
                    }
                    else
                    {
                        store.Add(cert);
                    }
                }
            }

            return httpClientHandler;
        }
    }
}



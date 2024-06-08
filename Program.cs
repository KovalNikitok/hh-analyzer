using hh_analyzer.Abstractions;
using hh_analyzer.HttpClients;
using hh_analyzer.HttpClients.HttpClientsSettings;
using hh_analyzer.Services;

namespace hh_analyzer
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.Configure<HHApiSettings>(
                builder.Configuration.GetSection(nameof(HHApiSettings))
            );
            builder.Services.Configure<TakeJobOfferApiSettings>(
                builder.Configuration.GetSection(nameof(TakeJobOfferApiSettings))
            );

            builder.Services.AddHttpClient<IHHApiClient, HHApiClient>();
            builder.Services.AddHttpClient<ITakeJobOfferApiClient, TakeJobOfferApiClient>();

            builder.Services.AddHostedService<HHAnalyzerService>();
            builder.Services.AddHostedService<TakeJobOfferApiService>();

            var host = builder.Build();
            host.Run();
        }
    }
}
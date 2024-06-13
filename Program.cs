using hh_analyzer.Application;
using hh_analyzer.Application.Abstractions;
using hh_analyzer.Application.HttpClients;
using hh_analyzer.Application.HttpClients.HttpClientsSettings;
using hh_analyzer.Infrastructure;

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

            builder.Services.AddHttpClient<HHApiClient>();

            builder.Services.AddScoped<ITakeJobOfferApiClient, TakeJobOfferApiClient>();
            builder.Services.AddScoped<IHHAnalyzer, HHAnalyzer>();

            builder.Services.AddHostedService<HHBackgroundService>();

            var host = builder.Build();
            host.Run();
        }
    }
}
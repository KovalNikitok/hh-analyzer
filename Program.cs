using hh_analyzer.Application;
using hh_analyzer.Application.Abstractions;
using hh_analyzer.Infrastructure;
using hh_analyzer.Infrastructure.Settings;

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

            builder.Services.AddHttpClient();

            builder.Services.AddSingleton<IHHApiSerice, HHApiService>();
            builder.Services.AddSingleton<ITakeJobOfferApiService, TakeJobOfferApiService>();

            builder.Services.AddHostedService<HHBackgroundService>();

            var host = builder.Build();
            host.Run();
        }
    }
}
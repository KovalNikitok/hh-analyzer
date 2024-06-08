
namespace hh_analyzer.Services
{
    public class TakeJobOfferApiService : BackgroundService
    {
        private readonly ILogger<HHAnalyzerService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public TakeJobOfferApiService(ILogger<TakeJobOfferApiService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        // TODO:
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool isInfoLogLevelEnabled = _logger.IsEnabled(LogLevel.Information);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (isInfoLogLevelEnabled)
                    _logger.LogInformation("[{time}] TakeJobOfferApiService running...", DateTimeOffset.Now);

                // Setting a pause with time offset on 12 hours
                await Task.Delay(new TimeSpan( 12, 0, 0), stoppingToken);
            }
        }
    }
}

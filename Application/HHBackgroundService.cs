using hh_analyzer.Application.Abstractions;

namespace hh_analyzer.Application
{
    public class HHBackgroundService : BackgroundService
    {
        private readonly ILogger<HHBackgroundService> _logger;
        private readonly ITakeJobOfferApiClient _takeJobOfferApiClient;
        private readonly IHHAnalyzer _hhAnalyzer;

        public HHBackgroundService(ILogger<HHBackgroundService> logger,
            ITakeJobOfferApiClient takeJobOfferApiClient,
            IHHAnalyzer hhAnalyzer)
        {
            _logger = logger;
            _takeJobOfferApiClient = takeJobOfferApiClient;
            _hhAnalyzer = hhAnalyzer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool isInfoLogLevelEnabled = _logger.IsEnabled(LogLevel.Information);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (isInfoLogLevelEnabled)
                    _logger.LogInformation("[{time}] HHAnalyzerService running...", DateTimeOffset.Now);

                // Setting a pause with time offset on 1 day
                await Task.Delay(new TimeSpan(1, 0, 0, 0), stoppingToken);
            }
        }
    }
}

using Microsoft.Extensions.Options;
using System.Net.Http;

namespace hh_analyzer.Services
{
    public class HHAnalyzerService : BackgroundService
    {
        private readonly ILogger<HHAnalyzerService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HHAnalyzerService(ILogger<HHAnalyzerService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
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

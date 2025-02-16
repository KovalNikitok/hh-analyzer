namespace hh_analyzer.Infrastructure.Settings
{
    /// <summary>
    /// Базовые настройки для HH API
    /// </summary>
    public class HHApiSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string Agent { get; set; } = string.Empty;
    }
}

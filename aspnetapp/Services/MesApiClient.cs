using System.Text;
using System.Text.Json;
using aspnetapp.Models.Mes;

namespace aspnetapp.Services
{
    public class MesApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MesApiClient> _logger;
        private readonly MesApiSettings _settings;
        private readonly SemaphoreSlim _concurrencyLimiter = new SemaphoreSlim(1, 1);
        private int _requestsThisSecond;
        private DateTime _windowStart = DateTime.UtcNow;
        private const int MaxRequestsPerSecond = 10;

        public MesApiClient(HttpClient httpClient, ILogger<MesApiClient> logger, MesApiSettings settings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settings = settings;

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Clear();
        }

        private async Task<bool> TryAcquireRateLimitAsync(CancellationToken cancellationToken)
        {
            await _concurrencyLimiter.WaitAsync(cancellationToken);
            try
            {
                var now = DateTime.UtcNow;
                if ((now - _windowStart).TotalSeconds >= 1)
                {
                    _windowStart = now;
                    _requestsThisSecond = 0;
                }

                if (_requestsThisSecond >= MaxRequestsPerSecond)
                    return false;

                _requestsThisSecond++;
                return true;
            }
            finally
            {
                _concurrencyLimiter.Release();
            }
        }

        public async Task<MesApiResult<List<ProcedureReportWriteBackParams>>?> QueryProcedureReportByTimeAsync(
            string startTime, string endTime, CancellationToken cancellationToken = default)
        {
            if (!await TryAcquireRateLimitAsync(cancellationToken))
            {
                _logger.LogWarning("MES API rate limit (10 QPS) reached, skipping request.");
                return null;
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post,
                    "/lightmesapi/open/workorderController/selectProceduresReportDataByTime");

                request.Headers.Add("AccessKeyId", _settings.AccessKeyId);
                request.Headers.Add("AccessKeySecret", _settings.AccessKeySecret);

                var body = new SelectByDataTimeDTO { StartTime = startTime, EndTime = endTime };
                var json = JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<MesApiResult<List<ProcedureReportWriteBackParams>>>(
                    responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying MES API for time range {StartTime} - {EndTime}", startTime, endTime);
                return null;
            }
        }
    }

    public class MesApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string AccessKeyId { get; set; } = string.Empty;
        public string AccessKeySecret { get; set; } = string.Empty;
    }
}


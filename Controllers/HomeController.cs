using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using weather_app_aspnet.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace weather_app_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IDistributedCache _cache;

    public HomeController(
        ILogger<HomeController> logger,
        HttpClient httpClient,
        IConfiguration configuration,
        IDistributedCache cache)
    {
        _logger = logger;
        _httpClient = httpClient;
        _apiKey = configuration["WeatherApikey"];
        _cache = cache;
    }
    // 👇 Add route binding here
    [HttpGet("{zipCode}")]
    public async Task<IActionResult> GetApiData(string zipCode)
    {
        string cacheKey = $"weather:{zipCode}";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Cache hit for {ZipCode}", zipCode);

            var data = JsonSerializer.Deserialize<WeatherApiResponseModel>(cachedData);
            return Ok(data);
        }
        string url = $"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{zipCode}?key={_apiKey}";
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<WeatherApiResponseModel>(jsonResponse, options);

            if (data == null)
            {
                _logger.LogWarning("Deserialized weather data was null for {Url}", url);
                return NotFound("Weather data not found");
            }
            await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(data),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            return Ok(data); // ✅ returns JSON automatically
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP request to {Url} failed.", url);
            return StatusCode(503, "Error calling weather API.");
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON deserialization failed for response from {Url}.", url);
            return StatusCode(500, "Error parsing weather data.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while calling {Url}.", url);
            return StatusCode(500, "Unexpected server error.");
        }
    }
}

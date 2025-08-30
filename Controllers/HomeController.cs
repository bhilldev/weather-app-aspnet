using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using weather_app_aspnet.Models;

namespace weather_app_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public HomeController(ILogger<HomeController> logger, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _apiKey = configuration["WeatherApikey"];
    }

    // 👇 Add route binding here
    [HttpGet("{zipCode}")]
    public async Task<IActionResult> GetApiData(string zipCode)
    {
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

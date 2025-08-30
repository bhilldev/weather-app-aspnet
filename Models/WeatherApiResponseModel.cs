namespace weather_app_aspnet.Models;

public class WeatherApiResponseModel
{
    public int QueryCost { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string ResolvedAddress { get; set; }
    public string Address { get; set; }
    public string Timezone { get; set; }
    public List<DayForecast> Days { get; set; }
}

public class DayForecast
{
    public string Datetime { get; set; }
    public double Tempmax { get; set; }
    public double Tempmin { get; set; }
    public double Temp { get; set; }
    public double Humidity { get; set; }
    public string Conditions { get; set; }
}


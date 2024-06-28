using Microsoft.AspNetCore.Mvc;

namespace IGSample.Server.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class TestAPIController : ControllerBase
  {
    public class WeatherForecast
    {
      public DateTime Date { get; set; }
      public int TemperatureC { get; set; }
      public string? Summary { get; set; }
      public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
        => Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
          Date = DateTime.Now.AddDays(index),
          TemperatureC = Random.Shared.Next(-20, 55),
          Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
  }
}

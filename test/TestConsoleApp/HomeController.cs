using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OneHttpClient;

namespace TestConsoleApp
{
    public class Post
    {
        public int UserId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpService _http;

        public HomeController(ILogger<HomeController> logger, IHttpService http)
        {
            _logger = logger;
            _http = http;
        }

        [HttpGet("/test")]
        public IActionResult Test()
        {
            _logger.LogInformation("Testing OneHttpClient...");

            Task.WaitAll(
                _http.GetAsync("https://jsonplaceholder.typicode.com/posts/1"),
                _http.PostAsync("https://jsonplaceholder.typicode.com/posts/1"),
                _http.PutAsync("https://jsonplaceholder.typicode.com/posts/1"),
                _http.PatchAsync("https://jsonplaceholder.typicode.com/posts/1"),
                _http.DeleteAsync("https://jsonplaceholder.typicode.com/posts/1")
            );

            return Ok();
        }

        [HttpGet("/times")]
        public async Task<IActionResult> TestSequential(int count = 10)
        {
            //string url = "http://jsonplaceholder.typicode.com/posts/1";
            string url = "https://api-staging-cadu.stone.com.br/membership/version";

            // Warmup
            var warmupResponse = await _http.GetAsync(url);

            // Max out the iteration count
            if (count > 50) { count = 50; }
            var stopwatch = new Stopwatch();

            // Make calls
            var elapsedTimes = new Dictionary<double, double>();
            for (int i = 0; i < count; i++)
            {
                stopwatch.Restart();
                var response = await _http.GetAsync(url);
                stopwatch.Stop();

                elapsedTimes.Add(response.ElapsedTime.TotalMilliseconds, stopwatch.Elapsed.TotalMilliseconds);
            }

            // Calculate overheads
            var overheads = new List<double>();
            foreach (var elapsedTime in elapsedTimes)
            {
                overheads.Add(elapsedTime.Value - elapsedTime.Key);
            }

            return Ok(new
            {
                warmup = warmupResponse.ElapsedTime.TotalMilliseconds,
                elapsedTimes = elapsedTimes,
                averageOverhead = (overheads.Any()) ? overheads.Average() : 0
            });
        }
    }
}

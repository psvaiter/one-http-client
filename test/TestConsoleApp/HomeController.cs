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
                _http.Get("https://jsonplaceholder.typicode.com/posts/1"),
                _http.Post("https://jsonplaceholder.typicode.com/posts/1"),
                _http.Put("https://jsonplaceholder.typicode.com/posts/1"),
                _http.Patch("https://jsonplaceholder.typicode.com/posts/1"),
                _http.Delete("https://jsonplaceholder.typicode.com/posts/1")
            );

            return Ok();
        }

        [HttpGet("/times")]
        public async Task<IActionResult> TestTimes(int iterationCount = 10)
        {
            // Warmup
            await _http.Get("https://jsonplaceholder.typicode.com/posts");

            // Max out the iteration count
            if (iterationCount > 50)
            {
                iterationCount = 50;
            }

            // Make calls
            var elapsedTimes = new Dictionary<double, double>();
            for (int i = 0; i < iterationCount; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await _http.Get("https://jsonplaceholder.typicode.com/posts");
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
                elapsedTimes = elapsedTimes,
                averageOverhead = overheads.Average()
            });
        }
    }
}

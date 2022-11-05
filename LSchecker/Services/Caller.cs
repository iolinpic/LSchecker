using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LSchecker.Services
{
    public class Caller : ICaller
    {
        private HttpClient _sharedClient;
        private ILogger _logger;

        private string _token = "321",
        _channel = "123";

        public Caller(IConfiguration configuration, ILoggerFactory lf)
        {
            _sharedClient = new();
            _channel = configuration.GetSection("channel").Value;
            _token = configuration.GetSection("token").Value;
            _logger = lf.CreateLogger<Caller>();
        }

        public async Task<HttpStatusCode> GetAsync(string url)
        {
            var response = await _sharedClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return response.StatusCode;
        }

        public async Task SendNotificationAsync(string message)
        {
            var res = await _sharedClient.GetAsync($"https://api.telegram.org/bot{_token}/sendMessage?chat_id={_channel}&text={message}");
        }
    }
}
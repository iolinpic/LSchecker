using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LSchecker.Services
{
    public class Caller : ICaller
    {
        private HttpClient _sharedClient;

        private string _token = "321",
        _channel = "123";

        private class Message
        {
            public string chat_id { get; set; }
            public string text { get; set; }
        }

        public Caller(IConfiguration configuration)
        {
            _sharedClient = new();
            _channel = configuration.GetSection("channel").Value;
            _token = configuration.GetSection("token").Value;
        }

        public async Task<HttpStatusCode> GetAsync(string url)
        {
            var response = await _sharedClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return response.StatusCode;
        }

        public async Task<bool> SendNotificationAsync(string message)
        {
            var json = JsonSerializer.Serialize(new Message { text = message, chat_id = _channel });
            var res = await _sharedClient.PostAsync($"https://api.telegram.org/bot{_token}/sendMessage", new StringContent(json, Encoding.UTF8, "application/json"));
            return res.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> SendReportFileAsync(string filename, string message)
        {
            MultipartFormDataContent form = new MultipartFormDataContent();
            form.Add(new StringContent(_channel), name: "chat_id");
            form.Add(new StringContent(message), name: "caption");
            form.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(filename)), "document", "report.xlsx");
            var res = await _sharedClient.PostAsync($"https://api.telegram.org/bot{_token}/sendDocument", form);
            return res.StatusCode == HttpStatusCode.OK;
        }
    }
}
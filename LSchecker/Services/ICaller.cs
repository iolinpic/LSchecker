using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LSchecker.Services
{
    public interface ICaller
    {
        public Task<HttpStatusCode> GetAsync(string url);

        public Task<bool> SendNotificationAsync(string message);

        public Task<bool> SendReportFileAsync(string filename, string message);
    }
}
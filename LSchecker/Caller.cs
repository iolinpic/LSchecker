using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LSchecker
{
    public class Caller
    {
        private static HttpClient sharedClient = new();

        private static string token = "924997149:AAGRGDurCwfv80OjUvo4U8VDaepENieZV8Y",
        channel = "@neksys_server_fail";

        public static async Task<System.Net.HttpStatusCode> GetAsync(string url)
        {
            using HttpResponseMessage response = await sharedClient.GetAsync(url);

            response.EnsureSuccessStatusCode();
            return response.StatusCode;
        }

        public static async Task SendTGMessage(string message)
        {
            await sharedClient.GetAsync($"https://api.telegram.org/bot{token}/sendMessage?chat_id={channel}&text={message}");
            //if (res.StatusCode == HttpStatusCode.OK)
            //{ Console.WriteLine("tgSendSucces"); }
            //else
            //{ Console.WriteLine(res.StatusCode); }
        }
    }
}
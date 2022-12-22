using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;


namespace App.Framework
{
    public class Message
    {
        public String data { get; set; }
        public String signature { get; set; }
        public uint counter { get; set; }
    }
    public class MessageResponse
    {
        public String signature { get; set; }
        public uint counter { get; set; }
        public Body body { get; set; }
    }
    public class Body
    {
        public String content { get; set; }
        public String signature { get; set; }
    }
    public class BlockchainAdapter
    {
        private async Task<String> Post(string url, Message message)
        {
            var httpClient = new ServiceCollection().AddHttpClient().BuildServiceProvider().GetService<IHttpClientFactory>().CreateClient();

            httpClient.Timeout = TimeSpan.FromSeconds(100);
            var json = JsonConvert.SerializeObject(message);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, data);
            var str = await response.Content.ReadAsStringAsync();
            return str;
        }
        public async Task<string> PostSign(Message message)
        {
            string url = "http://8.210.55.139:85/sign";
            return await Post(url, message);
        }
        public async Task<string> PostUpload(Message message)
        {
            string url = "http://8.210.55.139:86/upload";
            return await Post(url, message);
        }
    }
}

using App.Framework;
using App.IActor;
using Flurl;
using Flurl.Http;
using Lib;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;

namespace App.WebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private IClientFactory clientFactory;
        public HomeController(ILogger<HomeController> logger, IClientFactory clientFactory)
        {
            this.logger = logger;
            this.clientFactory = clientFactory;
        }

        public async Task<IActionResult> Index()
        {
            //var client = clientFactory.GetClient();
            //var actor = client.GetGrain<IHello>(0);
            //var result = await actor.SayHello("Kiwi");
            //return Content(result);
            var client = clientFactory.GetClient();
            var actor = client.GetGrain<IAccount>("1");
            var balance = await actor.Read();
            return Content("balance:" + balance);

        }
        public async Task<IActionResult> Write()
        {
            //var client = clientFactory.GetClient();
            //var actor = client.GetGrain<IHello>(0);
            //var result = await actor.SayHello("Kiwi");
            //return Content(result);
            var client = clientFactory.GetClient();
            var actor = client.GetGrain<IAccount>("1");
            await actor.Write();
            return Content($"{DateTime.Now.ToShortTimeString()}:done");
        }
        public async Task<IActionResult> WriteWithId(string userId)
        {
            var client = clientFactory.GetClient();
            var actor = client.GetGrain<IAccount>(userId);
            await actor.Write();
            return Content($"{DateTime.Now.ToShortTimeString()}:done");
        }
        public async Task<IActionResult> WriteWithHttp()
        {
            var msg = new Message()
            {
                data = "123",
                signature = string.Empty,
                counter = 0
            };
            var str = await "http://8.210.55.139:85".AppendPathSegment("sign").PostJsonAsync(msg).ReceiveString();
            return Content($"{DateTime.Now.ToShortTimeString()}:done");
        }
    }
}
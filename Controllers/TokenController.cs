using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Controllers
{
    [Route("token/directline")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        [HttpGet]
        public  async Task<string> GetAsync()
        {
            var secret = System.Environment.GetEnvironmentVariable("DIRECTLINE_SECRET");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secret);
            var content = new StringContent("{\"User\" : {\"Id\": \"dl_ABCD\"}}", Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://directline.botframework.com/v3/directline/tokens/generate", content);
            return await response.Content.ReadAsStringAsync();
        }
    }

    [Route("token/directlinease")]
    [ApiController]
    public class TokenController2 : ControllerBase
    {
        [HttpGet]
        public  async Task<string> GetAsync()
        {
            var secret = System.Environment.GetEnvironmentVariable("DIRECTLINE_SECRET");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secret);
            var content = new StringContent("{\"User\" : {\"Id\": \"dl_ABCD\"}}", Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://myasebot.azurewebsites.net/.bot/v3/directline/tokens/generate", content);
            return await response.Content.ReadAsStringAsync();
        }
    }
}